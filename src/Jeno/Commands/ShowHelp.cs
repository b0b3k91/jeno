using Jeno.Core;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Threading.Tasks;

namespace Jeno.Commands
{
    internal class ShowHelp : IJenoCommand
    {
        public string Name => "help";

        public Action<CommandLineApplication> Command => app =>
        {
            app.Description = "List of app features and available commands";

            app.OnExecuteAsync(async cancellationToken =>
            {
                app.Parent.ShowHelp();
                return 1;
            });
        };
    }
}