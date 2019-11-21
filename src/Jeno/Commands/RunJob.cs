using Jeno.Core;
using Jeno.Infrastructure;
using Jeno.Interfaces;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Jeno.Commands
{
    public class RunJob : IJenoCommand
    {
        private readonly string _defaulJobKey = "default";

        private readonly IGitClient _gitWrapper;
        private readonly HttpClient _client;
        private readonly JenoConfiguration _configuration;
        private readonly IPasswordProvider _passwordProvider;

        public string Name => "run";
        public Action<CommandLineApplication> Command { get; }

        public RunJob(IGitClient gitWrapper, IPasswordProvider passwordProvider, IHttpClientFactory factory, IOptions<JenoConfiguration> configuration)
        {
            _gitWrapper = gitWrapper;
            _client = factory.CreateClient();
            _configuration = configuration.Value;
            _passwordProvider = passwordProvider;

            Command = (CommandLineApplication app) =>
            {
                app.Description = "Run job on Jenkins";

                var jobParameters = app.Argument("jobParameters", "List of job parameters", true);

                app.OnExecuteAsync(async token =>
                {
                    var validationResult = ValidateConfiguration();
                    if (!validationResult.IsSuccess)
                    {
                        throw new JenoException(validationResult.ErrorMessage);
                    }

                    var baseUrl = new Uri(_configuration.JenkinsUrl);

                    if (!await _gitWrapper.IsGitRepository(Directory.GetCurrentDirectory()))
                    {
                        throw new JenoException("Current directory is not git repository.");
                    }

                    var currentRepo = await _gitWrapper.GetRepoUrl(Directory.GetCurrentDirectory());
                    var jobNumber = await _gitWrapper.GetCurrentBranch(Directory.GetCurrentDirectory());

                    var pipeline = _configuration.Repositories.ContainsKey(currentRepo)
                            ? _configuration.Repositories[currentRepo]
                            : _configuration.Repositories[_defaulJobKey];

                    var jobUrl = new Uri(baseUrl, $"job/{pipeline}/{jobNumber}/buildWithParameters");

                    if(jobParameters.Values.Count > 0)
                    {
                        var validateParameters = ValidateJobParameters(jobParameters.Values);
                        if (!validateParameters.IsSuccess)
                        {
                            throw new JenoException(validateParameters.ErrorMessage);
                        }

                        var builder = new UriBuilder(jobUrl);
                        builder.Query = string.Join("&", jobParameters.Values);
                        jobUrl = builder.Uri;
                    }

                    _client.DefaultRequestHeaders.Authorization = new BearerAuthenticationHeader(_configuration.Token);

                    var response = await _client.PostAsync(jobUrl, null);

                    if (response.StatusCode == HttpStatusCode.Forbidden && response.ReasonPhrase.Contains("No valid crumb"))
                    {
                        var password = _passwordProvider.GetPassword();
                        _client.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeader(_configuration.UserName, password);

                        var crumbUrl = new Uri(baseUrl, "crumbIssuer/api/json");
                        var crumbResponse = await _client.GetAsync(crumbUrl);

                        if (!crumbResponse.IsSuccessStatusCode)
                        {
                            throw new JenoException($"Cannot get crumb for CSRF protection system: {crumbResponse.ReasonPhrase}");
                        }

                        var crumbHeader = JsonConvert.DeserializeObject<CrumbHeader>(await crumbResponse.Content.ReadAsStringAsync());
                        _client.DefaultRequestHeaders.Add(crumbHeader.CrumbRequestField, crumbHeader.Crumb);
                        response = await _client.PostAsync(jobUrl, null);
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new JenoException($"Cannot run job: {response.ReasonPhrase}");
                    }

                    return JenoCodes.Ok;
                });
            };
        }

        private Result ValidateConfiguration()
        {
            var messageBuilder = new StringBuilder();

            if (!Uri.IsWellFormedUriString(_configuration.JenkinsUrl, UriKind.Absolute))
            {
                messageBuilder.AppendLine("Jenkins address is undefined or incorrect");
                messageBuilder.AppendLine("Use \"jeno set jenkinsUrl:[url]\" command to save correct Jenkins address");
            }

            if (!_configuration.Repositories.ContainsKey(_defaulJobKey))
            {
                messageBuilder.AppendLine("Missing default job");
                messageBuilder.AppendLine("Use \"jeno set repository:default=[defaultJob]\" command to save default job");
            }

            if (string.IsNullOrEmpty(_configuration.UserName))
            {
                messageBuilder.AppendLine("Username is undefined");
                messageBuilder.AppendLine("Use \"jeno set userName:[username]\" command to save login");
            }

            if (string.IsNullOrEmpty(_configuration.Token))
            {
                var configurationUrl = new Uri(new Uri(_configuration.JenkinsUrl), $"user/{_configuration.UserName}/configure");

                messageBuilder.AppendLine("User token is undefined");
                messageBuilder.AppendLine($"Token can be generated on {configurationUrl.AbsoluteUri}");
                messageBuilder.AppendLine("Use \"jeno set token:[token]\" command to save authorization token");
            }

            return messageBuilder.Length > 0 ? Result.Invalid(messageBuilder.ToString()) : Result.Ok();
        }

        private Result ValidateJobParameters(IEnumerable<string> parameters)
        {
            if(parameters.Any(s => !s.Contains("=")))
            {
                var invalidParameters = string.Join(", ", parameters.Where(s => !s.Contains("=")));

                var message = new StringBuilder();
                message.AppendLine("Some of job parameters have incorrect format");
                message.AppendLine(invalidParameters);

                return Result.Invalid(message.ToString());
            }

            return Result.Ok();
        }
    }
}