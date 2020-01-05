using Jeno.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jeno.UnitTests
{
    [TestFixture]
    class EncryptionTests
    {
        private const string Password = "Qwerty123";

        [Test]
        public void EndcryptAndDecryptPassword_GetNewPasswordEqualToOriginal()
        {
            var encryptor = new Encryptor();

            var encryptedPassword = encryptor.Encrypt(Password);
            var decryptedPassword = encryptor.Decrypt(encryptedPassword);

            Assert.That(decryptedPassword, Is.EqualTo(Password));
        }

        [Test]
        public void UseDifferentEncryptorInstanceForDecrypt_GetCorrectPassword()
        {
            var encryptor = new Encryptor();
            var decryptor = new Encryptor();

            var encryptedPass = encryptor.Encrypt(Password);
            var decryptedPass = decryptor.Decrypt(encryptedPass);

            Assert.That(decryptedPass, Is.EqualTo(Password));
        }
    }
}
