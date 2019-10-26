using McMaster.Extensions.CommandLineUtils;
using System;

namespace Jeno.Interfaces
{
    public interface IJenoCommand
    {
        string Name { get; }
        Action<CommandLineApplication> Command { get; }
    }
}