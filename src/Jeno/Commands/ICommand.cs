using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jeno.Commands
{
    interface ICommand
    {
        string Name { get; }
        Action<string> Command { get; }
    }
}
