using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnreleasedGitHubHistory.Models;
using LibGit2Sharp;
using Octokit;
using Octokit.Internal;
using Commit = LibGit2Sharp.Commit;
using Repository = LibGit2Sharp.Repository;

namespace UnreleasedGitHubHistory
{
    public static class UnreleasedGitHubHistoryBuilder
    {
        public static List<PullRequestDto> BuildReleaseHistory(ProgramArgs programArgs)
        {
            if (string.IsNullOrWhiteSpace(programArgs.GitHubToken))
            {
                if (programArgs.VerboseOutput)
                    Console.WriteLine($"GitHubToken was not supplied. Trying UNRELEASED_HISTORY_GITHUB_TOKEN environment variable.");
                programArgs.GitHubToken = Environment.GetEnvironmentVariable("UNRELEASED_HISTORY_GITHUB_TOKEN");
                if (string.IsNullOrWhiteSpace(programArgs.GitHubToken))
                {
                    if (programArgs.VerboseOutput)
                        Console.WriteLine($"GitHubToken was not found. Exiting.");
                    return null;
                }
            }

            var gitHubCredentials = new InMemoryCredentialStore(new Octokit.Credentials(programArgs.GitHubToken));
            var gitHubClient = new GitHubClient(new ProductHeaderValue("UnreleasedGitHubHistory"), gitHubCredentials);
            var releaseHistory = new List<PullRequestDto>();

            if (string.IsNullOrWhiteSpace(programArgs.GitRepositoryPath))
            {
                if (programArgs.VerboseOutput)
                    Console.WriteLine($"GitRepositoryPath was not supplied. Trying to discover the Git repository from the current directory.");
                programArgs.GitRepositoryPath = Directory.GetCurrentDirectory();
            }

            using (var localGitRepository = new Repository(Repository.Discover(programArgs.GitRepositoryPath)))
            {
                if (string.IsNullOrWhiteSpace(programArgs.ReleaseBranchRef))
                {
                    if (programArgs.VerboseOutput)
                        Console.WriteLine($"ReleaseBranchRef was not supplied. Using the current HEAD branch.");
                    programArgs.ReleaseBranchRef = localGitRepository.Head.CanonicalName;
                }
                var startingCommit = GetLastTaggedCommit(localGitRepository, programArgs.ReleaseBranchRef);
                if (programArgs.VerboseOutput)
                    Console.WriteLine($"Building history for {programArgs.ReleaseBranchRef} down to commit {startingCommit.Sha}");
                var unreleasedCommits = GetUnreleasedCommits(programArgs, localGitRepository, startingCommit);
                foreach (var mergeCommit in unreleasedCommits.Where(commit => commit.Parents.Count() > 1))
                {
                    var pullRequest = ExtractPullRequestFromCommit(programArgs, mergeCommit, gitHubClient);
                    if (pullRequest == null) continue;
                    if (programArgs.VerboseOutput)
                        Console.WriteLine($"Found #{pullRequest.Number}: {pullRequest.Title}: {mergeCommit.Sha}");
                    var pullRequestDto = GetPullRequestWithLabels(programArgs, pullRequest, gitHubClient);
                    releaseHistory.Add(pullRequestDto);
                }
                return releaseHistory;
            }
        }

        private static PullRequestDto GetPullRequestWithLabels(ProgramArgs programArgs, PullRequest pullRequest, GitHubClient gitHubClient)
        {
            // pull requests are actually GitHub issues so we have to use the issue API to get labels
            var issueNumber = int.Parse(pullRequest.IssueUrl.Segments.Last());
            var issue = gitHubClient.Issue.Get(programArgs.GitHubOwner, programArgs.GitHubRepository, issueNumber).Result;
            var pullRequestDto = new PullRequestDto
            {
                Number = pullRequest.Number,
                Title = pullRequest.Title,
                Labels = new List<string>()
            };
            foreach (var label in issue.Labels)
            {
                pullRequestDto.Labels.Add(label.Name);
                if (programArgs.VerboseOutput)
                    Console.WriteLine("   - Label : {0}", label.Name);
            }
            return pullRequestDto;
        }

        private static PullRequest ExtractPullRequestFromCommit(ProgramArgs programArgs, Commit commit, IGitHubClient gitHubClient)
        {
            var pattern = new Regex(@"Merge pull request #(?<pullRequestNumber>\d+) from .*");
            var match = pattern.Match(commit.Message);
            if (match.Groups.Count <= 0 || !match.Groups["pullRequestNumber"].Success)
                return null;
            var pullRequestNumber = int.Parse(match.Groups["pullRequestNumber"].Value);
            var pullRequest = gitHubClient.PullRequest.Get(programArgs.GitHubOwner, programArgs.GitHubRepository, pullRequestNumber).Result;
            return pullRequest;
        }

        private static ICommitLog GetUnreleasedCommits(ProgramArgs programArgs, IRepository localGitRepository, Commit startingCommit)
        {
            // Let's only consider the local branch refs that lead to this commit...
            var reachableRefs = localGitRepository.Refs.ReachableFrom(new[] {startingCommit})
                .Where(r => r.IsLocalBranch() && r.CanonicalName == programArgs.ReleaseBranchRef);
            //...and create a filter that will retrieve all the commits...
            var commitFilter = new CommitFilter
            {
                Since = reachableRefs,  // ...reachable from refs...
                Until = startingCommit, // ...until this commit is met
                FirstParentOnly = true  // ...only follow our direct lineage
            };
            var unreleasedCommits = localGitRepository.Commits.QueryBy(commitFilter);
            return unreleasedCommits;
        }

        private static Commit GetLastTaggedCommit(IRepository repository, string branchName)
        {
            var branch = repository.Branches.FirstOrDefault(b => b.CanonicalName == branchName);
            var tags = repository.Tags.ToArray();
            var olderThan = branch.Tip.Author.When;
            var commitFilter = new CommitFilter
            {
                FirstParentOnly = true
            };
            var queriableCommits = branch.Commits as IQueryableCommitLog;
            var lastTaggedCommit = queriableCommits.QueryBy(commitFilter).FirstOrDefault(c => c.Author.When <= olderThan && tags.Any(a => a.Target.Sha == c.Sha));
            if (lastTaggedCommit != null)
            {
                return lastTaggedCommit;
            }
            return branch.Commits.Last();
        }
    }
}