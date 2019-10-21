using Jeno.Commands;
using Jeno.Core;
using Jeno.Interfaces;
using Jeno.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Jeno
{
    internal class Jeno
    {
        public static async Task<int> Main(string[] args)
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

            app.OnExecuteAsync(async token =>
            {
                console.WriteLine(app.Description);
                console.WriteLine("Use \"jeno help\" command to get more information");
                return JenoCodes.Ok;
            });

            var jenoCommands = container.GetServices<IJenoCommand>();

            foreach (var jenoCommand in jenoCommands)
            {
                app.Command(jenoCommand.Name, jenoCommand.Command);
            }

            try
            {
                return await app.ExecuteAsync(args);
            }
            catch (JenoException ex)
            {
                console.WriteLine($"Error: {ex.Message}");
                return ex.ExitCode;
            }
        }
    }
}