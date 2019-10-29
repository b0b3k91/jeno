﻿using Jeno.Commands;
using Jeno.Core;
using Jeno.Interfaces;
using McMaster.Extensions.CommandLineUtils;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Jeno.UnitTests
{
    [TestFixture]
    internal class ChangeConfigurationTests
    {
        private readonly string _command = "set";

        [Test]
        public async Task SetNewJenkinsUrl_SaveItInConfiguration()
        {
            var value = "http://new_jenkins_host:8080";
            var args = new string[] { _command, $"jenkinsUrl:{value}" };

            var configuration = GetDefaultConfiguration();

            var configurationProvider = new Mock<IConfigurationSerializer>();

            configurationProvider.Setup(s => s.ReadConfiguration())
                .Returns(Task.FromResult(configuration));

            configurationProvider.Setup(s => s.SaveConfiguration(It.IsAny<JenoConfiguration>()))
                .Returns(Task.CompletedTask);

            var command = new ChangeConfiguration(configurationProvider.Object);
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);
            var code = await app.ExecuteAsync(args);

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
            Assert.That(configuration.JenkinsUrl, Is.EqualTo(value));
        }

        [Test]
        public async Task AddNewRepository_SaveRepositoryInConfiguration()
        {
            var exampleRepo = "thirdExampleRepoUrl";
            var exampleJob = "thirdExampleJob";
            var args = new string[] { _command, $"repository:{exampleRepo}={exampleJob}" };

            var configuration = GetDefaultConfiguration();

            var configurationProvider = new Mock<IConfigurationSerializer>();

            configurationProvider.Setup(s => s.ReadConfiguration())
                .Returns(Task.FromResult(configuration));

            configurationProvider.Setup(s => s.SaveConfiguration(It.IsAny<JenoConfiguration>()))
                .Returns(Task.CompletedTask);

            var command = new ChangeConfiguration(configurationProvider.Object);
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);
            var code = await app.ExecuteAsync(args);

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
            Assert.That(configuration.Repositories, Contains.Key(exampleRepo));
            Assert.That(configuration.Repositories, Contains.Value(exampleJob));
            Assert.That(configuration.Repositories[exampleRepo], Is.EqualTo(exampleJob));
        }

        [Test]
        public async Task EditSavedRepository_ReplaceOldValue()
        {
            var exampleRepo = "firstExampleRepoUrl";
            var exampleJob = "newExampleJob";
            var args = new string[] { _command, $"repository:{exampleRepo}={exampleJob}" };

            var configuration = GetDefaultConfiguration();

            var configurationProvider = new Mock<IConfigurationSerializer>();

            configurationProvider.Setup(s => s.ReadConfiguration())
                .Returns(Task.FromResult(configuration));

            configurationProvider.Setup(s => s.SaveConfiguration(It.IsAny<JenoConfiguration>()))
                .Returns(Task.CompletedTask);

            var command = new ChangeConfiguration(configurationProvider.Object);
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);
            var code = await app.ExecuteAsync(args);

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
            Assert.That(configuration.Repositories, Contains.Key(exampleRepo));
            Assert.That(configuration.Repositories, Contains.Value(exampleJob));
            Assert.That(configuration.Repositories[exampleRepo], Is.EqualTo(exampleJob));
        }

        [Test]
        public async Task UseDeleteParameter_RemovePassedRepositoryFromConfiguration()
        {
            var deleteOption = "--delete";
            var deletedRepository = "firstExampleRepoUrl";
            var args = new string[] { _command, $"repository:{deletedRepository}", deleteOption };

            var configuration = GetDefaultConfiguration();

            var configurationProvider = new Mock<IConfigurationSerializer>();

            configurationProvider.Setup(s => s.ReadConfiguration())
                .Returns(Task.FromResult(configuration));

            configurationProvider.Setup(s => s.SaveConfiguration(It.IsAny<JenoConfiguration>()))
                .Returns(Task.CompletedTask);

            var command = new ChangeConfiguration(configurationProvider.Object);
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);
            var code = await app.ExecuteAsync(args);

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
            Assert.That(configuration.Repositories, Does.Not.ContainKey(deletedRepository));
        }

        [Test]
        public async Task UseDeleteAliasParameter_RemovePassedRepositoryFromConfiguration()
        {
            var deleteOption = "-d";
            var deletedRepository = "firstExampleRepoUrl";
            var args = new string[] { _command, $"repository:{deletedRepository}", deleteOption };


            var configuration = GetDefaultConfiguration();

            var configurationProvider = new Mock<IConfigurationSerializer>();

            configurationProvider.Setup(s => s.ReadConfiguration())
                .Returns(Task.FromResult(configuration));

            configurationProvider.Setup(s => s.SaveConfiguration(It.IsAny<JenoConfiguration>()))
                .Returns(Task.CompletedTask);

            var command = new ChangeConfiguration(configurationProvider.Object);
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);
            var code = await app.ExecuteAsync(args);

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
            Assert.That(configuration.Repositories, Does.Not.ContainKey(deletedRepository));
        }

        [Test]
        public void TryToSetUndefinedParameter_InformUserAboutAvailableParameters()
        {
            var parameter = "password";
            var value = "s3Cr3t0n3";
            var args = new string[] { _command, $"{parameter}:{value}" };

            var configurationProvider = new Mock<IConfigurationSerializer>();
            configurationProvider.Setup(s => s.ReadConfiguration())
                .Returns(Task.FromResult(GetDefaultConfiguration()));

            var command = new ChangeConfiguration(configurationProvider.Object);

            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            var exception = Assert.ThrowsAsync<JenoException>(async () =>
            {
                await app.ExecuteAsync(args);
            });

            Assert.That(exception.ExitCode, Is.EqualTo(JenoCodes.DefaultError));
            Assert.That(exception.Message, Does.StartWith("Unsupported parameter"));
            Assert.That(exception.Message, Does.Contain(parameter));
        }

        [Test]
        public void TryDeteleDefaultJob_ThrowExcetpion()
        {
            var deletedRepository = "default";
            var deleteOption = "-d";
            var args = new string[] { _command, $"repository:{deletedRepository}", deleteOption };

            var configurationProvider = new Mock<IConfigurationSerializer>();
            configurationProvider.Setup(s => s.ReadConfiguration())
                .Returns(Task.FromResult(GetDefaultConfiguration()));

            var command = new ChangeConfiguration(configurationProvider.Object);

            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            var exception = Assert.ThrowsAsync<JenoException>(async () =>
            {
                await app.ExecuteAsync(args);
            });

            Assert.That(exception.ExitCode, Is.EqualTo(JenoCodes.DefaultError));
            Assert.That(exception.Message, Does.Contain("Cannot remove default job"));
        }

        private JenoConfiguration GetDefaultConfiguration()
        {
            return new JenoConfiguration
            {
                JenkinsUrl = "http://jenkins_host:8080",
                Repositories = new Dictionary<string, string>()
                {
                    { "firstExampleRepoUrl", "firstExampleJob" },
                    { "secondExampleRepoUrl", "secondExampleJob" },
                    { "default", "defaultJob" },
                }
            };
        }
    }
}