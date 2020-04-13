using System;
using System.IO;
using System.Net;
using System.Net.Http;
using Jeno.Core;
using Jeno.Infrastructure;
using Jeno.Interfaces;
using Jeno.Validators;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Jeno.Commands
{
    public class ScanPipeline : IJenoCommand
    {
        private const string DefaulJobKey = "default";
        private readonly IGitClient _gitWrapper;
        private readonly HttpClient _client;
        private readonly JenoConfiguration _configuration;
        private readonly IUserConsole _userConsole;
        private readonly IEncryptor _encryptor;
        private readonly ConfigurationValidator _validator = new ConfigurationValidator();

        public string Name => "scan";

        public Action<CommandLineApplication> Command { get; }

        public ScanPipeline(IGitClient gitWrapper, IEncryptor encryptor, IUserConsole userConsole, IHttpClientFactory factory, IOptions<JenoConfiguration> configuration)
        {
            _gitWrapper = gitWrapper;
            _client = factory.CreateClient();
            _configuration = configuration.Value;
            _userConsole = userConsole;
            _encryptor = encryptor;

            Command = (CommandLineApplication app) =>
            {
                app.Description = string.Empty;

                app.OnExecuteAsync(async token =>
                {
                    var validationResult = _validator.Validate(_configuration);
                    if (!validationResult.IsValid)
                    {
                        throw new JenoException(string.Join(Environment.NewLine, validationResult.Errors));
                    }

                    if (!await _gitWrapper.IsGitRepository(Directory.GetCurrentDirectory()))
                    {
                        throw new JenoException(Messages.NotGitRepoError);
                    }

                    var currentRepo = await _gitWrapper.GetRepoName(Directory.GetCurrentDirectory());
                    
                    var baseUrl = new Uri(_configuration.JenkinsUrl);

                    var pipeline = _configuration.Repository.ContainsKey(currentRepo) ?
                            _configuration.Repository[currentRepo] :
                            _configuration.Repository[DefaulJobKey];

                    var pipelineUrl = new Uri(baseUrl, $"{pipeline}/build?delay=0");

                    _client.DefaultRequestHeaders.Authorization = new BearerAuthenticationHeader(_configuration.Token);

                    var response = await _client.PostAsync(pipelineUrl, null);

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
                        response = await _client.PostAsync(pipelineUrl, null);
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new JenoException($"{Messages.ScanException}: {response.ReasonPhrase}");
                    }

                    return JenoCodes.Ok;
                });
            };
        }
    }
}
