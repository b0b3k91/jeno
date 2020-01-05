using Jeno.Core;
using McMaster.Extensions.CommandLineUtils;
using System;

namespace Jeno.Commands
{
    public class ShowHelp : IJenoCommand
    {
        public string Name => "help";

        public Action<CommandLineApplication> Command => app =>
        {
            app.Description = Messages.HelpCommandDescription;

            app.OnExecuteAsync(async token =>
            {
                app.Parent.ShowHelp();
                return JenoCodes.Ok;
            });
        };
    }
}