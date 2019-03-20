using System;
using System.Linq;
using LibGit2Sharp;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PullRequestReleaseNotes.Models;
using PullRequestReleaseNotes.Providers;

namespace PullRequestReleaseNotes
{
    public class PullRequestHistoryBuilder
    {
        private readonly ProgramArgs _programArgs;
        private readonly IPullRequestProvider _pullRequestProvider;
        private static readonly Regex ParseSemVer = new Regex(@"^(?<SemVer>(?<Major>\d+)(\.(?<Minor>\d+))(\.(?<Patch>\d+))?)(\.(?<FourthPart>\d+))?(-(?<Tag>[^\+]*))?(\+(?<BuildMetaData>.*))?$", RegexOptions.Compiled);

        public PullRequestHistoryBuilder(ProgramArgs programArgs)
        {
            _programArgs = programArgs;
            _pullRequestProvider = programArgs.PullRequestProvider;
        }

        public List<PullRequestDto> BuildHistory()
        {
            var unreleasedCommits = GetAllUnreleasedMergeCommits();
            return unreleasedCommits.Select(mergeCommit => _pullRequestProvider.Get(mergeCommit.Message))
                .Where(pullRequestDto => pullRequestDto != null).ToList();
        }

        private IEnumerable<Commit> GetAllUnreleasedMergeCommits()
        {
            var releasedCommitsHash = new Dictionary<string, Commit>();
            var branchReference = _programArgs.LocalGitRepository.Branches[_programArgs.ReleaseBranchRef];
            var tagCommits = _programArgs.LocalGitRepository.Tags
                .Where(LightOrAnnotatedTags())
                .Where(t => ParseSemVer.Match(t.FriendlyName).Success)
                .Select(tag => tag.Target as Commit).Where(x => x != null).ToList();
            var branchAncestors = _programArgs.LocalGitRepository.Commits
                .QueryBy(new CommitFilter { ExcludeReachableFrom = branchReference })
                .Where(commit => commit.Parents.Count() > 1);
            if (!tagCommits.Any())
                return branchAncestors;
            // for each tagged commit walk down all its parents and collect a dictionary of unique commits
            foreach (var tagCommit in tagCommits)
            {
                // we only care about tags descending from the branch we are interested in
                if (_programArgs.LocalGitRepository.Commits.QueryBy(new CommitFilter { ExcludeReachableFrom = branchReference }).Any(c => c.Sha == tagCommit.Sha))
                {
                    var releasedCommits = _programArgs.LocalGitRepository.Commits
                        .QueryBy(new CommitFilter { ExcludeReachableFrom = tagCommit.Id })
                        .Where(commit => commit.Parents.Count() > 1)
                        .ToDictionary(i => i.Sha, i => i);
                    releasedCommitsHash.Merge(releasedCommits);
                }
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