using System.Collections.Generic;
using PullRequestReleaseNotes.Models;

namespace PullRequestReleaseNotes.Providers
{
    public interface IPullRequestProvider
    {
        PullRequestDto Get(string commitMessage);
        string PullRequestUrl(int pullRequestId);
        string PrefixedPullRequest(int pullRequestId);
        bool DiscoverRemote();
    }
}