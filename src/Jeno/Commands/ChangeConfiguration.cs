using System;
using Microsoft.Extensions.Options;
using McMaster.Extensions.CommandLineUtils;
using Jeno.Core;
using System.Linq;
using System.Collections.Generic;

namespace Jeno.Commands
{
    class ChangeConfiguration : IJenoCommand
    {
        private readonly IConsole _console;
        private readonly IConfigurationSerializer _serializer;

        public string Name => "set";
        public Action<CommandLineApplication> Command { get; }

        public ChangeConfiguration(IConfigurationSerializer serializer, IConsole console)
        {
            _console = console;
            _serializer = serializer;

            Command = (app) =>
            {
                app.Description = "Set options of app configuration";

                var settings = app.Argument("options", "List of options to save in app configuration", true).Values;
                var deleteOption = app.Option("-d|--delete", "Remove passed repository", CommandOptionType.NoValue);

                app.OnExecute(() =>
                {
                    if (settings.Any(s => !s.Contains(':')))
                    {
                        var incorrectOptions = string.Join(", ", settings.Where(s => !s.Contains(':')).ToArray());
                        _console.WriteLine("Some of passed options have unhandled format:");
                        _console.WriteLine(incorrectOptions);

                        return;
                    }

                    var configuration = _serializer.ReadConfiguration();

                    var args = settings
                        .Where(s => s.Contains(':'))
                        .ToDictionary(s => s.Split(':')[0], s => s.Split(':')[1]) ?? new Dictionary<string, string>();

                    foreach (var arg in args)
                    {
                        switch (arg.Key)
                        {
                            case "jenkinsUrl":
                                _console.WriteLine($"Change token to: {arg.Value}");
                                configuration.JenkinsUrl = arg.Value;
                                break;

                            case "username":
                                _console.WriteLine($"Change token to: {arg.Value}");
                                configuration.Username = arg.Value;
                                break;

                            case "token":
                                _console.WriteLine($"Change token to: {arg.Value}");
                                configuration.Token = arg.Value;
                                break;

                            case "repository":
                                {
                                    var repositories = arg.Value.Split(',');

                                    if (Convert.ToBoolean(deleteOption.Value()))
                                    {
                                        foreach (var repository in repositories)
                                        {
                                            if (configuration.Repositories.ContainsKey(repository))
                                                configuration.Repositories.Remove(repository);
                                        }

                                        break;
                                    }

                                    if(repositories.Any(s => !s.Contains('=')))
                                    {
                                        _console.WriteLine("Some of passed repositores have unhandled format");
                                        break;
                                    }

                                    var repoDictionary = repositories.ToDictionary(s => s.Split('=')[0], s => s.Split('=')[1]);

                                    if (repoDictionary.Keys.Any(s => string.IsNullOrWhiteSpace(s)))
                                    {
                                        _console.WriteLine("Some of passed repositories have undefined origin address");
                                        break;
                                    }

                                    foreach(var repoKeyValue in repoDictionary)
                                    {
                                        configuration.Repositories[repoKeyValue.Key] = repoKeyValue.Value;
                                    }
                                    break;
                                }

                            default:
                                _console.WriteLine($"Unsupported parameter: {arg.Key}");
                                break;
                        }
                    }

                    _serializer.SaveConfiguration(configuration);
                });
            };
        }
    }
}
