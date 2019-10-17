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
            var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

            var container = new ServiceCollection()
                .Configure<JenoConfiguration>(configuration.GetSection("jeno"))
                .AddHttpClient()
                .AddSingleton(PhysicalConsole.Singleton)
                .AddSingleton<IGitWrapper, GitWrapper>()
                .AddSingleton<IConfigurationSerializer, ConfigurationSerializer>()
                .AddTransient<IJenoCommand, RunJob>()
                .AddTransient<IJenoCommand, ChangeConfiguration>()
                .AddTransient<IJenoCommand, ShowHelp>()
                .BuildServiceProvider();

            var console = container.GetService<IConsole>();

            var app = new CommandLineApplication
            {
                Name = "jeno",
                Description = "Jeno is a simple command line interface used to manage Jenkins jobs",
            };

            app.OnExecute(() =>
            {
                console.WriteLine(app.Description);
                console.WriteLine("Use \"jeno help\" command to get more information");
                return 0;
            });

            var jenoCommands = container.GetServices<IJenoCommand>();

            foreach (var jenoCommand in jenoCommands)
            {
                app.Command(jenoCommand.Name, jenoCommand.Command);
            }

            try
            {
                return app.Execute(args);
            }
            catch(JenoException ex)
            {
                console.WriteLine($"Error: {ex.Message}");
                return ex.ExitCode;
            }
        }
    }
}
