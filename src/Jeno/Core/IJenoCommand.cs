using System;
using McMaster.Extensions.CommandLineUtils;

namespace Jeno.Core
{
    public interface IJenoCommand
    {
        string Name { get; }
        Action<CommandLineApplication> Command { get; }
    }
}