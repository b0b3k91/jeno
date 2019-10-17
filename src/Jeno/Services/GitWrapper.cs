using Jeno.Core;
using System.Diagnostics;

namespace Jeno.Services
{
    internal class GitWrapper : IGitWrapper
    {
        private const string _branchCommand = "branch";
        private const string _remoteAddressCommand = "config --get remote.origin.url";

        public string GetRepoUrl(string repoPath)
        {
            return RunGit(_remoteAddressCommand, repoPath);
        }

        public string GetCurrentBranch(string repoPath)
        {
            return RunGit(_branchCommand, repoPath)
                .Remove(0, 1)
                .Trim();
        }

        private string RunGit(string command, string repoPath)
        {
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = command,
                    WorkingDirectory = repoPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                process.Start();

                while (!process.StandardOutput.EndOfStream)
                {
                    if (process.ExitCode == 0)
                    {
                        return process.StandardOutput.ReadLine();
                    }
                }

                throw new JenoException(process.StandardOutput.ReadLine(), process.ExitCode);
            }
        }
    }
}