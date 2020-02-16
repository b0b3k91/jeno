using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jeno.Core;
using Jeno.Interfaces;
using McMaster.Extensions.CommandLineUtils;

namespace Jeno.Commands
{
    public class SetConfiguration : IJenoCommand
    {
        private readonly IConfigurationSerializer _serializer;
        private readonly IEncryptor _encryptor;
        private readonly IUserConsole _userConsole;

        public string Name => "set";
        public Action<CommandLineApplication> Command { get; }

        public SetConfiguration(IConfigurationSerializer serializer, IEncryptor encryptor, IUserConsole userConsole)
        {
            _serializer = serializer;
            _encryptor = encryptor;
            _userConsole = userConsole;

            Command = (app) =>
            {
                app.Description = Messages.ChangeConfigurationDescription;

                var settings = app.Argument("settings", Messages.SettingDescription, true).Values;
                var deleteOption = app.Option("-d|--delete", Messages.DeleteRepoOptionDescription, CommandOptionType.NoValue);

                app.OnExecuteAsync(async token =>
                {
                    var configuration = await _serializer.ReadConfiguration();

                    var args = settings.Select(s => ParseSetting(s));

                    foreach (var arg in args)
                    {
                        switch (arg.Item1)
                        {
                            case "jenkinsurl":
                                configuration.JenkinsUrl = ReadSettingValue(arg.Item1, arg.Item2);
                                break;

                            case "username":
                                configuration.UserName = ReadSettingValue(arg.Item1, arg.Item2);
                                break;

                            case "token":
                                configuration.Token = ReadSettingValue(arg.Item1, arg.Item2);
                                break;

                            case "password":
                                configuration.Password = ReadSettingValue(arg.Item1, arg.Item2, true);
                                break;

                            case "repository":
                                {
                                    if (string.IsNullOrWhiteSpace(arg.Item2))
                                    {
                                        var repositoryName = _userConsole.GetInput("repository name");
                                        var jenkinsJob = _userConsole.GetInput("jenkins job");

                                        configuration.Repository[repositoryName] = jenkinsJob;
                                        break;
                                    }

                                    var repositories = arg.Item2.Split(',');

                                    if (deleteOption.Values.Count > 0)
                                    {
                                        if (repositories.Any(s => s == "default"))
                                            throw new JenoException(Messages.RemoveDefaultJobException);

                                        foreach (var repository in repositories)
                                        {
                                            if (configuration.Repository.ContainsKey(repository))
                                            {
                                                configuration.Repository.Remove(repository);
                                            }
                                        }

                                        break;
                                    }

                                    if (repositories.Any(s => !s.Contains('=')))
                                    {
                                        throw new JenoException(Messages.WrongReposFormat);
                                    }

                                    var repos = repositories.Select(s => (s.Split('=')[0], s.Split('=')[1]));

                                    if (repos.Select(s => s.Item1).Any(s => string.IsNullOrWhiteSpace(s)))
                                    {
                                        throw new JenoException(Messages.MissingRepoName);
                                    }

                                    foreach (var repo in repos)
                                    {
                                        configuration.Repository[repo.Item1] = repo.Item2;
                                    }
                                    break;
                                }

                            default:
                                throw new JenoException($"{Messages.UnsupportedSetting}{arg.Item1}");
                        }
                    }

                    await _serializer.SaveConfiguration(configuration);
                    return JenoCodes.Ok;
                });
            };
        }

        private (string, string) ParseSetting(string setting)
        {
            //only first colon should be treated like separator,
            //others (like the ones in url addresses) must be ignored.
            return (setting.Split(':').First().ToLower(), string.Join(':', setting.Split(':').Skip(1)));
        }

        private string ReadSettingValue(string parameterName, string parameterValue, bool encrypt = false)
        {
            if (encrypt)
            {
                return string.IsNullOrWhiteSpace(parameterValue) ?
                    _encryptor.Encrypt(_userConsole.GetInput(parameterName, true)) :
                    _encryptor.Encrypt(parameterValue);
            }
            else
            {
                return string.IsNullOrWhiteSpace(parameterValue) ?
                    _userConsole.GetInput(parameterName) :
                    parameterValue;
            }
        }
    }
}