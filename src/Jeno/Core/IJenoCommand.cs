using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jeno.Core
{
    interface IJenoCommand
    {
        string Name { get; }
        Action<CommandLineApplication> Command { get; }
    }
}
