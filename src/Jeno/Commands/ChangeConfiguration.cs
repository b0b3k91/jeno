using Jeno.Core;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using System;

namespace Jeno.Commands
{
    class ChangeConfiguration : IJenoCommand
    {
        public string Name => "set";

        public Action<CommandLineApplication> Command { get; }

        public ChangeConfiguration(IConfiguration configuration)
        {
            Command = (app) =>
            {
                string key = "token";
                string value = "5om3r4nd0mt0k3n";
                configuration[key] = value;
            };
        }

    }
}
