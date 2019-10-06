using System;
using System.Collections.Generic;
using System.Text;

namespace Jeno.Core
{
    interface IGitWrapper
    {
        string GetRepoUrl(string repoPath);

        string GetCurrentBranch(string repoPath);
    }
}
