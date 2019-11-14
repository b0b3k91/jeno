using Jeno.Core;
using Jeno.Interfaces;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Jeno.Commands
{
    public class ShowConfiguration : IJenoCommand
    {
        public string Name => "config";

        public Action<CommandLineApplication> Command { get; }

        private readonly JenoConfiguration _configuration;
        private readonly IConsole _console;
        private readonly ISerializer _serializer;

        public ShowConfiguration(ISerializer serializer, IOptions<JenoConfiguration> configuration, IConsole console)
        {
            _configuration = configuration.Value;
            _console = console;
            _serializer = serializer;

            Command = (app) =>
            {
                app.Description = "Show Jeno configuration";

                app.OnExecuteAsync(async token =>
                {
                    _console.WriteLine(_serializer.Serialize(_configuration));
                    return JenoCodes.Ok;
                });
            };
        }
    }
}
