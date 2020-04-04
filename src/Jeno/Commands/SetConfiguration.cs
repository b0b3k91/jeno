using System;
using System.Linq;
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
                    var delete = deleteOption.Values.Count > 0;

                    foreach (var arg in args)
                    {
                        switch(arg.Item1)
                        {
                            case ("jenkinsurl"):
                                configuration.JenkinsUrl = GetNewSettingValue(arg, delete);
                                break;

                            case ("username"):
                                configuration.UserName = GetNewSettingValue(arg, delete);
                                break;

                            case ("token"):
                                configuration.Token = GetNewSettingValue(arg, delete);
                                break;

                            case ("password"): 
                                configuration.Password = GetNewSettingValue(arg, delete, true);
                                break;

                            case ("repository"):
                                {
                                    if (string.IsNullOrWhiteSpace(arg.Item2))
                                    {
                                        var repositoryName = _userConsole.ReadInput("repository name");
                                        var jenkinsJob = _userConsole.ReadInput("jenkins job");

                                        configuration.Repository[repositoryName] = jenkinsJob;
                                        break;
                                    }

                                    var repositories = arg.Item2.Split(',');

                                    if (delete)
                                    {
                                        if (repositories.Any(s => s == "default"))
                                        {
                                            throw new JenoException(Messages.RemoveDefaultJobException);
                                        }

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
                        };
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

        private string GetNewSettingValue((string, string) setting, bool delete, bool encrypt = false)
        {
            if (delete)
            {
                return string.Empty;
            }

            var value = string.IsNullOrWhiteSpace(setting.Item2) ? _userConsole.ReadInput(setting.Item1, encrypt) : setting.Item2;
            return encrypt ? _encryptor.Encrypt(value) : value;
        }
    }
}