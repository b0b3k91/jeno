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
using Jeno.Validators;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Jeno.Commands
{
    public class RunJob : IJenoCommand
    {
        private const string DefaulJobKey = "default";
        private readonly HttpClient _client;
        private readonly JenoConfiguration _configuration;
        private readonly IEncryptor _encryptor;
        private readonly IGitClient _gitWrapper;
        private readonly IUserConsole _userConsole;
        private readonly ConfigurationValidator _validator = new ConfigurationValidator();

        public Action<CommandLineApplication> Command { get; }
        public string Name => "run";

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
                    var validationResult = _validator.Validate(_configuration);
                    if (!validationResult.IsValid)
                    {
                        throw new JenoException(string.Join(Environment.NewLine, validationResult.Errors));
                    }

                    var baseUrl = new Uri(_configuration.JenkinsUrl);

                    if (!await _gitWrapper.IsGitRepository(Directory.GetCurrentDirectory()))
                    {
                        throw new JenoException(Messages.NotGitRepoError);
                    }

                    var currentRepo = await _gitWrapper.GetRepoName(Directory.GetCurrentDirectory());
                    var jobNumber = await _gitWrapper.GetCurrentBranch(Directory.GetCurrentDirectory());

                    var pipeline = _configuration.Repository.ContainsKey(currentRepo) ?
                            _configuration.Repository[currentRepo] :
                            _configuration.Repository[DefaulJobKey];

                    var jobUrl = new Uri(baseUrl, $"{pipeline}/job/{jobNumber}/buildWithParameters");

                    if (jobParameters.Values.Count > 0)
                    {
                        ValidateJobParameters(jobParameters.Values);

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

        private void ValidateJobParameters(IEnumerable<string> parameters)
        {
            if (parameters.Any(s => !s.Contains("=")))
            {
                var invalidParameters = string.Join(", ", parameters.Where(s => !s.Contains("=")));

                var message = new StringBuilder();
                message.AppendLine(Messages.IncorrectJobParameters);
                message.AppendLine(invalidParameters);

                throw new JenoException(message.ToString());
            }
        }
    }
}