using System;
using System.Threading.Tasks;
using Jeno.Core;
using Jeno.Interfaces;
using Jeno.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Jeno
{
    internal class Program
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
                .AddSingleton(serializer)
                .AddSingleton<IUserConsole, UserConsole>()
                .AddSingleton<IEncryptor, Encryptor>()
                .AddSingleton<IGitClient, GitClient>()
                .AddSingleton<IConfigurationSerializer, ConfigurationSerializer>()
                .Scan(scan => scan.FromCallingAssembly()
                                .AddClasses(c => c.AssignableTo<IJenoCommand>())
                                .AsImplementedInterfaces()
                                .WithTransientLifetime())
                .BuildServiceProvider();

            var app = new CommandLineApplication
            {
                Name = "jeno",
                Description = Messages.JenoDescription,
            };

            app.OnExecuteAsync(token =>
            {
                Console.WriteLine(app.Description);
                Console.WriteLine(Messages.BasicMessage);
                return Task.FromResult(JenoCodes.Ok);
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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
                return ex.ExitCode;
            }
        }
    }
}