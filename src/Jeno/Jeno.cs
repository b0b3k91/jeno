using Jeno.Commands;
using Jeno.Core;
using Jeno.Interfaces;
using Jeno.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var container = new ServiceCollection()
                .Configure<JenoConfiguration>(configuration.GetSection("jeno"))
                .AddHttpClient()
                .AddSingleton(PhysicalConsole.Singleton)
                .AddSingleton(serializer)
                .AddSingleton<IPasswordProvider, PasswordProvider>()
                .AddSingleton<IGitClient, GitClient>()
                .AddSingleton<IConfigurationSerializer, ConfigurationSerializer>()
                .AddTransient<IJenoCommand, RunJob>()
                .AddTransient<IJenoCommand, ChangeConfiguration>()
                .AddTransient<IJenoCommand, ShowHelp>()
                .AddTransient<IJenoCommand, ShowConfiguration>()
                .BuildServiceProvider();

            var app = new CommandLineApplication
            {
                Name = "jeno",
                Description = "Jeno is a simple command line interface used to manage Jenkins jobs",
            };

            app.OnExecuteAsync(async token =>
            {
                Console.WriteLine(app.Description);
                Console.WriteLine("Use \"jeno help\" command to get more information");
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
                Console.WriteLine(ex.Message);
                return ex.ExitCode;
            }
        }
    }
}