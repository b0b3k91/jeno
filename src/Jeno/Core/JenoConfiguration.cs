using System;
using System.Collections.Generic;
using System.Text;

namespace Jeno.Core
{
    class JenoConfiguration
    {
        public string JenkinsUrl { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
        public Dictionary<string, string> Repositories { get; set; }
    }
}
