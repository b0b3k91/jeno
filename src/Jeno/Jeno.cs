using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Jeno.Core;
using Jeno.Services;
using Jeno.Commands;
using System.Net.Http;
using McMaster.Extensions.CommandLineUtils;
using System.Linq;

namespace Jeno
{
    class Jeno
    {
        public static int Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

            var container = new ServiceCollection()
                .AddSingleton<IConsole>(PhysicalConsole.Singleton)
                .AddSingleton<IGitWrapper, GitWrapper>()
                .AddSingleton<HttpClient>()
                .AddSingleton<IConfiguration>(config)
                .AddTransient<IJenoCommand, RunJob>()
                .AddTransient<IJenoCommand, ChangeConfiguration>()
                .BuildServiceProvider();

            var app = new CommandLineApplication
            {
                Name = "Jeno",
                Description = "CLI Jenkins manager",
            };

            app.OnExecute(() =>
            {
                app.ShowHelp();
                return 1;
            });

            var jenoCommands = container.GetServices<IJenoCommand>();

            foreach (var jenoCommand in jenoCommands)
            {
                app.Command(jenoCommand.Name, jenoCommand.Command);
            }

            return app.Execute(args);
        }
    }
}
