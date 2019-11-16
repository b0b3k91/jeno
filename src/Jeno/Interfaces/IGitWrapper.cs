using System.Threading.Tasks;

namespace Jeno.Interfaces
{
    public interface IGitWrapper
    {
        Task<bool> IsGitRepository(string repoPath);

        Task<string> GetRepoUrl(string repoPath);

        Task<string> GetCurrentBranch(string repoPath);
    }
}