using McMaster.Extensions.CommandLineUtils;
using System;

namespace Jeno.Core
{
    internal interface IJenoCommand
    {
        string Name { get; }
        Action<CommandLineApplication> Command { get; }
    }
}