using System.Collections.Generic;
using System.Threading.Tasks;
using Jeno.Commands;
using Jeno.Core;
using Jeno.Interfaces;
using McMaster.Extensions.CommandLineUtils;
using Moq;
using NUnit.Framework;

namespace Jeno.UnitTests
{
    [TestFixture]
    internal class SetConfigurationTests
    {
        private const string Command = "set";

        private const string jenkinsJob = "jenkins job";
        private const string repoName = "repository name";

        [Test]
        public async Task SetNewJenkinsUrl_SaveItInConfiguration()
        {
            var parameter = "jenkinsUrl";
            var value = "http://new_jenkins_host:8080";
            var args = new string[] { Command, $"{parameter}:{value}" };

            var configuration = GetDefaultConfiguration();
            var userConsole = GetUserConsoleMock(parameter.ToLower(), value);

            var command = new SetConfiguration(GetConfigurationSerializerMock(configuration).Object, GetEncryptorMock().Object, userConsole.Object);
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            var code = await app.ExecuteAsync(args);

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
            Assert.That(configuration.JenkinsUrl, Is.EqualTo(value));
            userConsole.Verify(c => c.ReadInput(parameter.ToLower(), false), Times.Never);
        }

        [Test]
        public async Task SetNewJenkinsUrlWithoutValue_GetValueFromUserAndSaveItInConfiguration()
        {
            var parameter = "jenkinsUrl";
            var value = "http://new_jenkins_host:8080";
            var args = new string[] { Command, parameter };

            var configuration = GetDefaultConfiguration();
            var userConsole = GetUserConsoleMock(parameter.ToLower(), value);

            var command = new SetConfiguration(GetConfigurationSerializerMock(configuration).Object, GetEncryptorMock().Object, userConsole.Object);
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            var code = await app.ExecuteAsync(args);

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
            Assert.That(configuration.JenkinsUrl, Is.EqualTo(value));
            userConsole.Verify(c => c.ReadInput(parameter.ToLower(), false), Times.Once);
        }

        [Test]
        public async Task SetNewUsername_SaveItInConfiguration()
        {
            var parameter = "userName";
            var value = "jfKennedy";
            var args = new string[] { Command, $"{parameter}:{value}" };

            var configuration = GetDefaultConfiguration();
            var userConsole = GetUserConsoleMock(parameter.ToLower(), value);

            var command = new SetConfiguration(GetConfigurationSerializerMock(configuration).Object, GetEncryptorMock().Object, userConsole.Object);
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            var code = await app.ExecuteAsync(args);

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
            Assert.That(configuration.UserName, Is.EqualTo(value));
            userConsole.Verify(c => c.ReadInput(parameter.ToLower(), false), Times.Never);
        }

        [Test]
        public async Task SetNewUsernameWithoutValue_GetValueFromUserAndSaveItInConfiguration()
        {
            var parameter = "userName";
            var value = "jfKennedy";
            var args = new string[] { Command, parameter };

            var configuration = GetDefaultConfiguration();
            var userConsole = GetUserConsoleMock(parameter.ToLower(), value);

            var command = new SetConfiguration(GetConfigurationSerializerMock(configuration).Object, GetEncryptorMock().Object, userConsole.Object);
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            var code = await app.ExecuteAsync(args);

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
            Assert.That(configuration.UserName, Is.EqualTo(value));
            userConsole.Verify(c => c.ReadInput(parameter.ToLower(), false), Times.Once);
        }

        [Test]
        public async Task SetNewToken_SaveItInConfiguration()
        {
            var parameter = "token";
            var value = "n3wt0k3n";
            var args = new string[] { Command, $"{parameter}:{value}" };

            var configuration = GetDefaultConfiguration();
            var userConsole = GetUserConsoleMock(parameter.ToLower(), value);

            var command = new SetConfiguration(GetConfigurationSerializerMock(configuration).Object, GetEncryptorMock().Object, userConsole.Object);
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            var code = await app.ExecuteAsync(args);

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
            Assert.That(configuration.Token, Is.EqualTo(value));
            userConsole.Verify(c => c.ReadInput(parameter.ToLower(), false), Times.Never);
        }

        [Test]
        public async Task SetNewTokenWithoutValue_GetValueFromUserAndSaveItInConfiguration()
        {
            var parameter = "token";
            var value = "n3wt0k3n";
            var args = new string[] { Command, parameter };

            var configuration = GetDefaultConfiguration();
            var userConsole = GetUserConsoleMock(parameter.ToLower(), value);

            var command = new SetConfiguration(GetConfigurationSerializerMock(configuration).Object, GetEncryptorMock().Object, userConsole.Object);
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            var code = await app.ExecuteAsync(args);

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
            Assert.That(configuration.Token, Is.EqualTo(value));
            userConsole.Verify(c => c.ReadInput(parameter.ToLower(), false), Times.Once);
        }

        [Test]
        public async Task SetPassword_UseEncryptorAndSaveInConfiguration()
        {
            var parameter = "password";
            var value = "321Ytrewq";
            var args = new string[] { Command, $"{parameter}:{value}" };

            var configuration = GetDefaultConfiguration();
            var encryptor = GetEncryptorMock(value);
            var userConsole = GetUserConsoleMock(parameter.ToLower(), value, true);

            var command = new SetConfiguration(GetConfigurationSerializerMock(configuration).Object, encryptor.Object, userConsole.Object);
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            var code = await app.ExecuteAsync(args);

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
            Assert.That(configuration.Password, Is.EqualTo(value));
            encryptor.Verify(e => e.Encrypt(value), Times.Once());
            userConsole.Verify(c => c.ReadInput(parameter.ToLower(), true), Times.Never);
        }

        [Test]
        public async Task RemovePassword_PasswordFieldInConfigurationIsEmpty()
        {
            var parameter = "password";
            var args = new string[] { Command, parameter, "-d" };

            var configuration = GetDefaultConfiguration();
            var encryptor = GetEncryptorMock();
            var userConsole = GetUserConsoleMock(parameter.ToLower(), It.IsAny<string>(), true);

            var command = new SetConfiguration(GetConfigurationSerializerMock(configuration).Object, encryptor.Object, userConsole.Object);
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            var code = await app.ExecuteAsync(args);

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
            Assert.That(configuration.Password, Is.Empty);
            encryptor.Verify(e => e.Encrypt(It.IsAny<string>()), Times.Never);
            userConsole.Verify(c => c.ReadInput(parameter.ToLower(), true), Times.Never);
        }

        [Test]
        public async Task SetPasswordWithoutValue_GetValueFromUserAndSaveInConfiguration()
        {
            var parameter = "password";
            var value = "321Ytrewq";
            var args = new string[] { Command, parameter };

            var configuration = GetDefaultConfiguration();
            var encryptor = GetEncryptorMock(value);
            var userConsole = GetUserConsoleMock(parameter.ToLower(), value, true);

            var command = new SetConfiguration(GetConfigurationSerializerMock(configuration).Object, encryptor.Object, userConsole.Object);
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            var code = await app.ExecuteAsync(args);

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
            Assert.That(configuration.Password, Is.EqualTo(value));
            encryptor.Verify(e => e.Encrypt(value), Times.Once);
            userConsole.Verify(c => c.ReadInput(parameter.ToLower(), true), Times.Once);
        }

        [Test]
        public async Task AddNewRepository_SaveRepositoryInConfiguration()
        {
            var exampleRepo = "thirdExampleRepoUrl";
            var exampleJob = "thirdExampleJob";

            var parameter = "repository";
            var value = $"{exampleRepo}:{exampleJob}";
            var args = new string[] { Command, $"{parameter}:{value}" };

            var configuration = GetDefaultConfiguration();
            var userConsole = new Mock<IUserConsole>();
            userConsole.Setup(c => c.ReadInput(repoName, false)).Returns(exampleRepo);
            userConsole.Setup(c => c.ReadInput(jenkinsJob, false)).Returns(exampleJob);

            var command = new SetConfiguration(GetConfigurationSerializerMock(configuration).Object, GetEncryptorMock().Object, userConsole.Object);
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            var code = await app.ExecuteAsync(args);

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
            Assert.That(configuration.Repository, Contains.Key(exampleRepo));
            Assert.That(configuration.Repository, Contains.Value(exampleJob));
            Assert.That(configuration.Repository[exampleRepo], Is.EqualTo(exampleJob));
            userConsole.Verify(c => c.ReadInput(repoName, false), Times.Never);
            userConsole.Verify(c => c.ReadInput(jenkinsJob, false), Times.Never);
        }

        [Test]
        public async Task AddNewRepositoryWithoutValue_GetValueFromUserAndSaveRepositoryInConfiguration()
        {
            var exampleRepo = "thirdExampleRepoUrl";
            var exampleJob = "thirdExampleJob";

            var parameter = "repository";
            var args = new string[] { Command, parameter };

            var configuration = GetDefaultConfiguration();
            var userConsole = new Mock<IUserConsole>();
            userConsole.Setup(c => c.ReadInput(repoName, false)).Returns(exampleRepo);
            userConsole.Setup(c => c.ReadInput(jenkinsJob, false)).Returns(exampleJob);

            var command = new SetConfiguration(GetConfigurationSerializerMock(configuration).Object, GetEncryptorMock().Object, userConsole.Object);
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            var code = await app.ExecuteAsync(args);

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
            Assert.That(configuration.Repository, Contains.Key(exampleRepo));
            Assert.That(configuration.Repository, Contains.Value(exampleJob));
            Assert.That(configuration.Repository[exampleRepo], Is.EqualTo(exampleJob));
            userConsole.Verify(c => c.ReadInput(repoName, false), Times.Once);
            userConsole.Verify(c => c.ReadInput(jenkinsJob, false), Times.Once);
        }

        [Test]
        public async Task EditSavedRepository_ReplaceOldValue()
        {
            var exampleRepo = "firstExampleRepoUrl";
            var exampleJob = "newExampleJob";

            var parameter = "repository";
            var value = $"{exampleRepo}:{exampleJob}";
            var args = new string[] { Command, $"{parameter}:{value}" };

            var configuration = GetDefaultConfiguration();
            var userConsole = new Mock<IUserConsole>();
            userConsole.Setup(c => c.ReadInput(repoName, false)).Returns(exampleRepo);
            userConsole.Setup(c => c.ReadInput(jenkinsJob, false)).Returns(exampleJob);

            var command = new SetConfiguration(GetConfigurationSerializerMock(configuration).Object, GetEncryptorMock().Object, userConsole.Object);
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            var code = await app.ExecuteAsync(args);

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
            Assert.That(configuration.Repository, Contains.Key(exampleRepo));
            Assert.That(configuration.Repository, Contains.Value(exampleJob));
            Assert.That(configuration.Repository[exampleRepo], Is.EqualTo(exampleJob));
            userConsole.Verify(c => c.ReadInput(repoName, false), Times.Never);
            userConsole.Verify(c => c.ReadInput(jenkinsJob, false), Times.Never);
        }

        [Test]
        public async Task UseDeleteParameter_RemoveSelectedRepositoryFromConfiguration()
        {
            var deleteOption = "--delete";
            var deletedRepository = "firstExampleRepoUrl";
            var args = new string[] { Command, $"repository:{deletedRepository}", deleteOption };

            var configuration = GetDefaultConfiguration();
            var userConsole = new Mock<IUserConsole>();
            userConsole.Setup(c => c.ReadInput(repoName, false)).Returns(string.Empty);
            userConsole.Setup(c => c.ReadInput(jenkinsJob, false)).Returns(string.Empty);

            var command = new SetConfiguration(GetConfigurationSerializerMock(configuration).Object, GetEncryptorMock().Object, userConsole.Object);
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            var code = await app.ExecuteAsync(args);

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
            Assert.That(configuration.Repository, Does.Not.ContainKey(deletedRepository));
        }

        [Test]
        public async Task UseDeleteParameterAlias_RemoveSelectedRepositoryFromConfiguration()
        {
            var deleteOption = "-d";
            var deletedRepository = "firstExampleRepoUrl";
            var args = new string[] { Command, $"repository:{deletedRepository}", deleteOption };

            var configuration = GetDefaultConfiguration();
            var userConsole = new Mock<IUserConsole>();
            userConsole.Setup(c => c.ReadInput(repoName, false)).Returns(string.Empty);
            userConsole.Setup(c => c.ReadInput(jenkinsJob, false)).Returns(string.Empty);

            var command = new SetConfiguration(GetConfigurationSerializerMock(configuration).Object, GetEncryptorMock().Object, userConsole.Object);
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            var code = await app.ExecuteAsync(args);

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
            Assert.That(configuration.Repository, Does.Not.ContainKey(deletedRepository));
        }

        [Test]
        public void TryToSetUndefinedParameter_InformUserAboutUnsupportedParameter()
        {
            var parameter = "domain";
            var value = "s3Cr3t";
            var args = new string[] { Command, $"{parameter}:{value}" };

            var command = new SetConfiguration(GetConfigurationSerializerMock(GetDefaultConfiguration()).Object, GetEncryptorMock().Object, GetUserConsoleMock(parameter.ToLower(), value).Object);

            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            Assert.That(async () => await app.ExecuteAsync(args), Throws.TypeOf<JenoException>()
            .With.Property(nameof(JenoException.ExitCode)).EqualTo(JenoCodes.DefaultError)
            .And.Message.StartsWith("Unsupported parameter")
            .And.Message.Contains(parameter));
        }

        [Test]
        public void TryDeteleDefaultJob_RefuseToDelete()
        {
            var deletedRepository = "default";
            var deleteOption = "-d";
            var args = new string[] { Command, $"repository:{deletedRepository}", deleteOption };

            var userConsole = new Mock<IUserConsole>();
            userConsole.Setup(c => c.ReadInput(repoName, false)).Returns(string.Empty);
            userConsole.Setup(c => c.ReadInput(jenkinsJob, false)).Returns(string.Empty);

            var command = new SetConfiguration(GetConfigurationSerializerMock(GetDefaultConfiguration()).Object, GetEncryptorMock().Object, userConsole.Object);

            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            Assert.That(async () => await app.ExecuteAsync(args), Throws.TypeOf<JenoException>()
            .With.Property(nameof(JenoException.ExitCode)).EqualTo(JenoCodes.DefaultError)
            .And.Message.Contains("Cannot remove default job"));
        }

        [Test]
        public async Task SetMultipleParameters_SaveThemInConfiguration()
        {
            var jenkinsUrlParameterName = "jenkinsUrl";
            var jenkinsUrlParameterValue = "http://new_jenkins_host:8080";
            var userNameParameterName = "username";
            var userNameParameterValue = "jfKennedy";
            var tokenParameterName = "token";
            var tokenParameterValue = "n3wt0k3n";

            var args = new string[]
            {
                Command,
                $"{jenkinsUrlParameterName}:{jenkinsUrlParameterValue}",
                $"{userNameParameterName}:{userNameParameterValue}",
                $"{tokenParameterName}:{tokenParameterValue}"
            };

            var configuration = GetDefaultConfiguration();
            var userConsole = new Mock<IUserConsole>();
            userConsole.Setup(c => c.ReadInput(jenkinsUrlParameterName.ToLower(), false)).Returns(jenkinsUrlParameterValue);
            userConsole.Setup(c => c.ReadInput(userNameParameterName.ToLower(), false)).Returns(userNameParameterValue);
            userConsole.Setup(c => c.ReadInput(tokenParameterName.ToLower(), false)).Returns(tokenParameterValue);

            var command = new SetConfiguration(GetConfigurationSerializerMock(configuration).Object, GetEncryptorMock().Object, userConsole.Object);
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            var code = await app.ExecuteAsync(args);

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
            Assert.That(configuration.JenkinsUrl, Is.EqualTo(jenkinsUrlParameterValue));
            Assert.That(configuration.UserName, Is.EqualTo(userNameParameterValue));
            Assert.That(configuration.Token, Is.EqualTo(tokenParameterValue));
            userConsole.Verify(c => c.ReadInput(jenkinsUrlParameterName.ToLower(), false), Times.Never);
            userConsole.Verify(c => c.ReadInput(userNameParameterName.ToLower(), false), Times.Never);
            userConsole.Verify(c => c.ReadInput(tokenParameterName.ToLower(), false), Times.Never);
        }

        [Test]
        public async Task SetMultipleParametersWithoutValues_GetValuesFromUserAndSaveThemInConfiguration()
        {
            var jenkinsUrlParameterName = "jenkinsUrl";
            var jenkinsUrlParameterValue = "http://new_jenkins_host:8080";
            var userNameParameterName = "userName";
            var userNameParameterValue = "jfKennedy";
            var tokenParameterName = "token";
            var tokenParameterValue = "n3wt0k3n";

            var args = new string[]
            {
                Command,
                jenkinsUrlParameterName,
                userNameParameterName,
                tokenParameterName
            };

            var configuration = GetDefaultConfiguration();
            var userConsole = new Mock<IUserConsole>();
            userConsole.Setup(c => c.ReadInput(jenkinsUrlParameterName.ToLower(), false)).Returns(jenkinsUrlParameterValue);
            userConsole.Setup(c => c.ReadInput(userNameParameterName.ToLower(), false)).Returns(userNameParameterValue);
            userConsole.Setup(c => c.ReadInput(tokenParameterName.ToLower(), false)).Returns(tokenParameterValue);

            var command = new SetConfiguration(GetConfigurationSerializerMock(configuration).Object, GetEncryptorMock().Object, userConsole.Object);
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            var code = await app.ExecuteAsync(args);

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
            Assert.That(configuration.JenkinsUrl, Is.EqualTo(jenkinsUrlParameterValue));
            Assert.That(configuration.UserName, Is.EqualTo(userNameParameterValue));
            Assert.That(configuration.Token, Is.EqualTo(tokenParameterValue));
            userConsole.Verify(c => c.ReadInput(jenkinsUrlParameterName.ToLower(), false), Times.Once);
            userConsole.Verify(c => c.ReadInput(userNameParameterName.ToLower(), false), Times.Once);
            userConsole.Verify(c => c.ReadInput(tokenParameterName.ToLower(), false), Times.Once);
        }

        private JenoConfiguration GetDefaultConfiguration()
        {
            return new JenoConfiguration
            {
                JenkinsUrl = "http://jenkins_host:8080",
                UserName = "jDoe",
                Token = "5om3r4nd0mt0k3n",
                Password = "Qwerty123",
                Repository = new Dictionary<string, string>()
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

        private Mock<IEncryptor> GetEncryptorMock(string password = "Qwerty123")
        {
            var encryptor = new Mock<IEncryptor>();

            encryptor.Setup(s => s.Encrypt(It.IsAny<string>()))
                .Returns(password);

            return encryptor;
        }

        private Mock<IUserConsole> GetUserConsoleMock(string parameterName, string parameterValue, bool hideInput = false)
        {
            var userConsole = new Mock<IUserConsole>();

            userConsole.Setup(s => s.ReadInput(parameterName, hideInput))
                .Returns(parameterValue);

            return userConsole;
        }
    }
}