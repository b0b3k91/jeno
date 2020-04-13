using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Jeno.Commands;
using Jeno.Core;
using Jeno.Interfaces;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace Jeno.UnitTests
{
    [TestFixture]
    class ScanPipelineTests
    {
        private const string Command = "scan";
        private const string JenkinsUrl = "http://jenkins_host:8080";
        private const string UserName = "jDoe";
        private const string Token = "5om3r4nd0mt0k3n";
        private const string DefaultKey = "default";
        private const string DefaultPipeline = "job/defaultPipeline";
        private const string Password = "Qwerty123";
        private const string ExampleRepo = "exampleRepo";
        private const string ExamplePipeline = "job/examplePipeline";

        [Test]
        public void CannotFindMatchedRepo_InformAboutIt()
        {
            var undefinedRepository = "job/fifthExampleRepo";

            var gitWrapper = new Mock<IGitClient>();
            gitWrapper.Setup(s => s.IsGitRepository(It.IsAny<string>()))
                .Returns(Task.FromResult(false));
            gitWrapper.Setup(s => s.GetRepoName(It.IsAny<string>()))
                .Returns(Task.FromResult(undefinedRepository));

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(new MockHttpMessageHandler().ToHttpClient());

            var command = new ScanPipeline(gitWrapper.Object,
                GetEncryptorMock().Object,
                GetUserConsoleMock().Object,
                httpClientFactory.Object,
                GetOptionsMock().Object);

            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            Assert.That(async () => await app.ExecuteAsync(new string[] { Command }), Throws.TypeOf<JenoException>()
            .With.Property(nameof(JenoException.ExitCode)).EqualTo(JenoCodes.DefaultError)
            .And.Property(nameof(JenoException.Message)).Contains("Current directory is not a git repository."));
        }

        [Test]
        public async Task RunScanFromMappedRepository_CreateProperUrl()
        {
            var gitWrapper = GetDefaultGitMock();
            var url = $"{JenkinsUrl}/{DefaultPipeline}/build?delay=0";

            var client = new MockHttpMessageHandler();
            client.Expect(HttpMethod.Post, url)
                .Respond(HttpStatusCode.OK);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new ScanPipeline(gitWrapper.Object,
                GetEncryptorMock().Object,
                GetUserConsoleMock().Object,
                httpClientFactory.Object,
                GetOptionsMock().Object);

            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);
            var code = await app.ExecuteAsync(new string[] { Command });

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
            Assert.That(() => client.VerifyNoOutstandingExpectation(), Throws.Nothing);
        }

        [Test]
        public async Task ScanWasSuccessful_ReturnOkCode()
        {
            var gitWrapper = GetDefaultGitMock();

            var client = new MockHttpMessageHandler();
            client.When($"{JenkinsUrl}/{DefaultPipeline}/build?delay=0")
                .Respond(HttpStatusCode.OK);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new ScanPipeline(gitWrapper.Object,
                GetEncryptorMock().Object,
                GetUserConsoleMock().Object,
                httpClientFactory.Object,
                GetOptionsMock().Object);

            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);
            var code = await app.ExecuteAsync(new string[] { Command });

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
        }

        [Test]
        public void SomeErrorOccuredInScanRequest_DisplayErrorMessage()
        {
            var gitWrapper = new Mock<IGitClient>();
            gitWrapper.Setup(s => s.IsGitRepository(It.IsAny<string>()))
                .Returns(Task.FromResult(true));
            gitWrapper.Setup(s => s.GetRepoName(It.IsAny<string>()))
                .Returns(Task.FromResult(DefaultKey));

            var client = new MockHttpMessageHandler();
            client.When($"{JenkinsUrl}/{DefaultPipeline}/build?delay=0")
                .Respond(HttpStatusCode.BadRequest);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new ScanPipeline(gitWrapper.Object,
                GetEncryptorMock().Object,
                GetUserConsoleMock().Object,
                httpClientFactory.Object,
                GetOptionsMock().Object);

            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            Assert.That(async () => await app.ExecuteAsync(new string[] { Command }), Throws.InstanceOf<JenoException>()
                .With.Property(nameof(JenoException.ExitCode)).EqualTo(JenoCodes.DefaultError)
                .And.Property(nameof(JenoException.Message)).Contains("Cannot scan pipeline")); ;
        }

        private Mock<IOptions<JenoConfiguration>> GetOptionsMock(JenoConfiguration configuration = null)
        {
            if (configuration == null)
            {
                configuration = new JenoConfiguration
                {
                    JenkinsUrl = JenkinsUrl,
                    UserName = UserName,
                    Token = Token,
                    Password = Password,
                    Repository = new Dictionary<string, string>()
                    {
                        { ExampleRepo, ExamplePipeline },
                        { DefaultKey,  DefaultPipeline},
                    }
                };
            }

            var options = new Mock<IOptions<JenoConfiguration>>();
            options.Setup(c => c.Value)
                .Returns(configuration);

            return options;
        }

        private Mock<IGitClient> GetDefaultGitMock()
        {
            var gitWrapper = new Mock<IGitClient>();
            gitWrapper.Setup(s => s.IsGitRepository(It.IsAny<string>()))
                .Returns(Task.FromResult(true));
            gitWrapper.Setup(s => s.GetRepoName(It.IsAny<string>()))
                .Returns(Task.FromResult(DefaultKey));

            return gitWrapper;
        }

        private Mock<IUserConsole> GetUserConsoleMock()
        {
            var passwordProvider = new Mock<IUserConsole>();
            passwordProvider.Setup(s => s.ReadInput("password", true))
                .Returns(Password);

            return passwordProvider;
        }

        private Mock<IEncryptor> GetEncryptorMock()
        {
            var encryptor = new Mock<IEncryptor>();
            encryptor.Setup(s => s.Decrypt(It.IsAny<string>()))
                .Returns(Password);

            return encryptor;
        }
    }
}
