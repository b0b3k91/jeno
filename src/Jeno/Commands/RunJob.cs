using Jeno.Core;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Jeno.Commands
{
    class RunJob : IJenoCommand
    {
        private readonly string _tokenKey = "token";
        private readonly string _baseUrlKey = "jenkinsUrl";
        private readonly string _userKey = "login";

        private readonly string _defaulJobtKey = "default";

        private readonly IGitWrapper _gitWrapper;
        private readonly IConfiguration _configuration;
        private readonly IConsole _console;
        private readonly HttpClient _client;

        public string Name => "run";
        public Action<CommandLineApplication> Command { get; }

        public RunJob(IGitWrapper gitWrapper, HttpClient client, IConfiguration configuration, IConsole console)
        {
            _gitWrapper = gitWrapper;
            _client = client;
            _configuration = configuration;
            _console = console;

            Command = (CommandLineApplication app) =>
            {
                app.Description = "Run job on Jenkins";

                app.OnExecute(() =>
                {

                    //Validate jenkins address by creating new Uri instance
                    try
                    {
                        new Uri(_configuration[_baseUrlKey]);
                    }
                    catch(ArgumentNullException)
                    {
                        _console.WriteLine("Jenkins address is undefined");
                        _console.WriteLine($"Use \"jeno set {_baseUrlKey}={{url}}\" command to save correct Jenkins address");
                        return;
                    }
                    catch (UriFormatException)
                    {
                        _console.WriteLine("Jenkins address is incorrect");
                        _console.WriteLine($"Use \"jeno set {_baseUrlKey}={{url}}\" command to save correct Jenkins address");
                        return;
                    }

                    var baseUrl = new Uri(_configuration[_baseUrlKey]);
                    var token = _configuration[_tokenKey];

                    if (string.IsNullOrEmpty(token))
                    {
                        var user = _configuration[_userKey];

                        if (string.IsNullOrEmpty(user))
                        {
                            _console.WriteLine("Login is undefined");
                            _console.WriteLine($"Use \"jeno set {_userKey}={{login}}\" command to save correct Jenkins address");
                            return;
                        }

                        var configurationUrl = new Uri(baseUrl, $"user/{user}/configure");

                        _console.WriteLine("User token is undefined");
                        _console.WriteLine($"To generate token go to {configurationUrl.AbsoluteUri}");
                        _console.WriteLine($"Use \"jeno set {_tokenKey}={{token}}\" command to save correct Jenkins address");
                        return;
                    }

                    var repositories = _configuration.GetSection("repositories")
                       .AsEnumerable()
                       .Where(s => !string.IsNullOrEmpty(s.Value))
                       .ToDictionary(s => s.Key.Replace("repositories:", string.Empty), s => s.Value);

                    var currentRepo = gitWrapper.GetRepoUrl(Directory.GetCurrentDirectory());
                    var jobNumber = _gitWrapper.GetCurrentBranch(Directory.GetCurrentDirectory());

                    if (currentRepo.Contains("fatal") || jobNumber.Contains("fatal"))
                    {
                        _console.WriteLine(currentRepo.Replace("fatal", "Error"));
                        return;
                    }

                    var pipeline = repositories.ContainsKey(currentRepo)
                            ? repositories[currentRepo]
                            : repositories[_defaulJobtKey];

                    if (string.IsNullOrEmpty(pipeline))
                    {
                        _console.WriteLine("Cannot find pipeline name in configuration");
                        return;
                    }

                    var jobUrl = new Uri(baseUrl, $"job/{pipeline}/job/{jobNumber}");

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    try
                    {
                        client.PostAsync(jobUrl, null);
                    }
                    catch(Exception ex)
                    {
                        _console.WriteLine(ex.Message);
                    }
                });
            };
        }
    }
}
