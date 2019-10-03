using Jeno.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Jeno.Commands
{
    class RunJob : ICommand
    {
        public string Name { get; }

        public Action<string> Command { get; }

        public RunJob(GitWrapper gitWrapper, HttpClient client, IConfiguration configuration)
        {
            Name = "run";
            var defaultKey = "default";

            var token = configuration.GetSection("authentication")["token"];
            var repositories = configuration.GetSection("repositories")
                .AsEnumerable()
                .ToDictionary(s => s.Key, s => s.Value);

            var currentDir = System.IO.Directory.GetCurrentDirectory();

            Command = async (args) =>
            {
                var currentRepo = gitWrapper.GetRepoUrl(currentDir);
                var pipelineUrl = repositories.ContainsKey(currentRepo) ? repositories[currentRepo] : repositories[defaultKey];
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                await client.PostAsync(pipelineUrl, null);
            };
        }
    }
}
