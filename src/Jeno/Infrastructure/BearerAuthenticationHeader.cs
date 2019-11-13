using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace Jeno.Infrastructure
{
    public class BearerAuthenticationHeader : AuthenticationHeaderValue
    {
        public BearerAuthenticationHeader(string token) : base("Bearer", token)
        {
        }
    }
}
