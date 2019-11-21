using System;
using System.Net.Http.Headers;
using System.Text;

namespace Jeno.Infrastructure
{
    public class BasicAuthenticationHeader : AuthenticationHeaderValue
    {
        public BasicAuthenticationHeader(string userName, string password)
            : base("Basic", EncodedCredentials(userName, password))
        {
        }

        private static string EncodedCredentials(string userName, string password)
        {
            return Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes($"{userName}:{password}"));
        }
    }
}