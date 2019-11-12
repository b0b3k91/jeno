using System;
using System.Collections.Generic;
using System.Text;

namespace Jeno.Infrastructure
{
    public class CrumbHeader
    {
        public string Crumb { get; set; }
        public string CrumbRequestField { get; set; }
    }
}
