using Jeno.Core;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Jeno.Commands
{
    internal class RunJob : IJenoCommand
    {
        private readonly string _defaulJobKey = "default";

        private readonly IGitWrapper _gitWrapper;
        private readonly IConsole _console;
        private readonly HttpClient _client;
        private readonly JenoConfiguration _configuration;

        public string Name => "run";
        public Action<CommandLineApplication> Command { get; }

        public RunJob(IGitWrapper gitWrapper, IHttpClientFactory clientFactory, IOptions<JenoConfiguration> configuration, IConsole console)
        {
            _gitWrapper = gitWrapper;
            _client = clientFactory.CreateClient();
            _configuration = configuration.Value;
            _console = console;

            Command = (CommandLineApplication app) =>
            {
                app.Description = "Run job on Jenkins";

                app.OnExecuteAsync(async cancellationToken =>
                {
                    //Validate jenkins address by creating new Uri instance
                    try
                    {
                        new Uri(_configuration.JenkinsUrl);
                    }
                    catch (UriFormatException)
                    {
                        _console.WriteLine("Jenkins address is undefined or incorrect");
                        _console.WriteLine("Use \"jeno set jenkinsUrl:[url]\" command to save correct Jenkins address");
                        return 1;
                    }

                    var baseUrl = new Uri(_configuration.JenkinsUrl);

                    if (string.IsNullOrEmpty(_configuration.Token))
                    {
                        if (string.IsNullOrEmpty(_configuration.Username))
                        {
                            _console.WriteLine("Username is undefined");
                            _console.WriteLine($"Use \"jeno set username:[username]\" command to save login");
                            return 1;
                        }

                        var configurationUrl = new Uri(baseUrl, $"user/{_configuration.Username}/configure");

                        _console.WriteLine("User token is undefined");
                        _console.WriteLine($"Token can be generated on {configurationUrl.AbsoluteUri}");
                        _console.WriteLine($"Use \"jeno set token:[token]\" command to save authorization token");
                        return 1;
                    }

                    var currentRepo = await _gitWrapper.GetRepoUrl(Directory.GetCurrentDirectory());
                    var jobNumber = await _gitWrapper.GetCurrentBranch(Directory.GetCurrentDirectory());

                    var pipeline = _configuration.Repositories.ContainsKey(currentRepo)
                            ? _configuration.Repositories[currentRepo]
                            : _configuration.Repositories[_defaulJobKey];

                    if (string.IsNullOrEmpty(pipeline))
                    {
                        _console.WriteLine("Cannot find chosen pipeline in configuration");
                        return 1;
                    }

                    var jobUrl = new Uri(baseUrl, $"job/{pipeline}/job/{jobNumber}");

                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration.Token);
                    var response = await _client.PostAsync(jobUrl, null);

                    return response.StatusCode == HttpStatusCode.OK ? 0 : 1;
                });
            };
        }
    }
}