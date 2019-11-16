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
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Jeno.UnitTests
{
    [TestFixture]
    public class RunJobTests
    {
        private readonly string _command = "run";

        private readonly string _jenkinsUrl = "http://jenkins_host:8080";
        private readonly string _userName = "jDoe";
        private readonly string _token = "5om3r4nd0mt0k3n";
        private readonly string _defaultKey = "default";
        private readonly string _defaultJob = "defaultJob";

        private readonly string _branch = "master";
        private readonly string _password = "qwerty123";

        private readonly string _crumbContentType = "application/json";
        private readonly CrumbHeader _crumbHeader = new CrumbHeader
        {
            Crumb = "hYwN6MK1RlHpinq963cOO0jdwdb8Flrn",
            CrumbRequestField = "Jenkins-Crumb"
        };

        [Test]
        public async Task PassUndefinedRepository_RunDefaultJob()
        {
            var undefinedRepository = "fifthExampleRepo";

            var gitWrapper = new Mock<IGitWrapper>();
            gitWrapper.Setup(s => s.IsGitRepository(It.IsAny<string>()))
                .Returns(Task.FromResult(true));
            gitWrapper.Setup(s => s.GetRepoUrl(It.IsAny<string>()))
                .Returns(Task.FromResult(undefinedRepository));
            gitWrapper.Setup(s => s.GetCurrentBranch(It.IsAny<string>()))
                .Returns(Task.FromResult(_branch));

            var client = new MockHttpMessageHandler();
            client.When($"{_jenkinsUrl}/job/{_defaultJob}/{_branch}/buildWithParameters")
                .Respond(HttpStatusCode.OK);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new RunJob(gitWrapper.Object, PasswordProviderMock, httpClientFactory.Object, GetOptionsMock());

            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);
            var code = await app.ExecuteAsync(new string[] { _command });

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
        }

        [Test]
        public async Task PassRepositoryDefinedInConfiguration_RunJobSavedInConfiguration()
        {
            var exampleRepo = "firstExampleRepoUrl";
            var exampleJob = "firstExampleJob";

            var configuration = new JenoConfiguration
            {
                JenkinsUrl = _jenkinsUrl,
                UserName = _userName,
                Token = _token,
                Repositories = new Dictionary<string, string>()
                {
                    { exampleRepo, exampleJob },
                    { "secondExampleRepoUrl", "secondExampleJob" },
                    { _defaultKey,  _defaultJob},
                }
            };

            var gitWrapper = new Mock<IGitWrapper>();
            gitWrapper.Setup(s => s.IsGitRepository(It.IsAny<string>()))
                .Returns(Task.FromResult(true));
            gitWrapper.Setup(s => s.GetRepoUrl(It.IsAny<string>()))
                .Returns(Task.FromResult(exampleRepo));
            gitWrapper.Setup(s => s.GetCurrentBranch(It.IsAny<string>()))
                .Returns(Task.FromResult(_branch));

            var client = new MockHttpMessageHandler();
            client.When($"{_jenkinsUrl}/job/{exampleJob}/{_branch}/buildWithParameters")
                .Respond(HttpStatusCode.OK);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new RunJob(gitWrapper.Object, PasswordProviderMock, httpClientFactory.Object, GetOptionsMock(configuration));

            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);
            var code = await app.ExecuteAsync(new string[] { _command });

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
        }

        [Test]
        public async Task RunJenoOutsideRepository_BreakExecution()
        {
            var gitWrapper = new Mock<IGitWrapper>();
            gitWrapper.Setup(s => s.IsGitRepository(It.IsAny<string>()))
                .Returns(Task.FromResult(false));
            gitWrapper.Setup(s => s.GetRepoUrl(It.IsAny<string>()))
                .Returns(Task.FromResult(_defaultKey));
            gitWrapper.Setup(s => s.GetCurrentBranch(It.IsAny<string>()))
                .Returns(Task.FromResult(_branch));

            var client = new MockHttpMessageHandler();
            client.When($"{_jenkinsUrl}/job/{_defaultJob}/{_branch}/buildWithParameters")
                .Respond(HttpStatusCode.OK);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new RunJob(gitWrapper.Object, PasswordProviderMock, httpClientFactory.Object, GetOptionsMock());

            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            Assert.That(async () => await app.ExecuteAsync(new string[] { _command }), Throws.TypeOf<JenoException>()
            .With.Property(nameof(JenoException.ExitCode)).EqualTo(JenoCodes.DefaultError)
            .And.Property(nameof(JenoException.Message)).Contains("Current directory is not git repository."));
        }

        [Test]
        public async Task MissingUserName_InformAboutIt()
        {
            var configuration = new JenoConfiguration
            {
                JenkinsUrl = _jenkinsUrl,
                UserName = string.Empty,
                Token = _token,
                Repositories = new Dictionary<string, string>()
                {
                    { "firstExampleRepoUrl", "firstExampleJob" },
                    { "secondExampleRepoUrl", "secondExampleJob" },
                    { _defaultKey, _defaultJob },
                }
            };

            var client = new MockHttpMessageHandler();
            client.When($"{_jenkinsUrl}/job/{_defaultJob}/job/{_branch}/buildWithParameters")
                .Respond(HttpStatusCode.OK);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new RunJob(DefaultGitMock, PasswordProviderMock, httpClientFactory.Object, GetOptionsMock(configuration));
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            Assert.That(async () => await app.ExecuteAsync(new string[] { _command }), Throws.TypeOf<JenoException>()
            .With.Property(nameof(JenoException.ExitCode)).EqualTo(JenoCodes.DefaultError)
            .And.Property(nameof(JenoException.Message)).Contains("Username is undefined"));
        }

        [Test]
        public async Task MissingTokenInConfiguration_InformUserAndShowLinkToTokenGenerator()
        {
            var configuration = new JenoConfiguration
            {
                JenkinsUrl = _jenkinsUrl,
                UserName = _userName,
                Token = string.Empty,
                Repositories = new Dictionary<string, string>()
                {
                    { "firstExampleRepoUrl", "firstExampleJob" },
                    { "secondExampleRepoUrl", "secondExampleJob" },
                    { _defaultKey, _defaultJob },
                }
            };

            var client = new MockHttpMessageHandler();
            client.When($"{_jenkinsUrl}/job/{_defaultJob}/job/{_branch}/buildWithParameters")
                .Respond(HttpStatusCode.OK);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new RunJob(DefaultGitMock, PasswordProviderMock, httpClientFactory.Object, GetOptionsMock(configuration));
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            Assert.That(async () => await app.ExecuteAsync(new string[] { _command }), Throws.TypeOf<JenoException>()
            .With.Property(nameof(JenoException.ExitCode)).EqualTo(JenoCodes.DefaultError)
            .And.Property(nameof(JenoException.Message)).Contains("User token is undefined")
            .And.Property(nameof(JenoException.Message)).Contain($"{_jenkinsUrl}/user/{_userName}/configure"));
        }

        [Test]
        public async Task RunJobWithoutDefinedMainUrl_InformAboutIt()
        {
            var configuration = new JenoConfiguration
            {
                JenkinsUrl = string.Empty,
                UserName = _userName,
                Token = _token,
                Repositories = new Dictionary<string, string>()
                {
                    { "firstExampleRepoUrl", "firstExampleJob" },
                    { "secondExampleRepoUrl", "secondExampleJob" },
                    { _defaultKey, _defaultJob },
                }
            };

            var client = new MockHttpMessageHandler();
            client.When($"{_jenkinsUrl}/job/{_defaultJob}/job/{_branch}/buildWithParameters")
                .Respond(HttpStatusCode.OK);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new RunJob(DefaultGitMock, PasswordProviderMock, httpClientFactory.Object, GetOptionsMock(configuration));
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            Assert.That(async () => await app.ExecuteAsync(new string[] { _command }), Throws.TypeOf<JenoException>()
            .With.Property(nameof(JenoException.ExitCode)).EqualTo(JenoCodes.DefaultError)
            .And.Property(nameof(JenoException.Message)).Contains("Jenkins address is undefined or incorrect"));
        }

        [Test]
        public async Task RunJobWithIncorrectJenkinsUrl_InformAboutIt()
        {
            var configuration = new JenoConfiguration
            {
                JenkinsUrl = "incorrectUrl",
                UserName = _userName,
                Token = _token,
                Repositories = new Dictionary<string, string>()
                {
                    { "firstExampleRepoUrl", "firstExampleJob" },
                    { "secondExampleRepoUrl", "secondExampleJob" },
                    { _defaultKey, _defaultJob },
                }
            };

            var passwordProvider = new Mock<IPasswordProvider>();
            passwordProvider.Setup(s => s.GetPassword())
                .Returns(_password);

            var client = new MockHttpMessageHandler();
            client.When($"{_jenkinsUrl}/job/{_defaultJob}/job/{_branch}/buildWithParameters")
                .Respond(HttpStatusCode.OK);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new RunJob(DefaultGitMock, PasswordProviderMock, httpClientFactory.Object, GetOptionsMock(configuration));
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            Assert.That(async () => await app.ExecuteAsync(new string[] { _command }), Throws.TypeOf<JenoException>()
            .With.Property(nameof(JenoException.ExitCode)).EqualTo(JenoCodes.DefaultError)
            .And.Property(nameof(JenoException.Message)).Contains("Jenkins address is undefined or incorrect"));
        }

        [Test]
        public async Task RunJobWithUndefinedDefaultJob_InformAboutIt()
        {
            var configuration = new JenoConfiguration
            {
                JenkinsUrl = _jenkinsUrl,
                UserName = _userName,
                Token = _token,
                Repositories = new Dictionary<string, string>()
                {
                    { "firstExampleRepoUrl", "firstExampleJob" },
                    { "secondExampleRepoUrl", "secondExampleJob" },
                }
            };

            var client = new MockHttpMessageHandler();
            client.When($"{_jenkinsUrl}/job/{_defaultJob}/job/{_branch}/buildWithParameters")
                .Respond(HttpStatusCode.OK);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new RunJob(DefaultGitMock, PasswordProviderMock, httpClientFactory.Object, GetOptionsMock(configuration));
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            Assert.That(async () => await app.ExecuteAsync(new string[] { _command }), Throws.TypeOf<JenoException>()
            .With.Property(nameof(JenoException.ExitCode)).EqualTo(JenoCodes.DefaultError)
            .And.Property(nameof(JenoException.Message)).Contains("Missing default job"));
        }

        [Test]
        public async Task TryRunJobOnJenkinsWithCSRFProtection_UseBasicCredentialsAndAskForCrumb()
        {
            var basicAuthHeader = new BasicAuthenticationHeader(_userName, _password);
            var tokenAuthHeader = new BearerAuthenticationHeader(_token);

            var client = new MockHttpMessageHandler();
            client.Expect($"{_jenkinsUrl}/job/{_defaultJob}/{_branch}/buildWithParameters")
                .WithHeaders("Authorization", $"{tokenAuthHeader.Scheme} {tokenAuthHeader.Parameter}")
                .Respond(request => new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    ReasonPhrase = "No valid crumb was included in the request"
                });
            client.Expect($"{_jenkinsUrl}/crumbIssuer/api/json")
                .WithHeaders("Authorization", $"{basicAuthHeader.Scheme} {basicAuthHeader.Parameter}")
                .Respond(_crumbContentType, JsonConvert.SerializeObject(_crumbHeader));
            client.Expect($"{_jenkinsUrl}/job/{_defaultJob}/{_branch}/buildWithParameters")
                .WithHeaders("Authorization", $"{basicAuthHeader.Scheme} {basicAuthHeader.Parameter}")
                .WithHeaders(_crumbHeader.CrumbRequestField, _crumbHeader.Crumb)
                .Respond(HttpStatusCode.OK);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new RunJob(DefaultGitMock, PasswordProviderMock, httpClientFactory.Object, GetOptionsMock());
            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            var result = await app.ExecuteAsync(new string[] { _command });

            Assert.That(result, Is.EqualTo(JenoCodes.Ok));
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
            client.Expect($"{_jenkinsUrl}/job/{_defaultJob}/{_branch}/buildWithParameters?{string.Join("&", parameters)}")
                .Respond(HttpStatusCode.OK);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new RunJob(DefaultGitMock, PasswordProviderMock, httpClientFactory.Object, GetOptionsMock());

            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            parameters.Insert(0, _command);

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
            client.Expect($"{_jenkinsUrl}/job/{_defaultJob}/{_branch}/buildWithParameters?{string.Join("&", parameters)}")
                .Respond(HttpStatusCode.OK);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new RunJob(DefaultGitMock, PasswordProviderMock, httpClientFactory.Object, GetOptionsMock());

            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);

            parameters.Insert(0, _command);

            Assert.That(async () => await app.ExecuteAsync(parameters.ToArray()), Throws.TypeOf<JenoException>()
                .With.Property(nameof(JenoException.ExitCode)).EqualTo(JenoCodes.DefaultError)
                .And.Property(nameof(JenoException.Message)).Contains("Some of job parameters have incorrect format")
                .And.Property(nameof(JenoException.Message)).Contains("runUnitTeststrue")
                .And.Property(nameof(JenoException.Message)).Not.Contains("sendEmail=true"));
        }

        private IOptions<JenoConfiguration> GetOptionsMock(JenoConfiguration configuration = null)
        {
            if (configuration == null)
            {
                configuration = new JenoConfiguration
                {
                    JenkinsUrl = _jenkinsUrl,
                    UserName = _userName,
                    Token = _token,
                    Repositories = new Dictionary<string, string>()
                    {
                        { "firstExampleRepoUrl", "firstExampleJob" },
                        { "secondExampleRepoUrl", "secondExampleJob" },
                        { _defaultKey,  _defaultJob},
                    }
                };
            }

            var options = new Mock<IOptions<JenoConfiguration>>();
            options.Setup(c => c.Value)
                .Returns(configuration);

            return options.Object;
        }

        private IGitWrapper DefaultGitMock
        {
            get
            {
                var gitWrapper = new Mock<IGitWrapper>();
                gitWrapper.Setup(s => s.IsGitRepository(It.IsAny<string>()))
                    .Returns(Task.FromResult(true));
                gitWrapper.Setup(s => s.GetRepoUrl(It.IsAny<string>()))
                    .Returns(Task.FromResult(_defaultKey));
                gitWrapper.Setup(s => s.GetCurrentBranch(It.IsAny<string>()))
                    .Returns(Task.FromResult(_branch));

                return gitWrapper.Object;
            }
        }

        private IPasswordProvider PasswordProviderMock
        {
            get
            {
                var passwordProvider = new Mock<IPasswordProvider>();
                passwordProvider.Setup(s => s.GetPassword())
                    .Returns(_password);

                return passwordProvider.Object;
            }
        }
    }
}