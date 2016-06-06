using System;
using System.Linq;
using LibGit2Sharp;
using System.Collections.Generic;
using PullRequestReleaseNotes.Models;
using PullRequestReleaseNotes.Providers;

namespace PullRequestReleaseNotes
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
            var unreleasedCommits = GetAllUnreleasedCommits();
            var releaseHistory = unreleasedCommits.Where(commit => commit.Parents.Count() > 1)
                .Select(mergeCommit => _pullRequestProvider.Get(mergeCommit.Message))
                .Where(pullRequestDto => pullRequestDto != null).ToList();
            return releaseHistory.Distinct(new PullRequestDtoEqualityComparer()).ToList();
        }

        private IEnumerable<Commit> GetAllUnreleasedCommits()
        {
            IEnumerable<Commit> unreleasedCommits = new List<Commit>();
            var tags = _programArgs.LocalGitRepository.Tags.Where(LightOrAnnotatedTags())
               .Select(tag => tag.Target as Commit).Where(x => x != null);
            var tagCommits = tags as IList<Commit> ?? tags.ToList();
            if (!tagCommits.Any())
            {
                return _programArgs.LocalGitRepository.Commits.QueryBy(new CommitFilter
                {
                    Since = _programArgs.LocalGitRepository.Branches[_programArgs.ReleaseBranchRef],
                });
            }
            // first fill it with all released and unreleased commits down to all tagged (release) commits
            foreach (var tagCommit in tagCommits)
            {
                var commits = _programArgs.LocalGitRepository.Commits.QueryBy(new CommitFilter
                {
                    Since = _programArgs.LocalGitRepository.Branches[_programArgs.ReleaseBranchRef],
                    Until = tagCommit
                });
                unreleasedCommits = unreleasedCommits.Concat(commits);
            }
            unreleasedCommits = unreleasedCommits.Distinct();
            // then for each tagged commit traverse further down all its parents and remove them from released/unreleased commits as they have been included in a release
            foreach (var tagCommit in tagCommits)
            {
                var releasedCommits = _programArgs.LocalGitRepository.Commits.QueryBy(new CommitFilter { Since = tagCommit.Id });
                unreleasedCommits = unreleasedCommits.Except(releasedCommits);
            }
            return unreleasedCommits;
        }

        private Func<Tag, bool> LightOrAnnotatedTags()
        {
            if (_programArgs.GitTagsAnnotated)
                return t => t.IsAnnotated;
            return t => true;
        }
    }
}