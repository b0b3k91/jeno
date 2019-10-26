using Jeno.Core;
using Jeno.Interfaces;
using McMaster.Extensions.CommandLineUtils;
using System;

namespace Jeno.Commands
{
    public class ShowHelp : IJenoCommand
    {
        public string Name => "help";

        public Action<CommandLineApplication> Command => app =>
        {
            app.Description = "List of app features and available commands";

            app.OnExecuteAsync(async token =>
            {
                app.Parent.ShowHelp();
                return JenoCodes.Ok;
            });
        };
    }
}