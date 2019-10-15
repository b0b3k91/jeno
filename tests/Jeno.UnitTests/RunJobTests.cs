using NUnit.Framework;

namespace Jeno.UnitTests
{
    [TestFixture]
    public class RunJobTests
    {
        [Test]
        public void PassUndefinedRepository_RunInfrastructureJob()
        {
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
    }
}