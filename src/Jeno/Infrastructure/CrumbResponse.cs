using System;
using System.Collections.Generic;
using System.Text;

namespace Jeno.Infrastructure
{
    public class CrumbResponse
    {
        public string Crumb { get; set; }
        public string CrumbRequestField { get; set; }
    }
}
