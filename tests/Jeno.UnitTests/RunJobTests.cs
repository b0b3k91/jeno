using Jeno.Commands;
using Jeno.Core;
using Jeno.Interfaces;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using System;
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
        private readonly string _defaultKey = "default";
        private readonly string _defaultJob = "defaultJob";

        private readonly string _branch = "master";

        [Test]
        public async Task PassUndefinedRepository_RunDefaultJob()
        {
            var undefinedRepository = "fifthExampleRepo";
            
            var configuration = new JenoConfiguration
            {
                JenkinsUrl = _jenkinsUrl,
                Repositories = new Dictionary<string, string>()
                {
                    { "firstExampleRepoUrl", "firstExampleJob" },
                    { "secondExampleRepoUrl", "secondExampleJob" },
                    { _defaultKey,  _defaultJob},
                }
            };

            var options = new Mock<IOptions<JenoConfiguration>>();
            options.Setup(c => c.Value)
                .Returns(configuration);

            var gitWrapper = new Mock<IGitWrapper>();
            gitWrapper.Setup(s => s.GetRepoUrl(It.IsAny<string>()))
                .Returns(Task.FromResult(undefinedRepository));
            gitWrapper.Setup(s => s.GetCurrentBranch(It.IsAny<string>()))
                .Returns(Task.FromResult(_branch));


            var client = new MockHttpMessageHandler();
            client.When($"{_jenkinsUrl}/job/{_defaultJob}/{_branch}")
                .Respond(HttpStatusCode.OK);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new RunJob(gitWrapper.Object, httpClientFactory.Object, options.Object);

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
                Repositories = new Dictionary<string, string>()
                {
                    { exampleRepo, exampleJob },
                    { "secondExampleRepoUrl", "secondExampleJob" },
                    { _defaultKey,  _defaultJob},
                }
            };

            var options = new Mock<IOptions<JenoConfiguration>>();
            options.Setup(c => c.Value)
                .Returns(configuration);

            var gitWrapper = new Mock<IGitWrapper>();
            gitWrapper.Setup(s => s.GetRepoUrl(It.IsAny<string>()))
                .Returns(Task.FromResult(exampleRepo));
            gitWrapper.Setup(s => s.GetCurrentBranch(It.IsAny<string>()))
                .Returns(Task.FromResult(_branch));

            var client = new MockHttpMessageHandler();
            client.When($"{_jenkinsUrl}/job/{exampleJob}/{_branch}")
                .Respond(HttpStatusCode.OK);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new RunJob(gitWrapper.Object, httpClientFactory.Object, options.Object);

            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);
            var code = await app.ExecuteAsync(new string[] { _command });

            Assert.That(code, Is.EqualTo(JenoCodes.Ok));
        }

        [Test]
        public async Task RunJobWithoutMainUrl_InformAboutIt()
        {
            var configuration = new JenoConfiguration
            {
                JenkinsUrl = string.Empty,
                Repositories = new Dictionary<string, string>()
                {
                    { "firstExampleRepoUrl", "firstExampleJob" },
                    { "secondExampleRepoUrl", "secondExampleJob" },
                    { _defaultKey, _defaultJob },
                }
            };

            var options = new Mock<IOptions<JenoConfiguration>>();
            options.Setup(c => c.Value)
                .Returns(configuration);

            var gitWrapper = new Mock<IGitWrapper>();
            gitWrapper.Setup(s => s.GetRepoUrl(It.IsAny<string>()))
                .Returns(Task.FromResult(_defaultKey));
            gitWrapper.Setup(s => s.GetCurrentBranch(It.IsAny<string>()))
                .Returns(Task.FromResult(_branch));

            var client = new MockHttpMessageHandler();
            client.When($"{_jenkinsUrl}/job/{_defaultJob}/job/{_branch}")
                .Respond(HttpStatusCode.OK);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new RunJob(gitWrapper.Object, httpClientFactory.Object, options.Object);

            Assert.That(async () =>
            {
                var app = new CommandLineApplication();
                app.Command(command.Name, command.Command);
                await app.ExecuteAsync(new string[] { _command });
            },
            Throws.TypeOf<JenoException>()
            .With.Property("ExitCode").EqualTo(JenoCodes.DefaultError)
            .And.Property("Message").StartsWith("Jenkins address is undefined or incorrect"));
        }

        [Test]
        public async Task RunJobWithIncorrectJenkinsUrl_InformAboutIt()
        {
            var configuration = new JenoConfiguration
            {
                JenkinsUrl = "incorrectUrl",
                Repositories = new Dictionary<string, string>()
                {
                    { "firstExampleRepoUrl", "firstExampleJob" },
                    { "secondExampleRepoUrl", "secondExampleJob" },
                    { _defaultKey, _defaultJob },
                }
            };

            var options = new Mock<IOptions<JenoConfiguration>>();
            options.Setup(c => c.Value)
                .Returns(configuration);

            var gitWrapper = new Mock<IGitWrapper>();
            gitWrapper.Setup(s => s.GetRepoUrl(It.IsAny<string>()))
                .Returns(Task.FromResult(_defaultKey));
            gitWrapper.Setup(s => s.GetCurrentBranch(It.IsAny<string>()))
                .Returns(Task.FromResult(_branch));

            var client = new MockHttpMessageHandler();
            client.When($"{_jenkinsUrl}/job/{_defaultJob}/job/{_branch}")
                .Respond(HttpStatusCode.OK);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new RunJob(gitWrapper.Object, httpClientFactory.Object, options.Object);
            var exception = Assert.ThrowsAsync<JenoException>(async () =>
            {
                var app = new CommandLineApplication();
                app.Command(command.Name, command.Command);
                await app.ExecuteAsync(new string[] { _command });
            });

            Assert.That(exception.ExitCode, Is.EqualTo(JenoCodes.DefaultError));
            Assert.That(exception.Message, Does.StartWith("Jenkins address is undefined or incorrect"));
        }

        [Test]
        public async Task RunJobWithUndefinedDefaultJob_InformAboutIt()
        {
            var configuration = new JenoConfiguration
            {
                JenkinsUrl = _jenkinsUrl,
                Repositories = new Dictionary<string, string>()
                {
                    { "firstExampleRepoUrl", "firstExampleJob" },
                    { "secondExampleRepoUrl", "secondExampleJob" },
                }
            };

            var options = new Mock<IOptions<JenoConfiguration>>();
            options.Setup(c => c.Value)
                .Returns(configuration);

            var gitWrapper = new Mock<IGitWrapper>();
            gitWrapper.Setup(s => s.GetRepoUrl(It.IsAny<string>()))
                .Returns(Task.FromResult(_defaultKey));
            gitWrapper.Setup(s => s.GetCurrentBranch(It.IsAny<string>()))
                .Returns(Task.FromResult(_branch));

            var client = new MockHttpMessageHandler();
            client.When($"{_jenkinsUrl}/job/{_defaultJob}/job/{_branch}")
                .Respond(HttpStatusCode.OK);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new RunJob(gitWrapper.Object, httpClientFactory.Object, options.Object);
            var exception = Assert.ThrowsAsync<JenoException>(async () =>
            {
                var app = new CommandLineApplication();
                app.Command(command.Name, command.Command);
                await app.ExecuteAsync(new string[] { _command });
            });

            Assert.That(exception.ExitCode, Is.EqualTo(JenoCodes.DefaultError));
            Assert.That(exception.Message, Does.StartWith("Missing default job"));
        }

        [Test]
        public async Task TryRunJobOnJenkinsWithCSRFProtection_InformAboutIt()
        {
            var configuration = new JenoConfiguration
            {
                JenkinsUrl = _jenkinsUrl,
                Repositories = new Dictionary<string, string>()
                {
                    { "firstExampleRepoUrl", "firstExampleJob" },
                    { "secondExampleRepoUrl", "secondExampleJob" },
                    { _defaultKey,  _defaultJob},
                }
            };

            var options = new Mock<IOptions<JenoConfiguration>>();
            options.Setup(c => c.Value)
                .Returns(configuration);

            var gitWrapper = new Mock<IGitWrapper>();
            gitWrapper.Setup(s => s.GetRepoUrl(It.IsAny<string>()))
                .Returns(Task.FromResult(_branch));
            gitWrapper.Setup(s => s.GetCurrentBranch(It.IsAny<string>()))
                .Returns(Task.FromResult(_branch));

            var client = new MockHttpMessageHandler();
            client.When($"{_jenkinsUrl}/job/{_defaultJob}/{_branch}")
                .Respond(s => Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Forbidden,
                    ReasonPhrase = "No valid crumb was included in the request"
                }));

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new RunJob(gitWrapper.Object, httpClientFactory.Object, options.Object);

            var exception = Assert.ThrowsAsync<JenoException>(async () =>
            {
                var app = new CommandLineApplication();
                app.Command(command.Name, command.Command);
                var code = await app.ExecuteAsync(new string[] { _command });
            });

            Assert.That(exception.ExitCode, Is.EqualTo(JenoCodes.DefaultError));
            Assert.That(exception.Message, Does.Contain("CSRF Protection"));
        }

        [Test]
        public async Task PassJobParameters_RunJubWithCustomParameters()
        {
            Assert.Fail("Unimplemented feature");
        }
    }
}