using Jeno.Core;
using Jeno.Interfaces;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Jeno.Commands
{
    public class RunJob : IJenoCommand
    {
        private readonly string _defaulJobKey = "default";

        private readonly IGitWrapper _gitWrapper;
        private readonly HttpClient _client;
        private readonly JenoConfiguration _configuration;

        public string Name => "run";
        public Action<CommandLineApplication> Command { get; }

        public RunJob(IGitWrapper gitWrapper, IHttpClientFactory clientFactory, IOptions<JenoConfiguration> configuration)
        {
            _gitWrapper = gitWrapper;
            _client = clientFactory.CreateClient();
            _configuration = configuration.Value;


            Command = (CommandLineApplication app) =>
            {
                app.Description = "Run job on Jenkins";

                app.OnExecuteAsync(async token =>
                {
                    var messageBuilder = new StringBuilder();

                    if (!_configuration.Repositories.ContainsKey(_defaulJobKey))
                    {
                        throw new JenoException("Default job for ");
                    }

                    //Validate jenkins address by creating new Uri instance
                    try
                    {
                        new Uri(_configuration.JenkinsUrl);
                    }
                    catch (UriFormatException)
                    {
                        messageBuilder.AppendLine("Jenkins address is undefined or incorrect");
                        messageBuilder.AppendLine("Use \"jeno set jenkinsUrl:[url]\" command to save correct Jenkins address");
                        throw new JenoException(messageBuilder.ToString());
                    }

                    var baseUrl = new Uri(_configuration.JenkinsUrl);

                    if (string.IsNullOrEmpty(_configuration.Token))
                    {
                        if (string.IsNullOrEmpty(_configuration.Username))
                        {

                            messageBuilder.AppendLine("Username is undefined");
                            messageBuilder.AppendLine("Use \"jeno set username:[username]\" command to save login");
                            throw new JenoException(messageBuilder.ToString());
                        }

                        var configurationUrl = new Uri(baseUrl, $"user/{_configuration.Username}/configure");

                        messageBuilder.AppendLine("User token is undefined");
                        messageBuilder.AppendLine($"Token can be generated on {configurationUrl.AbsoluteUri}");
                        messageBuilder.AppendLine("Use \"jeno set token:[token]\" command to save authorization token");
                        throw new JenoException(messageBuilder.ToString());
                    }

                    var currentRepo = await _gitWrapper.GetRepoUrl(Directory.GetCurrentDirectory());
                    var jobNumber = await _gitWrapper.GetCurrentBranch(Directory.GetCurrentDirectory());

                    var pipeline = _configuration.Repositories.ContainsKey(currentRepo)
                            ? _configuration.Repositories[currentRepo]
                            : _configuration.Repositories[_defaulJobKey];

                    if (string.IsNullOrEmpty(pipeline))
                    {
                        throw new JenoException("Cannot find chosen pipeline in configuration");
                    }

                    var jobUrl = new Uri(baseUrl, $"job/{pipeline}/job/{jobNumber}");

                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration.Token);
                    var response = await _client.PostAsync(jobUrl, null);

                    return response.StatusCode == HttpStatusCode.OK ? JenoCodes.Ok : JenoCodes.DefaultError;
                });
            };
        }
    }
}