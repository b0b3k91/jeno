using System;
using System.Security.Cryptography;
using System.Text;
using Jeno.Interfaces;

namespace Jeno.Services
{
    public class Encryptor : IEncryptor
    {
        private readonly byte[] _entropyBytes = Encoding.ASCII.GetBytes("Does the Pope shit in the woods?");

        public string Decrypt(string encryptedValue)
        {
            var encryptedByteArray = Convert.FromBase64String(encryptedValue);
            var decryptedByteArray = ProtectedData.Unprotect(encryptedByteArray, _entropyBytes, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(decryptedByteArray);
        }

        public string Encrypt(string value)
        {
            var decryptedByteArray = Encoding.UTF8.GetBytes(value);
            var encryptedByteArray = ProtectedData.Protect(decryptedByteArray, _entropyBytes, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedByteArray);
        }
    }
}