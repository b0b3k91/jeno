using System;
using System.Threading.Tasks;
using Jeno.Core;
using McMaster.Extensions.CommandLineUtils;

namespace Jeno.Commands
{
    public class ShowHelp : IJenoCommand
    {
        public string Name => "help";

        public Action<CommandLineApplication> Command => app =>
        {
            app.Description = Messages.HelpCommandDescription;

            app.OnExecuteAsync(token =>
            {
                app.Parent.ShowHelp();
                return Task.FromResult(JenoCodes.Ok);
            });
        };
    }
}