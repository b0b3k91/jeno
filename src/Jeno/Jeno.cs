using System;
using Jeno.Services;
using Jeno.Commands;

namespace Jeno
{
    class Jeno
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            var git = new GitWrapper();
            var branch = git.GetCurrentBranch(System.IO.Directory.GetCurrentDirectory());
            Console.WriteLine(branch);
        }
    }
}
