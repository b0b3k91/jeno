using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Jeno.Core;
using Jeno.Infrastructure;
using Jeno.Interfaces;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Jeno.Commands
{
    public class RunJob : IJenoCommand
    {
        private readonly string _defaulJobKey = "default";

        private readonly IGitClient _gitWrapper;
        private readonly HttpClient _client;
        private readonly JenoConfiguration _configuration;
        private readonly IUserConsole _userConsole;
        private readonly IEncryptor _encryptor;

        public string Name => "run";
        public Action<CommandLineApplication> Command { get; }

        public RunJob(IGitClient gitWrapper, IEncryptor encryptor, IUserConsole userConsole, IHttpClientFactory factory, IOptions<JenoConfiguration> configuration)
        {
            _gitWrapper = gitWrapper;
            _client = factory.CreateClient();
            _configuration = configuration.Value;
            _userConsole = userConsole;
            _encryptor = encryptor;

            Command = (CommandLineApplication app) =>
            {
                app.Description = Messages.RunJobDescription;

                var jobParameters = app.Argument("jobParameters", Messages.RunJobArgumentsDescription, true);

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
                        throw new JenoException(Messages.NotGitRepo);
                    }

                    var currentRepo = await _gitWrapper.GetRepoName(Directory.GetCurrentDirectory());
                    var jobNumber = await _gitWrapper.GetCurrentBranch(Directory.GetCurrentDirectory());

                    var pipeline = _configuration.Repository.ContainsKey(currentRepo) ?
                            _configuration.Repository[currentRepo] :
                            _configuration.Repository[_defaulJobKey];

                    var jobUrl = new Uri(baseUrl, $"{pipeline}/job/{jobNumber}/buildWithParameters");

                    if (jobParameters.Values.Count > 0)
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

                    if (response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        var password = string.IsNullOrWhiteSpace(_configuration.Password) ?
                            _userConsole.ReadInput("password", true) :
                            _encryptor.Decrypt(_configuration.Password);

                        _client.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeader(_configuration.UserName, password);

                        var crumbUrl = new Uri(baseUrl, "crumbIssuer/api/json");
                        var crumbResponse = await _client.GetAsync(crumbUrl);

                        if (!crumbResponse.IsSuccessStatusCode)
                        {
                            throw new JenoException($"{Messages.CsrfException}: {crumbResponse.ReasonPhrase}");
                        }

                        var crumbHeader = JsonConvert.DeserializeObject<CrumbHeader>(await crumbResponse.Content.ReadAsStringAsync());
                        _client.DefaultRequestHeaders.Add(crumbHeader.CrumbRequestField, crumbHeader.Crumb);
                        response = await _client.PostAsync(jobUrl, null);
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new JenoException($"{Messages.JobException}: {response.ReasonPhrase}");
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
                messageBuilder.AppendLine(Messages.IncorrectJenkinsAddress);
                messageBuilder.AppendLine(Messages.ConfigureJenkinsAddressTip);
            }

            if (!_configuration.Repository.ContainsKey(_defaulJobKey))
            {
                messageBuilder.AppendLine(Messages.MissingDefaultJob);
                messageBuilder.AppendLine(Messages.ConfigureDefaultJobTip);
            }

            if (string.IsNullOrEmpty(_configuration.UserName))
            {
                messageBuilder.AppendLine(Messages.UndefinedUserName);
                messageBuilder.AppendLine(Messages.ConfigureUserNameTip);
            }

            if (string.IsNullOrEmpty(_configuration.Token))
            {
                var configurationUrl = new Uri(new Uri(_configuration.JenkinsUrl), $"user/{_configuration.UserName}/configure");

                messageBuilder.AppendLine(Messages.UndefinedToken);
                messageBuilder.AppendLine($"{Messages.JenkinsConfigurationAddressTip} {configurationUrl.AbsoluteUri}");
                messageBuilder.AppendLine(Messages.ConfigureTokenTip);
            }

            return messageBuilder.Length > 0 ? Result.Invalid(messageBuilder.ToString()) : Result.Ok();
        }

        private Result ValidateJobParameters(IEnumerable<string> parameters)
        {
            if (parameters.Any(s => !s.Contains("=")))
            {
                var invalidParameters = string.Join(", ", parameters.Where(s => !s.Contains("=")));

                var message = new StringBuilder();
                message.AppendLine(Messages.IncorrectJobParameters);
                message.AppendLine(invalidParameters);

                return Result.Invalid(message.ToString());
            }

            return Result.Ok();
        }
    }
}