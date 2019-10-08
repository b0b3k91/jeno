using Jeno.Core;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using System;

namespace Jeno.Commands
{
    class ChangeConfiguration : IJenoCommand
    {

        private readonly IConsole _console;

        public string Name => "set";
        public Action<CommandLineApplication> Command { get; }

        public ChangeConfiguration(IOptions<JenoConfiguration> configuration, IConsole console)
        {
            _console = console;

            Command = (app) =>
            {
                app.Description = "Set parameter in app configuration";

                app.OnExecute(() =>
                {
                    _console.WriteLine("Beep beep!");
                    configuration.Value.Token = "5om3r4nd0mt0k3n";
                });
            };
        }

    }
}
