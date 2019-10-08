using Jeno.Core;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jeno.Commands
{
    class ShowHelp : IJenoCommand
    {
        public string Name => "help";

        public Action<CommandLineApplication> Command => app =>
        {
            app.Description = "List of app options and available commands";

            app.OnExecute(() =>
            {
                app.Parent.ShowHelp();
            });
        };
    }
}
