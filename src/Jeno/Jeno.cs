using Jeno.Commands;
using Jeno.Core;
using Jeno.Interfaces;
using Jeno.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;

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
                .AddHttpClient(client => 
                    client.ConfigurePrimaryHttpMessageHandler(() =>
                    {
                        return new HttpClientHandler()
                        {
                            UseDefaultCredentials = true,
                            Credentials = CredentialCache.DefaultNetworkCredentials
                        };
                    }))
                .AddSingleton<IGitWrapper, GitWrapper>()
                .AddSingleton<IConfigurationSerializer, ConfigurationSerializer>()
                .AddTransient<IJenoCommand, RunJob>()
                .AddTransient<IJenoCommand, ChangeConfiguration>()
                .AddTransient<IJenoCommand, ShowHelp>()
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

            foreach (var jenoCommand in container.GetServices<IJenoCommand>())
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