using System.Threading.Tasks;

namespace Jeno.Core
{
    internal interface IGitWrapper
    {
        Task<string> GetRepoUrl(string repoPath);

        Task<string> GetCurrentBranch(string repoPath);
    }
}