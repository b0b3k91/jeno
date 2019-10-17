using System;
using System.Collections.Generic;
using System.Text;

namespace Jeno.Core
{
    class JenoException : ApplicationException
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
