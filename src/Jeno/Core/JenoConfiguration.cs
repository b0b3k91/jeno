using System.Collections.Generic;

namespace Jeno.Core
{
    public class JenoConfiguration
    {
        public string JenkinsUrl { get; set; }
        public string UserName { get; set; }
        public string Token { get; set; }
        public Dictionary<string, string> Repositories { get; set; }
    }
}