using Jeno.Core;
using Jeno.Services;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Jeno.Commands
{
    class RunJob : IJenoCommand
    {
        public string Name => "run";

        public Action<CommandLineApplication> Command { get; }

        public RunJob(GitWrapper gitWrapper, HttpClient client, IConfiguration configuration)
        {

            var defaultKey = "default";

            var token = configuration.GetSection("authentication")["token"];

            var repositories = configuration.GetSection("repositories")
                .AsEnumerable()
                .ToDictionary(s => s.Key, s => s.Value);

            var currentDir = System.IO.Directory.GetCurrentDirectory();

            Command = async (CommandLineApplication app) =>
            {
                var currentRepo = gitWrapper.GetRepoUrl(currentDir);
                var pipelineUrl = repositories.ContainsKey(currentRepo) ? repositories[currentRepo] : repositories[defaultKey];
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.PostAsync(pipelineUrl, null);
            };
        }
    }
}
