using System.Net.Http.Headers;

namespace Jeno.Infrastructure
{
    public class BearerAuthenticationHeader : AuthenticationHeaderValue
    {
        public BearerAuthenticationHeader(string token) : base("Bearer", token)
        {
        }
    }
}