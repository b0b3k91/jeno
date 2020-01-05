using System;
using System.Collections.Generic;
using System.Text;

namespace Jeno.Interfaces
{
    public interface IEncryptor
    {
        string Encrypt(string value);
        string Decrypt(string encryptedString);
    }
}
