using System;
using System.Diagnostics;

namespace Jeno
{
    class GitWrapper
    {
        private const string _processName = "git";

        private const string _branchCommand = "branch";
        private const string _remoteAddressCommand = "config --get remote.origin.url";

        public Uri GetRepoUrl(string repoPath)
        {
            return new Uri(RunGit(_remoteAddressCommand, repoPath));
        }

        public string GetCurrentBranch(string repoPath)
        {
            return RunGit(_branchCommand, repoPath)
                .Remove(0,1)
                .Trim();
        }

        private string RunGit(string command, string repoPath)
        {
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = _processName,
                    Arguments = command,
                    WorkingDirectory = repoPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                process.Start();

                while (!process.StandardOutput.EndOfStream)
                {
                    return process.StandardOutput.ReadLine();
                }

                throw new Exception("Cannot get response from git process");
            }
        }
    }
}
