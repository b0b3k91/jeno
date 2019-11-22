using System.Threading.Tasks;

namespace Jeno.Interfaces
{
    public interface IGitClient
    {
        Task<bool> IsGitRepository(string repoPath);

        Task<string> GetRepoName(string repoPath);

        Task<string> GetCurrentBranch(string repoPath);
    }
}