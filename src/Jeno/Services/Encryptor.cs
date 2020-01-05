using Jeno.Interfaces;
using System;
using System.Collections.Generic;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace Jeno.Services
{
    public class Encryptor : IEncryptor
    {
        private readonly byte[] _entropyBytes;

        public Encryptor()
        {
            using (var sha = SHA256.Create())
            {
                var mos = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
                foreach (var mo in mos.Get())
                {
                    var number = mo["SerialNumber"].ToString();
                    _entropyBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(mo["SerialNumber"].ToString()));
                }
            }
        }

        public string Decrypt(string encryptedString)
        {
            var encryptedByteArray = Convert.FromBase64String(encryptedString);
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
