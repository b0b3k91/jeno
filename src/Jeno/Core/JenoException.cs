using System;

namespace Jeno.Core
{
    internal class JenoException : ApplicationException
    {
        public int ExitCode { get; }

        public JenoException(int exitCode = 1)
        {
            ExitCode = exitCode;
        }

        public JenoException(string message, int exitCode = 1) : base(message)
        {
            ExitCode = exitCode;
        }
    }
}