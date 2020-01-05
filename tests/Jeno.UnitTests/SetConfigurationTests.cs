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
    internal class SetConfigurationTests
    {
        private readonly string _command = "set";
        private const string Password = "Qwerty123";

        [Test]
        public async Task SetNewJenkinsUrl_SaveItInConfiguration()
        {
            var value = "http://new_jenkins_host:8080";
            var args = new string[] { _command, $"jenkinsUrl:{value}" };

            var configuration = GetDefaultConfiguration();

            var command = new SetConfiguration(GetConfigurationSerializerMock(configuration).Object, GetEncryptorMock().Object);
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            var code = await app.ExecuteAsync(args);

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
            Assert.That(configuration.JenkinsUrl, Is.EqualTo(value));
        }

        [Test]
        public async Task SetNewUsername_SaveItInConfiguration()
        {
            var value = "jfKennedy";
            var args = new string[] { _command, $"username:{value}" };

            var configuration = GetDefaultConfiguration();

            var command = new SetConfiguration(GetConfigurationSerializerMock(configuration).Object, GetEncryptorMock().Object);
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            var code = await app.ExecuteAsync(args);

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
            Assert.That(configuration.UserName, Is.EqualTo(value));
        }

        [Test]
        public async Task SetNewToken_SaveItInConfiguratrion()
        {
            var value = "n3wt0k3n";
            var args = new string[] { _command, $"token:{value}" };

            var configuration = GetDefaultConfiguration();
            var command = new SetConfiguration(GetConfigurationSerializerMock(configuration).Object, GetEncryptorMock().Object);
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            var code = await app.ExecuteAsync(args);

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
            Assert.That(configuration.Token, Is.EqualTo(value));
        }

        [Test]
        public async Task SetPassword_UseEncryptorAndSaveInConfiguration()
        {
            var value = "Qwertz123";
            var args = new string[] { _command, $"password:{value}" };

            var configuration = GetDefaultConfiguration();
            var encryptor = GetEncryptorMock(value);

            var command = new SetConfiguration(GetConfigurationSerializerMock(configuration).Object, encryptor.Object);
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            var code = await app.ExecuteAsync(args);

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
            Assert.That(configuration.Password, Is.EqualTo(value));
            encryptor.Verify(s => s.Encrypt(value), Times.AtLeastOnce());
        }

        [Test]
        public async Task AddNewRepository_SaveRepositoryInConfiguration()
        {
            var exampleRepo = "thirdExampleRepoUrl";
            var exampleJob = "thirdExampleJob";
            var args = new string[] { _command, $"repository:{exampleRepo}={exampleJob}" };

            var configuration = GetDefaultConfiguration();

            var command = new SetConfiguration(GetConfigurationSerializerMock(configuration).Object, GetEncryptorMock().Object);
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

            var command = new SetConfiguration(GetConfigurationSerializerMock(configuration).Object, GetEncryptorMock().Object);
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

            var command = new SetConfiguration(GetConfigurationSerializerMock(configuration).Object, GetEncryptorMock().Object);
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

            var command = new SetConfiguration(GetConfigurationSerializerMock(configuration).Object, GetEncryptorMock().Object);
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            var code = await app.ExecuteAsync(args);

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
            Assert.That(configuration.Repositories, Does.Not.ContainKey(deletedRepository));
        }

        [Test]
        public void TryToSetUndefinedParameter_InformUserAboutAvailableParameters()
        {
            var parameter = "domain";
            var value = "s3Cr3t";
            var args = new string[] { _command, $"{parameter}:{value}" };

            var command = new SetConfiguration(GetConfigurationSerializerMock(GetDefaultConfiguration()).Object, GetEncryptorMock().Object);

            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            Assert.That(async () => await app.ExecuteAsync(args), Throws.TypeOf<JenoException>()
            .With.Property("ExitCode").EqualTo(JenoCodes.DefaultError)
            .And.Property("Message").StartsWith("Unsupported parameter")
            .And.Property("Message").Contains(parameter));
        }

        [Test]
        public void TryDeteleDefaultJob_ThrowException()
        {
            var deletedRepository = "default";
            var deleteOption = "-d";
            var args = new string[] { _command, $"repository:{deletedRepository}", deleteOption };

            var command = new SetConfiguration(GetConfigurationSerializerMock(GetDefaultConfiguration()).Object, GetEncryptorMock().Object);

            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            Assert.That(async () => await app.ExecuteAsync(args), Throws.TypeOf<JenoException>()
            .With.Property("ExitCode").EqualTo(JenoCodes.DefaultError)
            .And.Property("Message").Contains("Cannot remove default job"));
        }

        private JenoConfiguration GetDefaultConfiguration()
        {
            return new JenoConfiguration
            {
                JenkinsUrl = "http://jenkins_host:8080",
                UserName = "jDoe",
                Token = "5om3r4nd0mt0k3n",
                Password = "Qwerty123",
                Repositories = new Dictionary<string, string>()
                {
                    { "firstExampleRepoUrl", "firstExampleJob" },
                    { "secondExampleRepoUrl", "secondExampleJob" },
                    { "default", "defaultJob" },
                }
            };
        }

        private Mock<IConfigurationSerializer> GetConfigurationSerializerMock(JenoConfiguration configuration)
        {
            var configurationProvider = new Mock<IConfigurationSerializer>();
            configurationProvider.Setup(s => s.ReadConfiguration())
                .Returns(Task.FromResult(configuration));
            configurationProvider.Setup(s => s.SaveConfiguration(It.IsAny<JenoConfiguration>()))
                .Returns(Task.CompletedTask);

            return configurationProvider;
        }

        private Mock<IEncryptor> GetEncryptorMock(string password = Password)
        {
            var encryptor = new Mock<IEncryptor>();

            encryptor.Setup(s => s.Encrypt(It.IsAny<string>()))
                .Returns(password);

            return encryptor;
        }
    }
}