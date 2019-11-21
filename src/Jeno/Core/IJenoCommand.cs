using McMaster.Extensions.CommandLineUtils;
using System;

namespace Jeno.Core
{
    public interface IJenoCommand
    {
        string Name { get; }
        Action<CommandLineApplication> Command { get; }
    }
}