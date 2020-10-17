using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LibGit2Sharp;

namespace PullRequestReleaseNotes
{
    public class UnreleasedCommitsProvider
    {
        private static readonly Regex ParseSemVer = new Regex(@"^(?<SemVer>(?<Major>\d+)(\.(?<Minor>\d+))(\.(?<Patch>\d+))?)(\.(?<FourthPart>\d+))?(-(?<Tag>[^\+]*))?(\+(?<BuildMetaData>.*))?$", RegexOptions.Compiled);
        private static Func<Tag, bool> LightOrAnnotatedTags(bool annotatedTagOnly)
        {
            if (annotatedTagOnly)
                return t => t.IsAnnotated;
            return t => true;
        }
        public IEnumerable<Commit> GetAllUnreleasedMergeCommits(IRepository repo, string releaseBranchRef, bool annotatedTagOnly)
        {
            var releasedCommitsHash = new Dictionary<string, Commit>();
            var branchReference = repo.Branches[releaseBranchRef];
            var tagCommits = repo.Tags
                .Where(LightOrAnnotatedTags(annotatedTagOnly))
                .Where(t => ParseSemVer.Match(t.FriendlyName).Success)
                .Select(tag => tag.Target as Commit).Where(x => x != null).ToList();
            var branchAncestors = repo.Commits
                .QueryBy(new CommitFilter { IncludeReachableFrom = branchReference })
                .Where(commit => commit.Parents.Count() > 1);
            if (!tagCommits.Any())
                return branchAncestors;
            // for each tagged commit walk down all its parents and collect a dictionary of unique commits
            foreach (var tagCommit in tagCommits)
            {
                // we only care about tags descending from the branch we are interested in
                if (repo.Commits.QueryBy(new CommitFilter { IncludeReachableFrom = branchReference }).Any(c => c.Sha == tagCommit.Sha))
                {
                    var releasedCommits = repo.Commits
                        .QueryBy(new CommitFilter { IncludeReachableFrom = tagCommit.Id })
                        .Where(commit => commit.Parents.Count() > 1)
                        .ToDictionary(i => i.Sha, i => i);
                    releasedCommitsHash.Merge(releasedCommits);
                }
            }
            // remove released commits from the branch ancestor commits as they have been previously released
            return branchAncestors.Except(releasedCommitsHash.Values.AsEnumerable());
        }
    }
}
