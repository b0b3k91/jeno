using Jeno.Commands;
using Jeno.Core;
using Jeno.Interfaces;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Options;
using Moq;
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
        [Test]
        public async Task PassUndefinedRepository_RunDefaultJob()
        {
            var configuration = GetDefaultConfiguration();
            var options = new Mock<IOptions<JenoConfiguration>>();
            options.Setup(c => c.Value)
                .Returns(configuration);

            var gitWrapper = new Mock<IGitWrapper>();
            gitWrapper.Setup(s => s.GetRepoUrl(It.IsAny<string>()))
                .Returns(Task.FromResult("fifthExampleRepo"));
            gitWrapper.Setup(s => s.GetCurrentBranch(It.IsAny<string>()))
                .Returns(Task.FromResult("master"));


            var client = new MockHttpMessageHandler();
            client.When("http://jenkins_host:8080/job/defaultJob/job/master")
                .Respond(HttpStatusCode.OK);
            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>()))
                .Returns(client.ToHttpClient());

            var command = new RunJob(gitWrapper.Object, httpClientFactory.Object, options.Object);

            var app = new CommandLineApplication();
            app.Command(command.Name, command.Command);
            var code = await app.ExecuteAsync(new string[] { "run" });

            Assert.AreEqual(JenoCodes.Ok, code);
        }

        [Test]
        public void PassRepositoryDefinedInConfiguration_RunJobSavedInConfiguration()
        {
        }

        [Test]
        public void MissingTokenAndUsername_AskForUsername()
        {
        }

        [Test]
        public void MissingTokenInConfiguration_InformUserAndShowLinkToTokenGenerator()
        {
        }

        [Test]
        public void RunJobWithoutParameters_RunJobWithDefaultConfiguration()
        {
        }

        [Test]
        public void PassJobParameters_RunJubWithCustomParameters()
        {
        }

        private JenoConfiguration GetDefaultConfiguration()
        {
            return new JenoConfiguration
            {
                JenkinsUrl = "http://jenkins_host:8080",
                Username = "jDoe",
                Token = "5om3r4nd0mt0k3n",
                Repositories = new Dictionary<string, string>()
                {
                    { "firstExampleRepoUrl", "firstExampleJob" },
                    { "secondExampleRepoUrl", "secondExampleJob" },
                    { "thirdExampleRepoUrl", "thirdExampleJob" },
                    { "fourthExampleRepoUrl", "fourthExampleJob" },
                    { "default", "defaultJob" },
                }
            };
        }
    }
}