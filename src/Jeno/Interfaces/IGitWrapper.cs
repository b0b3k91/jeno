using System.Threading.Tasks;

namespace Jeno.Interfaces
{
    internal interface IGitWrapper
    {
        Task<string> GetRepoUrl(string repoPath);

        Task<string> GetCurrentBranch(string repoPath);
    }
}