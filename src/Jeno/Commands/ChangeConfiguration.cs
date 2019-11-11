using Jeno.Core;
using Jeno.Interfaces;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jeno.Commands
{
    public class ChangeConfiguration : IJenoCommand
    {
        private readonly IConfigurationSerializer _serializer;

        public string Name => "set";
        public Action<CommandLineApplication> Command { get; }

        public ChangeConfiguration(IConfigurationSerializer serializer)
        {
            _serializer = serializer;

            Command = (app) =>
            {
                app.Description = "Set options of app configuration";

                var settings = app.Argument("options", "List of options to save in app configuration", true).Values;
                var deleteOption = app.Option("-d|--delete", "Remove passed repository", CommandOptionType.NoValue);

                app.OnExecuteAsync(async token =>
                {
                    if (settings.Any(s => !s.Contains(':')))
                    {
                        var incorrectSettings = string.Join(", ", settings.Where(s => !s.Contains(':')).ToArray());

                        var messageBuilder = new StringBuilder();
                        messageBuilder.AppendLine("Some of passed options have unhandled format:");
                        messageBuilder.AppendLine(incorrectSettings);

                        throw new JenoException(messageBuilder.ToString());
                    }

                    var configuration = await _serializer.ReadConfiguration();

                    var args = settings
                        .Where(s => s.Contains(':'))
                        .Select(s => ParseSetting(s))
                        .ToDictionary(k => k.Key, v =>  v.Value) ?? new Dictionary<string, string>();

                    foreach (var arg in args)
                    {
                        switch (arg.Key)
                        {
                            case "jenkinsurl":
                                configuration.JenkinsUrl = arg.Value;
                                break;

                            case "username":
                                configuration.Username = arg.Value;
                                break;

                            case "token":
                                configuration.Token = arg.Value;
                                break;

                            case "repository":
                                {
                                    var repositories = arg.Value.Split(',');

                                    if (deleteOption.Values.Count > 0)
                                    {
                                        if (repositories.Any(s => s == "default"))
                                            throw new JenoException("Cannot remove default job");

                                        foreach (var repository in repositories)
                                        {
                                            if (configuration.Repositories.ContainsKey(repository))
                                                configuration.Repositories.Remove(repository);
                                        }

                                        break;
                                    }

                                    if (repositories.Any(s => !s.Contains('=')))
                                    {
                                        throw new JenoException("Some of passed repositories have unhandled format");
                                    }

                                    var repoDictionary = repositories.ToDictionary(s => s.Split('=')[0], s => s.Split('=')[1]);

                                    if (repoDictionary.Keys.Any(s => string.IsNullOrWhiteSpace(s)))
                                    {
                                        throw new JenoException("Some of passed repositories have undefined origin address");
                                    }

                                    foreach (var repoKeyValue in repoDictionary)
                                    {
                                        configuration.Repositories[repoKeyValue.Key] = repoKeyValue.Value;
                                    }
                                    break;
                                }

                            default:
                                throw new JenoException($"Unsupported parameter: {arg.Key}");
                        }
                    }

                    await _serializer.SaveConfiguration(configuration);
                    return JenoCodes.Ok;
                });
            };
        }

        private KeyValuePair<string, string> ParseSetting(string setting)
        {
            //only first colon should be treated like separator,
            //others (like the ones in url addresses) must be ignored.
            return new KeyValuePair<string, string>
                (
                    key: setting.Split(':').First().ToLower(),
                    value: string.Join(':', setting.Split(':').Skip(1))
                );
        }
    }
}