using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Jeno.Commands;
using Jeno.Core;
using Jeno.Infrastructure;
using Jeno.Interfaces;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace Jeno.UnitTests
{
    [TestFixture]
    public class RunJobTests
    {
        private const string Command = "run";

        private const string JenkinsUrl = "http://jenkins_host:8080";
        private const string UserName = "jDoe";
        private const string Token = "5om3r4nd0mt0k3n";
        private const string DefaultKey = "default";
        private const string DefaultJob = "defaultJob";

        private const string Branch = "master";
        private const string Password = "Qwerty123";

        private const string CrumbContentType = "application/json";

        private readonly CrumbHeader _crumbHeader = new CrumbHeader
        {
            Crumb = "hYwN6MK1RlHpinq963cOO0jdwdb8Flrn",
            CrumbRequestField = "Jenkins-Crumb"
        };

        [Test]
        public async Task PassUndefinedRepository_RunDefaultJob()
        {
            var undefinedRepository = "fifthExampleRepo";

            var gitWrapper = new Mock<IGitClient>();
            gitWrapper.Setup(s => s.IsGitRepository(It.IsAny<string>()))
                .Returns(Task.FromResult(true));
            gitWrapper.Setup(s => s.GetRepoName(It.IsAny<string>()))
                .Returns(Task.FromResult(undefinedRepository));
            gitWrapper.Setup(s => s.GetCurrentBranch(It.IsAny<string>()))
                .Returns(Task.FromResult(Branch));

            var client = new MockHttpMessageHandler();
            client.When($"{JenkinsUrl}/job/{DefaultJob}/{Branch}/buildWithParameters")
                .Respond(HttpStatusCode.OK);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new RunJob(gitWrapper.Object,
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
        public async Task PassRepositoryDefinedInConfiguration_RunJobSavedInConfiguration()
        {
            var exampleRepo = "firstExampleRepoUrl";
            var exampleJob = "firstExampleJob";

            var configuration = new JenoConfiguration
            {
                JenkinsUrl = JenkinsUrl,
                UserName = UserName,
                Token = Token,
                Password = Password,
                Repository = new Dictionary<string, string>()
                {
                    { exampleRepo, exampleJob },
                    { "secondExampleRepoUrl", "secondExampleJob" },
                    { DefaultKey,  DefaultJob},
                }
            };

            var gitWrapper = new Mock<IGitClient>();
            gitWrapper.Setup(s => s.IsGitRepository(It.IsAny<string>()))
                .Returns(Task.FromResult(true));
            gitWrapper.Setup(s => s.GetRepoName(It.IsAny<string>()))
                .Returns(Task.FromResult(exampleRepo));
            gitWrapper.Setup(s => s.GetCurrentBranch(It.IsAny<string>()))
                .Returns(Task.FromResult(Branch));

            var client = new MockHttpMessageHandler();
            client.When($"{JenkinsUrl}/job/{exampleJob}/{Branch}/buildWithParameters")
                .Respond(HttpStatusCode.OK);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new RunJob(gitWrapper.Object,
                GetEncryptorMock().Object,
                GetUserConsoleMock().Object,
                httpClientFactory.Object,
                GetOptionsMock(configuration).Object);

            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);
            var code = await app.ExecuteAsync(new string[] { Command });

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
        }

        [Test]
        public async Task RunJenoOutsideRepository_BreakExecution()
        {
            var gitWrapper = new Mock<IGitClient>();
            gitWrapper.Setup(s => s.IsGitRepository(It.IsAny<string>()))
                .Returns(Task.FromResult(false));
            gitWrapper.Setup(s => s.GetRepoName(It.IsAny<string>()))
                .Returns(Task.FromResult(DefaultKey));
            gitWrapper.Setup(s => s.GetCurrentBranch(It.IsAny<string>()))
                .Returns(Task.FromResult(Branch));

            var client = new MockHttpMessageHandler();
            client.When($"{JenkinsUrl}/job/{DefaultJob}/{Branch}/buildWithParameters")
                .Respond(HttpStatusCode.OK);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new RunJob(gitWrapper.Object,
                GetEncryptorMock().Object,
                GetUserConsoleMock().Object,
                httpClientFactory.Object,
                GetOptionsMock().Object);

            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            Assert.That(async () => await app.ExecuteAsync(new string[] { Command }), Throws.TypeOf<JenoException>()
            .With.Property(nameof(JenoException.ExitCode)).EqualTo(JenoCodes.DefaultError)
            .And.Property(nameof(JenoException.Message)).Contains("Current directory is not git repository."));
        }

        [Test]
        public async Task MissingUserName_InformAboutIt()
        {
            var configuration = new JenoConfiguration
            {
                JenkinsUrl = JenkinsUrl,
                UserName = string.Empty,
                Token = Token,
                Password = Password,
                Repository = new Dictionary<string, string>()
                {
                    { "firstExampleRepoUrl", "firstExampleJob" },
                    { "secondExampleRepoUrl", "secondExampleJob" },
                    { DefaultKey, DefaultJob },
                }
            };

            var client = new MockHttpMessageHandler();
            client.When($"{JenkinsUrl}/job/{DefaultJob}/job/{Branch}/buildWithParameters")
                .Respond(HttpStatusCode.OK);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new RunJob(GetDefaultGitMock().Object,
                GetEncryptorMock().Object,
                GetUserConsoleMock().Object,
                httpClientFactory.Object,
                GetOptionsMock(configuration).Object);

            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            Assert.That(async () => await app.ExecuteAsync(new string[] { Command }), Throws.TypeOf<JenoException>()
            .With.Property(nameof(JenoException.ExitCode)).EqualTo(JenoCodes.DefaultError)
            .And.Property(nameof(JenoException.Message)).Contains("Username is undefined"));
        }

        [Test]
        public async Task MissingTokenInConfiguration_InformUserAndShowLinkToTokenGenerator()
        {
            var configuration = new JenoConfiguration
            {
                JenkinsUrl = JenkinsUrl,
                UserName = UserName,
                Token = string.Empty,
                Password = Password,
                Repository = new Dictionary<string, string>()
                {
                    { "firstExampleRepoUrl", "firstExampleJob" },
                    { "secondExampleRepoUrl", "secondExampleJob" },
                    { DefaultKey, DefaultJob },
                }
            };

            var client = new MockHttpMessageHandler();
            client.When($"{JenkinsUrl}/job/{DefaultJob}/job/{Branch}/buildWithParameters")
                .Respond(HttpStatusCode.OK);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new RunJob(GetDefaultGitMock().Object,
                GetEncryptorMock().Object,
                GetUserConsoleMock().Object,
                httpClientFactory.Object,
                GetOptionsMock(configuration).Object);

            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            Assert.That(async () => await app.ExecuteAsync(new string[] { Command }), Throws.TypeOf<JenoException>()
            .With.Property(nameof(JenoException.ExitCode)).EqualTo(JenoCodes.DefaultError)
            .And.Property(nameof(JenoException.Message)).Contains("User token is undefined")
            .And.Property(nameof(JenoException.Message)).Contain($"{JenkinsUrl}/user/{UserName}/configure"));
        }

        [Test]
        public async Task RunJobWithoutDefinedMainUrl_InformAboutIt()
        {
            var configuration = new JenoConfiguration
            {
                JenkinsUrl = string.Empty,
                UserName = UserName,
                Token = Token,
                Repository = new Dictionary<string, string>()
                {
                    { "firstExampleRepoUrl", "firstExampleJob" },
                    { "secondExampleRepoUrl", "secondExampleJob" },
                    { DefaultKey, DefaultJob },
                }
            };

            var client = new MockHttpMessageHandler();
            client.When($"{JenkinsUrl}/job/{DefaultJob}/job/{Branch}/buildWithParameters")
                .Respond(HttpStatusCode.OK);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new RunJob(GetDefaultGitMock().Object,
                GetEncryptorMock().Object,
                GetUserConsoleMock().Object,
                httpClientFactory.Object,
                GetOptionsMock(configuration).Object);

            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            Assert.That(async () => await app.ExecuteAsync(new string[] { Command }), Throws.TypeOf<JenoException>()
            .With.Property(nameof(JenoException.ExitCode)).EqualTo(JenoCodes.DefaultError)
            .And.Property(nameof(JenoException.Message)).Contains("Jenkins address is undefined or incorrect"));
        }

        [Test]
        public async Task RunJobWithIncorrectJenkinsUrl_InformAboutIt()
        {
            var configuration = new JenoConfiguration
            {
                JenkinsUrl = "incorrectUrl",
                UserName = UserName,
                Token = Token,
                Repository = new Dictionary<string, string>()
                {
                    { "firstExampleRepoUrl", "firstExampleJob" },
                    { "secondExampleRepoUrl", "secondExampleJob" },
                    { DefaultKey, DefaultJob },
                }
            };

            var passwordProvider = GetUserConsoleMock();

            var client = new MockHttpMessageHandler();
            client.When($"{JenkinsUrl}/job/{DefaultJob}/job/{Branch}/buildWithParameters")
                .Respond(HttpStatusCode.OK);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new RunJob(GetDefaultGitMock().Object,
                GetEncryptorMock().Object,
                GetUserConsoleMock().Object,
                httpClientFactory.Object,
                GetOptionsMock(configuration).Object);

            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            Assert.That(async () => await app.ExecuteAsync(new string[] { Command }), Throws.TypeOf<JenoException>()
            .With.Property(nameof(JenoException.ExitCode)).EqualTo(JenoCodes.DefaultError)
            .And.Property(nameof(JenoException.Message)).Contains("Jenkins address is undefined or incorrect"));
        }

        [Test]
        public async Task RunJobWithUndefinedDefaultJob_InformAboutIt()
        {
            var configuration = new JenoConfiguration
            {
                JenkinsUrl = JenkinsUrl,
                UserName = UserName,
                Token = Token,
                Password = Password,
                Repository = new Dictionary<string, string>()
                {
                    { "firstExampleRepoUrl", "firstExampleJob" },
                    { "secondExampleRepoUrl", "secondExampleJob" },
                }
            };

            var client = new MockHttpMessageHandler();
            client.When($"{JenkinsUrl}/job/{DefaultJob}/job/{Branch}/buildWithParameters")
                .Respond(HttpStatusCode.OK);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new RunJob(GetDefaultGitMock().Object,
                GetEncryptorMock().Object,
                GetUserConsoleMock().Object,
                httpClientFactory.Object,
                GetOptionsMock(configuration).Object);

            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            Assert.That(async () => await app.ExecuteAsync(new string[] { Command }), Throws.TypeOf<JenoException>()
            .With.Property(nameof(JenoException.ExitCode)).EqualTo(JenoCodes.DefaultError)
            .And.Property(nameof(JenoException.Message)).Contains("Missing default job"));
        }

        [Test]
        public async Task TryRunJobOnJenkinsWithCSRFProtection_UseBasicCredentialsAndAskForCrumb()
        {
            var basicAuthHeader = new BasicAuthenticationHeader(UserName, Password);
            var tokenAuthHeader = new BearerAuthenticationHeader(Token);

            var client = new MockHttpMessageHandler();
            client.Expect($"{JenkinsUrl}/job/{DefaultJob}/{Branch}/buildWithParameters")
                .WithHeaders("Authorization", $"{tokenAuthHeader.Scheme} {tokenAuthHeader.Parameter}")
                .Respond(request => new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    ReasonPhrase = "No valid crumb was included in the request"
                });
            client.Expect($"{JenkinsUrl}/crumbIssuer/api/json")
                .WithHeaders("Authorization", $"{basicAuthHeader.Scheme} {basicAuthHeader.Parameter}")
                .Respond(CrumbContentType, JsonConvert.SerializeObject(_crumbHeader));
            client.Expect($"{JenkinsUrl}/job/{DefaultJob}/{Branch}/buildWithParameters")
                .WithHeaders("Authorization", $"{basicAuthHeader.Scheme} {basicAuthHeader.Parameter}")
                .WithHeaders(_crumbHeader.CrumbRequestField, _crumbHeader.Crumb)
                .Respond(HttpStatusCode.OK);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new RunJob(GetDefaultGitMock().Object,
                GetEncryptorMock().Object,
                GetUserConsoleMock().Object,
                httpClientFactory.Object,
                GetOptionsMock().Object);

            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            var result = await app.ExecuteAsync(new string[] { Command });

            Assert.That(result, Is.EqualTo(JenoCodes.Ok));
        }

        [Test]
        public async Task TryToRunJobOnJenkinsWithCSRFProtectionWithoutSavedPassword_AskUserForPassword()
        {
            var basicAuthHeader = new BasicAuthenticationHeader(UserName, Password);
            var tokenAuthHeader = new BearerAuthenticationHeader(Token);

            var configuration = new JenoConfiguration
            {
                JenkinsUrl = JenkinsUrl,
                UserName = UserName,
                Token = Token,
                Password = string.Empty,
                Repository = new Dictionary<string, string>()
                    {
                        { "firstExampleRepoUrl", "firstExampleJob" },
                        { "secondExampleRepoUrl", "secondExampleJob" },
                        { DefaultKey,  DefaultJob},
                    }
            };

            var client = new MockHttpMessageHandler();
            client.Expect($"{JenkinsUrl}/job/{DefaultJob}/{Branch}/buildWithParameters")
                .WithHeaders("Authorization", $"{tokenAuthHeader.Scheme} {tokenAuthHeader.Parameter}")
                .Respond(request => new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    ReasonPhrase = "No valid crumb was included in the request"
                });
            client.Expect($"{JenkinsUrl}/crumbIssuer/api/json")
                .WithHeaders("Authorization", $"{basicAuthHeader.Scheme} {basicAuthHeader.Parameter}")
                .Respond(CrumbContentType, JsonConvert.SerializeObject(_crumbHeader));
            client.Expect($"{JenkinsUrl}/job/{DefaultJob}/{Branch}/buildWithParameters")
                .WithHeaders("Authorization", $"{basicAuthHeader.Scheme} {basicAuthHeader.Parameter}")
                .WithHeaders(_crumbHeader.CrumbRequestField, _crumbHeader.Crumb)
                .Respond(HttpStatusCode.OK);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var passwordProvider = GetUserConsoleMock();

            var command = new RunJob(GetDefaultGitMock().Object,
                GetEncryptorMock().Object,
                passwordProvider.Object,
                httpClientFactory.Object,
                GetOptionsMock(configuration).Object);

            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            var result = await app.ExecuteAsync(new string[] { Command });

            Assert.That(result, Is.EqualTo(JenoCodes.Ok));
            passwordProvider.Verify(s => s.ReadInput("password", true), Times.AtLeastOnce);
        }

        [Test]
        public async Task PassJobParameters_RunJubWithCustomParameters()
        {
            var parameters = new List<string>
            {
                "runUnitTests=true",
                "buildType=Quick",
                "sendEmail=true"
            };

            var client = new MockHttpMessageHandler();
            client.Expect($"{JenkinsUrl}/job/{DefaultJob}/{Branch}/buildWithParameters?{string.Join("&", parameters)}")
                .Respond(HttpStatusCode.OK);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new RunJob(GetDefaultGitMock().Object,
                GetEncryptorMock().Object,
                GetUserConsoleMock().Object,
                httpClientFactory.Object,
                GetOptionsMock().Object);

            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            parameters.Insert(0, Command);

            var code = await app.ExecuteAsync(parameters.ToArray());

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
        }

        [Test]
        public async Task PassIncorrectJobParameters_InformAboutInvalidParameters()
        {
            var parameters = new List<string>
            {
                "runUnitTeststrue",
                "buildType=Quick",
                "sendEmail=true"
            };

            var client = new MockHttpMessageHandler();
            client.Expect($"{JenkinsUrl}/job/{DefaultJob}/{Branch}/buildWithParameters?{string.Join("&", parameters)}")
                .Respond(HttpStatusCode.OK);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new RunJob(GetDefaultGitMock().Object,
                GetEncryptorMock().Object,
                GetUserConsoleMock().Object,
                httpClientFactory.Object,
                GetOptionsMock().Object);

            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            parameters.Insert(0, Command);

            Assert.That(async () => await app.ExecuteAsync(parameters.ToArray()), Throws.TypeOf<JenoException>()
                .With.Property(nameof(JenoException.ExitCode)).EqualTo(JenoCodes.DefaultError)
                .And.Property(nameof(JenoException.Message)).Contains("Some of job parameters have incorrect format")
                .And.Property(nameof(JenoException.Message)).Contains("runUnitTeststrue")
                .And.Property(nameof(JenoException.Message)).Not.Contains("sendEmail=true"));
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
                        { "firstExampleRepoUrl", "firstExampleJob" },
                        { "secondExampleRepoUrl", "secondExampleJob" },
                        { DefaultKey,  DefaultJob},
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
            gitWrapper.Setup(s => s.GetCurrentBranch(It.IsAny<string>()))
                .Returns(Task.FromResult(Branch));

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