using System;
using System.Linq;
using LibGit2Sharp;
using System.Collections.Generic;
using UnreleasedGitHubHistory.Models;
using UnreleasedGitHubHistory.Providers;

namespace UnreleasedGitHubHistory
{
    public class PullRequestHistoryBuilder
    {
        private readonly ProgramArgs _programArgs;
        private readonly IPullRequestProvider _pullRequestProvider;

        public PullRequestHistoryBuilder(ProgramArgs programArgs)
        {
            _programArgs = programArgs;
            _pullRequestProvider = programArgs.PullRequestProvider;
        }

        public List<PullRequestDto> BuildHistory()
        {
            var releaseHistory = new List<PullRequestDto>();

            var lastCommit = GetLastTaggedCommit(_programArgs.LocalGitRepository, _programArgs.ReleaseBranchRef);
            if (lastCommit == null)
            {
                Console.WriteLine($"Couldn't find the last commit to build history from");
                return null;
            }
            if (_programArgs.VerboseOutput)
                Console.WriteLine($"Building history for {_programArgs.ReleaseBranchRef} down to commit {lastCommit.Sha}");
            var unreleasedCommits = GetUnreleasedCommits(lastCommit);
            foreach (var mergeCommit in unreleasedCommits.Where(commit => commit.Parents.Count() > 1))
            {
                var pullRequestDto = _pullRequestProvider.Get(mergeCommit.Message);
                if (pullRequestDto == null) continue;
                if (pullRequestDto.Labels.Contains(_programArgs.FollowLabel, StringComparer.InvariantCultureIgnoreCase))
                    FollowChildPullRequests(pullRequestDto.Number, releaseHistory);
                else
                    releaseHistory.Add(pullRequestDto);
            }
            return OrderReleaseNotes(releaseHistory.Distinct(new PullRequestDtoEqualityComparer()).ToList());
        }

        private void FollowChildPullRequests(int parentPullRequest, List<PullRequestDto> releaseHistory)
        {
            var commits = _pullRequestProvider.Commits(parentPullRequest);
            foreach (var commit in commits.Where(c => c.Merge))
            {
                var pullRequestDto = _pullRequestProvider.Get(commit.Message);
                if (pullRequestDto == null)
                    continue;
                if (pullRequestDto.Labels.Contains(_programArgs.FollowLabel, StringComparer.InvariantCultureIgnoreCase))
                    FollowChildPullRequests(pullRequestDto.Number, releaseHistory);
                else
                    releaseHistory.Add(pullRequestDto);
            }
        }

        private List<PullRequestDto> OrderReleaseNotes(List<PullRequestDto> releaseHistory)
        {
            var orderWhenKey = OrderWhenKey();
            if (_programArgs.ReleaseNoteOrderAscending.Value)
                return releaseHistory.OrderByDescending(orderWhenKey).ToList();
            return releaseHistory.OrderBy(orderWhenKey).ToList();
        }

        private Func<PullRequestDto, DateTimeOffset?> OrderWhenKey()
        {
            if (_programArgs.ReleaseNoteOrderWhen.CaseInsensitiveContains("created"))
                return r => r.CreatedAt;
            return r => r.MergedAt;
        }

        private ICommitLog GetUnreleasedCommits(Commit startingCommit)
        {
            // Let's only consider the local branch refs that lead to this commit...
            var reachableRefs = _programArgs.LocalGitRepository.Refs.ReachableFrom(new[] {startingCommit})
                .Where(r => r.IsLocalBranch() && r.CanonicalName == _programArgs.ReleaseBranchRef);
            //...and create a filter that will retrieve all the commits...
            var commitFilter = new CommitFilter
            {
                Since = reachableRefs,  // ...reachable from refs...
                Until = startingCommit, // ...until this commit is met
                FirstParentOnly = true  // ...only follow our direct lineage
            };
            return _programArgs.LocalGitRepository.Commits.QueryBy(commitFilter);
        }

        private Commit GetLastTaggedCommit(IRepository repository, string branchName)
        {
            var branch = repository.Branches.FirstOrDefault(b => b.CanonicalName == branchName);
            var tags = repository.Tags.Where(LightOrAnnotatedTags()).ToArray();
            var olderThan = branch?.Tip.Author.When;
            var commitFilter = new CommitFilter { FirstParentOnly = true };
            var queriableCommits = branch?.Commits as IQueryableCommitLog;
            var lastTaggedCommit = queriableCommits?.QueryBy(commitFilter)
                .FirstOrDefault(c => c.Author.When <= olderThan && tags.Any(a => a.Target.Sha == c.Sha));
            return lastTaggedCommit ?? branch?.Commits.Last();
        }

        private Func<Tag, bool> LightOrAnnotatedTags()
        {
            if (_programArgs.GitTagsAnnotated)
                return t => t.IsAnnotated;
            return t => true;
        }
    }
}