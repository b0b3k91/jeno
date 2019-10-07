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
        private readonly string _defaultKey = "defaultKey";
        private readonly string _tokenKey = "token";
        private readonly string _baseUrlKey = "baseUrl";

        private readonly IGitWrapper _gitWrapper;
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;

        public string Name => "run";

        public Action<CommandLineApplication> Command { get; }

        public RunJob(IGitWrapper gitWrapper, HttpClient client, IConfiguration configuration)
        {
            _gitWrapper = gitWrapper;
            _client = client;
            _configuration = configuration;

            Command = (CommandLineApplication app) =>
            {
                app.OnExecute(() =>
                {
                   var token = configuration[_tokenKey];
                   var baseUrl = new Uri(configuration[_baseUrlKey]);

                   var repositories = configuration.GetSection("repositories")
                       .AsEnumerable()
                       .Where(s => !string.IsNullOrEmpty(s.Value))
                       .ToDictionary(s => s.Key, s => s.Value);

                   var currentDir = Directory.GetCurrentDirectory();

                   var currentRepo = gitWrapper.GetRepoUrl(currentDir);

                   if (currentRepo.Contains("fatal"))
                   {
                       return;
                   }

                   var pipelineUrl = repositories.ContainsKey(currentRepo) ? repositories[currentRepo] : repositories[_defaultKey];
                   client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                   client.PostAsync(pipelineUrl, null);
                });
            };
        }
    }
}
