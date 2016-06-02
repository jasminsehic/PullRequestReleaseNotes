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
            var unreleasedCommits = GetAllUnreleasedCommits();
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

        private IEnumerable<Commit> GetAllUnreleasedCommits()
        {
            IEnumerable<Commit> commitSet = new List<Commit>();
            var tags = _programArgs.LocalGitRepository.Tags.Where(LightOrAnnotatedTags())
               .Select(tag => tag.Target as Commit).Where(x => x != null);
           var tagCommits = tags as IList<Commit> ?? tags.ToList();
           foreach (var tagCommit in tagCommits)
           {
                var commits = _programArgs.LocalGitRepository.Commits.QueryBy(new CommitFilter
                {
                    Since = _programArgs.LocalGitRepository.Branches[_programArgs.ReleaseBranchRef],
                    Until = tagCommit
                });
                commitSet = commitSet.Concat(commits);
            }
            commitSet = commitSet.Distinct();
            // then for each tagged commit traverse down its parents and remove them from commit set as they are considered released
            foreach (var tagCommit in tagCommits)
            {
                var commitFilter = new CommitFilter { Since = tagCommit.Id };
                var commitsToExclude = _programArgs.LocalGitRepository.Commits.QueryBy(commitFilter);
                commitSet = commitSet.Except(commitsToExclude);
            }
            return commitSet;
        }

        private Func<Tag, bool> LightOrAnnotatedTags()
        {
            if (_programArgs.GitTagsAnnotated)
                return t => t.IsAnnotated;
            return t => true;
        }
    }
}