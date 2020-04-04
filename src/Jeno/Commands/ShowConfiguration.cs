using System;
using System.Threading.Tasks;
using Jeno.Core;
using Jeno.Interfaces;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using YamlDotNet.Serialization;

namespace Jeno.Commands
{
    public class ShowConfiguration : IJenoCommand
    {
        public string Name => "config";

        public Action<CommandLineApplication> Command { get; }

        private readonly JenoConfiguration _configuration;
        private readonly IUserConsole _console;
        private readonly ISerializer _yamlSerializer;

        public ShowConfiguration(ISerializer yamlSerializer, IOptions<JenoConfiguration> configuration, IUserConsole console)
        {
            _configuration = configuration.Value;
            _console = console;
            _yamlSerializer = yamlSerializer;

            Command = (app) =>
            {
                app.Description = Messages.ShowConfigurationCommandDescription;

                app.OnExecuteAsync(token =>
                {
                    _console.WriteLine(_yamlSerializer.Serialize(_configuration));
                    return Task.FromResult(JenoCodes.Ok);
                });
            };
        }
    }
}