﻿using Jeno.Core;
using Jeno.Interfaces;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jeno.Commands
{
    public class SetConfiguration : IJenoCommand
    {
        private readonly IConfigurationSerializer _serializer;
        private readonly IEncryptor _encryptor;

        public string Name => "set";
        public Action<CommandLineApplication> Command { get; }

        public SetConfiguration(IConfigurationSerializer serializer, IEncryptor encryptor)
        {
            _serializer = serializer;
            _encryptor = encryptor;

            Command = (app) =>
            {
                app.Description = Messages.ChangeConfigurationDescription;

                var settings = app.Argument("settings", Messages.SettingDescription, true).Values;
                var deleteOption = app.Option("-d|--delete", Messages.DeleteRepoOptionDescription, CommandOptionType.NoValue);

                app.OnExecuteAsync(async token =>
                {
                    var validationResult = ValidateSettings(settings);

                    if (!validationResult.IsSuccess)
                    {
                        throw new JenoException(validationResult.ErrorMessage);
                    }

                    var configuration = await _serializer.ReadConfiguration();

                    var args = settings
                        .Where(s => s.Contains(':'))
                        .Select(s => ParseSetting(s))
                        .ToDictionary(k => k.Key, v => v.Value) ?? new Dictionary<string, string>();

                    foreach (var arg in args)
                    {
                        switch (arg.Key)
                        {
                            case "jenkinsurl":
                                configuration.JenkinsUrl = arg.Value;
                                break;

                            case "username":
                                configuration.UserName = arg.Value;
                                break;

                            case "token":
                                configuration.Token = arg.Value;
                                break;

                            case "password":
                                configuration.Password = _encryptor.Encrypt(arg.Value);
                                break;

                            case "repository":
                                {
                                    var repositories = arg.Value.Split(',');

                                    if (deleteOption.Values.Count > 0)
                                    {
                                        if (repositories.Any(s => s == "default"))
                                            throw new JenoException(Messages.RemoveDefaultJobException);

                                        foreach (var repository in repositories)
                                        {
                                            if (configuration.Repositories.ContainsKey(repository))
                                                configuration.Repositories.Remove(repository);
                                        }

                                        break;
                                    }

                                    if (repositories.Any(s => !s.Contains('=')))
                                    {
                                        throw new JenoException(Messages.WrongReposFormat);
                                    }

                                    var repoDictionary = repositories.ToDictionary(s => s.Split('=')[0], s => s.Split('=')[1]);

                                    if (repoDictionary.Keys.Any(s => string.IsNullOrWhiteSpace(s)))
                                    {
                                        throw new JenoException(Messages.MissingRepoName);
                                    }

                                    foreach (var repoKeyValue in repoDictionary)
                                    {
                                        configuration.Repositories[repoKeyValue.Key] = repoKeyValue.Value;
                                    }
                                    break;
                                }

                            default:
                                throw new JenoException($"{Messages.UnsupportedSetting}{arg.Key}");
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

        private Result ValidateSettings(List<string> settings)
        {
            if (settings.Any(s => !s.Contains(':')))
            {
                var incorrectSettings = string.Join(", ", settings.Where(s => !s.Contains(':')).ToArray());

                var messageBuilder = new StringBuilder();
                messageBuilder.AppendLine(Messages.WrongConfigurationParametersFormat);
                messageBuilder.AppendLine(incorrectSettings);

                return Result.Invalid(messageBuilder.ToString());
            }

            return Result.Ok();
        }
    }
}