using NUnit.Framework;

namespace Jeno.UnitTests
{
    [TestFixture]
    internal class ChangeConfigurationTests
    {
        [Test]
        public void TryToSetUndefinedParameter_InformUserAboutAvailableParameters()
        {
        }

        [Test]
        public void SetUncorrectJenkinsUrl_InformUser()
        {
        }

        [Test]
        public void AddNewRepository_SaveRepositoryInConfiguration()
        {
        }

        [Test]
        public void EditSavedRepository_ReplaceOldValue()
        {
        }

        [Test]
        public void UseDeleteParameter_RemovePassedRepositoryFromFonciguration()
        {
        }

        [Test]
        public void UseDeleteAliasParameter_RemovePassedRepositoryFromConniguration()
        {
        }

        [Test]
        public void TryDeteleDefaultJob_ThrowExcetpion()
        {
        }
    }
}