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

        public IEnumerable<Commit> GetAllUnreleasedMergeCommits(IRepository repo, string releaseBranchRef, bool annotatedTagOnly)
        {
            var releasedCommitsHash = new Dictionary<string, Commit>();
            var branchReference = repo.Branches[releaseBranchRef];
            var tagCommits = repo.Tags
                .Where(x => !annotatedTagOnly || x.IsAnnotated)
                .Where(t => ParseSemVer.Match(t.FriendlyName).Success)
                .Select(tag => tag.PeeledTarget.Peel<Commit>()).Where(x => x != null)
                .OrderByDescending(x => x.Author.When)
                .ToList()
                .AsParallel().Where(x => BranchContainsTag(repo, x, branchReference))
                .ToList();
            var branchAncestors = repo.Commits
                .QueryBy(new CommitFilter { IncludeReachableFrom = branchReference })
                .Where(commit => commit.Parents.Count() > 1);
            if (!tagCommits.Any())
                return branchAncestors;

            var checkedTags = new List<Commit>();

            // for each tagged commit walk down all its parents and collect a dictionary of unique commits
            foreach (var tagCommit in tagCommits)
            {
                var containedInOtherTag = TagContainedInOtherCheckedTags(repo, checkedTags, tagCommit);

                if (containedInOtherTag)
                {
                    // insert to the beginning so this tag will be checked first for next tag
                    // because this tag is probably the closest tag that contains the next one.
                    checkedTags.Insert(0, tagCommit);
                    continue;
                }

                var releasedCommits = repo.Commits
                    .QueryBy(new CommitFilter {IncludeReachableFrom = tagCommit.Id})
                    .Where(commit => commit.Parents.Count() > 1)
                    .ToDictionary(i => i.Sha, i => i);
                releasedCommitsHash.Merge(releasedCommits);
                checkedTags.Insert(0, tagCommit);
            } 

            // remove released commits from the branch ancestor commits as they have been previously released
            return branchAncestors.Except(releasedCommitsHash.Values.AsEnumerable());
        }

        private static bool TagContainedInOtherCheckedTags(IRepository repo, IEnumerable<Commit> checkedTags, Commit tagCommit)
        {
            var containedInOtherTag = false;
            foreach (var checkedTag in checkedTags)
            {
                containedInOtherTag = repo.ObjectDatabase.FindMergeBase(checkedTag, tagCommit)?.Sha == tagCommit.Sha;
                if (containedInOtherTag)
                {
                    break;
                }
            }

            return containedInOtherTag;
        }

        private static bool BranchContainsTag(IRepository repo, Commit tagCommit, Branch branchReference)
        {
            var mergeBase = repo.ObjectDatabase.FindMergeBase(tagCommit, branchReference.Tip);
            var branchContainsTag = mergeBase?.Sha == tagCommit.Sha;
            return branchContainsTag;
        }
    }
}
