using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Jeno.Core;
using Jeno.Services;
using Jeno.Commands;

namespace Jeno
{
    class Jeno
    {
        static void Main(string[] args)
        {
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configuration.json");

            var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(configPath, optional: true, reloadOnChange: true)
                    .Build();

            var container = new ServiceCollection()
                .AddSingleton<IGitWrapper, GitWrapper>()
                .AddTransient<IJenoCommand, RunJob>()
                .AddTransient<IJenoCommand, ChangeConfiguration>()
                .Configure<IConfiguration>(config)
                .BuildServiceProvider();

            var app = new CommandLineApplication(throwOnUnexpectedArg: false);

            var jenoCommands = container.GetServices<IJenoCommand>();

            foreach(var jenoCommand in jenoCommands)
            {
                app.Command(jenoCommand.Name, jenoCommand.Command);
            }

            app.Execute(args);
        }
    }
}
