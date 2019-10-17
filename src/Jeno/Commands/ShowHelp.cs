using Jeno.Core;
using McMaster.Extensions.CommandLineUtils;
using System;

namespace Jeno.Commands
{
    internal class ShowHelp : IJenoCommand
    {
        public string Name => "help";

        public Action<CommandLineApplication> Command => app =>
        {
            app.Description = "List of app features and available commands";

            app.OnExecute(() =>
            {
                app.Parent.ShowHelp();
            });
        };
    }
}