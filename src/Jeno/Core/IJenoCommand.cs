using System;
using McMaster.Extensions.CommandLineUtils;

namespace Jeno.Core
{
    interface IJenoCommand
    {
        string Name { get; }
        Action<CommandLineApplication> Command { get; }
    }
}
