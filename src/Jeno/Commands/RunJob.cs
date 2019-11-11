using Jeno.Core;
using Jeno.Infrastructure;
using Jeno.Interfaces;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Jeno.Commands
{
    public class RunJob : IJenoCommand
    {
        private readonly string _defaulJobKey = "default";

        private readonly IGitWrapper _gitWrapper;
        private readonly HttpClient _client;
        private readonly JenoConfiguration _configuration;
        private readonly IPasswordProvider _passwordProvider;

        public string Name => "run";
        public Action<CommandLineApplication> Command { get; }

        public RunJob(IGitWrapper gitWrapper, IPasswordProvider passwordProvider, HttpClient client, IOptions<JenoConfiguration> configuration)
        {
            _gitWrapper = gitWrapper;
            _client = client;
            _configuration = configuration.Value;
            _passwordProvider = passwordProvider;

            Command = (CommandLineApplication app) =>
            {
                app.Description = "Run job on Jenkins";

                app.OnExecuteAsync(async token =>
                {
                    ValidateConfiguration();

                    var baseUrl = new Uri(_configuration.JenkinsUrl);

                    var currentRepo = await _gitWrapper.GetRepoUrl(Directory.GetCurrentDirectory());
                    var jobNumber = await _gitWrapper.GetCurrentBranch(Directory.GetCurrentDirectory());

                    var pipeline = _configuration.Repositories.ContainsKey(currentRepo)
                            ? _configuration.Repositories[currentRepo]
                            : _configuration.Repositories[_defaulJobKey];

                    var jobUrl = new Uri(baseUrl, $"job/{pipeline}/{jobNumber}/buildWithParameters");


                    if (!_configuration.UseWindowsCredentials)
                    {
                        _client.DefaultRequestHeaders.Authorization = new BearerAuthenticationHeader(_configuration.Token);
                    }

                    var response = await _client.PostAsync(jobUrl, null);

                    if (response.StatusCode == HttpStatusCode.Forbidden && response.ReasonPhrase.Contains("No valid crumb"))
                    {
                        if (!_configuration.UseWindowsCredentials) 
                        {
                            var password = _passwordProvider.GetPassword();
                            _client.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeader(_configuration.Username, password);
                        }

                        var crumbUrl = new Uri(baseUrl, "crumbIssuer/api/json");
                        var crumbResponse = await _client.GetAsync(crumbUrl);

                        if(!crumbResponse.IsSuccessStatusCode)
                        {
                            throw new JenoException("Cannot get crumb for CSRF protection system");
                        }

                        var crumb = JsonConvert.DeserializeObject<CrumbResponse>(await crumbResponse.Content.ReadAsStringAsync());
                        _client.DefaultRequestHeaders.Add(crumb.CrumbRequestField, crumb.Crumb);
                        response = await _client.PostAsync(jobUrl, null);
                    }

                    return response.IsSuccessStatusCode ? JenoCodes.Ok : JenoCodes.DefaultError;
                });
            };
        }

        private void ValidateConfiguration()
        {
            var messageBuilder = new StringBuilder();

            if (!Uri.IsWellFormedUriString(_configuration.JenkinsUrl, UriKind.Absolute))
            {
                messageBuilder.AppendLine("Jenkins address is undefined or incorrect");
                messageBuilder.AppendLine("Use \"jeno set jenkinsUrl:[url]\" command to save correct Jenkins address");
                throw new JenoException(messageBuilder.ToString());
            }

            if (!_configuration.Repositories.ContainsKey(_defaulJobKey))
            {
                messageBuilder.AppendLine("Missing default job");
                messageBuilder.AppendLine("Use \"jeno set repository:default=[defaultJob]\" command to save default job");
                throw new JenoException(messageBuilder.ToString());
            }

            if (string.IsNullOrEmpty(_configuration.Username))
            {
                messageBuilder.AppendLine("Username is undefined");
                messageBuilder.AppendLine("Use \"jeno set username:[username]\" command to save login");
                throw new JenoException(messageBuilder.ToString());
            }

            if (string.IsNullOrEmpty(_configuration.Token))
            {
                var configurationUrl = new Uri(new Uri(_configuration.JenkinsUrl), $"user/{_configuration.Username}/configure");

                messageBuilder.AppendLine("User token is undefined");
                messageBuilder.AppendLine($"Token can be generated on {configurationUrl.AbsoluteUri}");
                messageBuilder.AppendLine("Use \"jeno set token:[token]\" command to save authorization token");
                throw new JenoException(messageBuilder.ToString());
            }
        }
    }
}