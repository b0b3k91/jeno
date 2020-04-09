using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace Jeno.UnitTests
{
    [TestFixture]
    class ScanPipelineTests
    {
        [Test]
        public void RunScanOutsideRepository_InformAboutIt()
        {
            Assert.Fail();
        }

        [Test]
        public void CannotFindMatchedRepo_InformAboutIt()
        {
            Assert.Fail();
        }

        [Test]
        public void RunScanFromMappedRepository_CreateProperUrl()
        {
            Assert.Fail();
        }

        [Test]
        public void ScanWasSuccessful_ReturnOkCode()
        {
            Assert.Fail();
        }

        [Test]
        public void SomeErrorOccuredInScanRequest_DisplayErrorMessage()
        {
            Assert.Fail();
        }
    }
}
