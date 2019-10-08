using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Jeno.Core;
using Jeno.Services;
using Jeno.Commands;
using System.Net.Http;
using McMaster.Extensions.CommandLineUtils;

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
                .Configure<JenoConfiguration>(config.GetSection("jeno"))
                .AddHttpClient()
                .AddSingleton(PhysicalConsole.Singleton)
                .AddSingleton<IGitWrapper, GitWrapper>()
                .AddTransient<IJenoCommand, RunJob>()
                .AddTransient<IJenoCommand, ChangeConfiguration>()
                .AddTransient<IJenoCommand, ShowHelp>()
                .BuildServiceProvider();

            var app = new CommandLineApplication
            {
                Name = "jeno",
                Description = "Jeno is a simple command line interface used to manage Jenkins jobs",
            };

            app.OnExecute(() =>
            {
                var console = container.GetService<IConsole>();
                console.WriteLine(app.Description);
                console.WriteLine("Use \"jeno help\" command to get more information");
                return 0;
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
