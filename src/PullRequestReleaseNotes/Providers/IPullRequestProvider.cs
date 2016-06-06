using System.Collections.Generic;
using PullRequestReleaseNotes.Models;

namespace PullRequestReleaseNotes.Providers
{
    public interface IPullRequestProvider
    {
        PullRequestDto Get(string commitMessage);
        List<PullRequestCommitDto> Commits(int pullRequestId);
        string PullRequestUrl(int pullRequestId);
        string PrefixedPullRequest(int pullRequestId);
        bool DiscoverRemote();
    }
}