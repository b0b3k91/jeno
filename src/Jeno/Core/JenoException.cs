using System;

namespace Jeno.Core
{
    public class JenoException : ApplicationException
    {
        public int ExitCode { get; }

        public JenoException(int exitCode = JenoCodes.DefaultError)
        {
            ExitCode = exitCode;
        }

        public JenoException(string message, int exitCode = JenoCodes.DefaultError) : base(message)
        {
            ExitCode = exitCode;
        }
    }
}