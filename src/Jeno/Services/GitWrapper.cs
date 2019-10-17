using Jeno.Core;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Jeno.Services
{
    internal class GitWrapper : IGitWrapper
    {
        private const string _branchCommand = "branch";
        private const string _remoteAddressCommand = "config --get remote.origin.url";

        public async Task<string> GetRepoUrl(string repoPath)
        {
            return await RunGit(_remoteAddressCommand, repoPath);
        }

        public async Task<string> GetCurrentBranch(string repoPath)
        {
            var branch = await RunGit(_branchCommand, repoPath);

            return branch.Remove(0, 1)
            .Trim();
        }

        private async Task<string> RunGit(string command, string repoPath)
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
                        return await process.StandardOutput.ReadLineAsync();
                    }
                }

                throw new JenoException(process.StandardOutput.ReadLine(), process.ExitCode);
            }
        }
    }
}