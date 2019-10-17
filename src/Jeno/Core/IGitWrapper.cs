namespace Jeno.Core
{
    internal interface IGitWrapper
    {
        string GetRepoUrl(string repoPath);

        string GetCurrentBranch(string repoPath);
    }
}