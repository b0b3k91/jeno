using NUnit.Framework;

namespace Jeno.UnitTests
{
    public class RunJobTests
    {
        [Test]
        public void PassDefaultRepository_RunInfrastructureJob()
        {
        }

        [Test]
        public void PassSpecificRepository_RunJobSavedInConfiguration()
        {
        }

        [Test]
        public void MissingTokenInConfiguration_InformUserAndShowLinkToTokenGenerator()
        {
        }

        [Test]
        public void PassJobParameters_RunJubWithCustomParameters()
        {
        }
    }
}