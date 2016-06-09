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
            return unreleasedCommits.Where(commit => commit.Parents.Count() > 1)
                .Select(mergeCommit => _pullRequestProvider.Get(mergeCommit.Message))
                .Where(pullRequestDto => pullRequestDto != null).ToList();
        }

        private IEnumerable<Commit> GetAllUnreleasedCommits()
        {
            var releasedCommitsHash = new Dictionary<string, Commit>();
            var branchReference = _programArgs.LocalGitRepository.Branches[_programArgs.ReleaseBranchRef];
            var tagCommits = _programArgs.LocalGitRepository.Tags.Where(LightOrAnnotatedTags()).Select(tag => tag.Target as Commit).Where(x => x != null).ToList();
            IEnumerable<Commit> branchAncestors = _programArgs.LocalGitRepository.Commits.QueryBy(new CommitFilter { Since = branchReference });
            if (!tagCommits.Any())
                return branchAncestors;
            // for each tagged commit walk down all its parents and collect a dictionary of unique commits
            foreach (var tagCommit in tagCommits)
            {
                var releasedCommits =_programArgs.LocalGitRepository.Commits.QueryBy(new CommitFilter {Since = tagCommit.Id}).ToDictionary(i => i.Sha, i => i);
                releasedCommitsHash.MergeOverwrite(releasedCommits);
            }
            // remove released commits from the branch ancestor commits as they have been previously released
            return branchAncestors.Except(releasedCommitsHash.Values.AsEnumerable());
        }

        private Func<Tag, bool> LightOrAnnotatedTags()
        {
            if (_programArgs.GitTagsAnnotated)
                return t => t.IsAnnotated;
            return t => true;
        }
    }
}