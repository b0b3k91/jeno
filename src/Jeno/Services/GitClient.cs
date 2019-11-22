using Jeno.Core;
using Jeno.Interfaces;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Jeno.Services
{
    internal class GitClient : IGitClient
    {
        public async Task<bool> IsGitRepository(string repoPath)
        {
            try
            {
                var result = (await RunGit("rev-parse --is-inside-work-tree", repoPath))
                    .Replace("\n", string.Empty);

                return Convert.ToBoolean(result);
            }
            catch (JenoException)
            {
                return false;
            }
        }

        public async Task<string> GetRepoName(string repoPath)
        {
            return (await RunGit("config --get remote.origin.url", repoPath))
                .Replace("\n", string.Empty)
                .Split('/')
                .Last()
                .Replace(".git", string.Empty);
        }

        public async Task<string> GetCurrentBranch(string repoPath)
        {
            return (await RunGit("branch", repoPath))
                .Split("\n")
                .Single(s => s.Contains("*"))
                .Remove(0, 1)
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

                while (!process.HasExited)
                {
                }

                if (process.ExitCode == 0)
                {
                    return await process.StandardOutput.ReadToEndAsync();
                }

                throw new JenoException(await process.StandardOutput.ReadLineAsync(), process.ExitCode);
            }
        }
    }
}