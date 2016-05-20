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
            if (!DiscoverGitHubToken(programArgs))
                return null;
            var gitHubCredentials = new InMemoryCredentialStore(new Octokit.Credentials(programArgs.GitHubToken));
            var gitHubClient = new GitHubClient(new ProductHeaderValue("UnreleasedGitHubHistory"), gitHubCredentials);
            var releaseHistory = new List<PullRequestDto>();
            if (string.IsNullOrWhiteSpace(programArgs.GitRepositoryPath))
            {
                if (programArgs.VerboseOutput)
                    Console.WriteLine($"GitRepositoryPath was not supplied. Trying to discover the Git repository from the current directory.");
                programArgs.GitRepositoryPath = Directory.GetCurrentDirectory();
            }
            try
            {
                using (var localGitRepository = new Repository(Repository.Discover(programArgs.GitRepositoryPath)))
                {
                    if (string.IsNullOrWhiteSpace(programArgs.ReleaseBranchRef))
                    {
                        if (programArgs.VerboseOutput)
                            Console.WriteLine($"ReleaseBranchRef was not supplied. Using the current HEAD branch.");
                        programArgs.ReleaseBranchRef = localGitRepository.Head.CanonicalName;
                    }
                    if (!DiscoverGitHubSettings(programArgs, localGitRepository))
                        return null;
                    var startingCommit = GetLastTaggedCommit(localGitRepository, programArgs.ReleaseBranchRef);
                    if (programArgs.VerboseOutput)
                        Console.WriteLine($"Building history for {programArgs.ReleaseBranchRef} down to commit {startingCommit.Sha}");
                    var unreleasedCommits = GetUnreleasedCommits(programArgs, localGitRepository, startingCommit);
                    foreach (var mergeCommit in unreleasedCommits.Where(commit => commit.Parents.Count() > 1))
                    {
                        var pullRequestNumber = ExtractPullRequestNumber(programArgs, mergeCommit);
                        var pullRequest = new PullRequest();
                        if (pullRequestNumber != null)
                            pullRequest = gitHubClient.PullRequest.Get(programArgs.GitHubOwner, programArgs.GitHubRepository, (int)pullRequestNumber).Result;
                        if (pullRequest != null && pullRequest.Number > 0)
                        {
                            if (programArgs.VerboseOutput)
                                Console.WriteLine($"Found #{pullRequest.Number}: {pullRequest.Title}: {mergeCommit.Sha}");
                            var pullRequestDto = GetPullRequestWithLabels(programArgs, pullRequest, gitHubClient);
                            if (pullRequestDto != null)
                                releaseHistory.Add(pullRequestDto);
                        }
                    }
                    return releaseHistory;
                }
            }
            catch (Exception ex) when (ex is ArgumentNullException || ex is ArgumentException || ex is RepositoryNotFoundException)
            {
                Console.WriteLine("GitRepositoryPath was not supplied or is invalid.");
                return null;
            }
        }

        private static bool DiscoverGitHubSettings(ProgramArgs programArgs, IRepository localGitRepository)
        {
            if (!string.IsNullOrWhiteSpace(programArgs.GitHubOwner) && !string.IsNullOrWhiteSpace(programArgs.GitHubRepository))
                return true;

            Remote remote = null;
            if (localGitRepository.Network.Remotes.Any())
                remote = localGitRepository.Network.Remotes[programArgs.GitRemote] ?? localGitRepository.Network.Remotes.First(r => r.Url.IndexOf("github.com", StringComparison.InvariantCultureIgnoreCase) >= 0);
            if (remote == null)
            {
                Console.WriteLine($"GitHubOwner and GitHubRepository were not supplied and could not be discovered");
                return false;
            }
            var remoteUrl = new Uri(remote.Url);
            programArgs.GitHubOwner = remoteUrl.Segments[1].Replace(@"/", string.Empty);
            programArgs.GitHubRepository = remoteUrl.Segments[2].Replace(@".git", string.Empty);
            return true;
        }

        private static bool DiscoverGitHubToken(ProgramArgs programArgs)
        {
            if (!string.IsNullOrWhiteSpace(programArgs.GitHubToken))
                return true;
            if (programArgs.VerboseOutput)
                Console.WriteLine("GitHubToken was not supplied. Trying UNRELEASED_HISTORY_GITHUB_TOKEN environment variable.");
            programArgs.GitHubToken = Environment.GetEnvironmentVariable("UNRELEASED_HISTORY_GITHUB_TOKEN");
            if (!string.IsNullOrWhiteSpace(programArgs.GitHubToken))
                return true;
            Console.WriteLine($"GitHubToken was not supplied and could not be found.");
            return false;
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
                // filter out any unwanted pull requests
                if (label.Name.IndexOf(programArgs.ExcludeLabel, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    return null;
                pullRequestDto.Labels.Add(label.Name);
                if (programArgs.VerboseOutput)
                    Console.WriteLine($"   - Label : {label.Name}");
            }
            return pullRequestDto;
        }

        private static int? ExtractPullRequestNumber(ProgramArgs programArgs, Commit commit)
        {
            var pattern = new Regex(@"Merge pull request #(?<pullRequestNumber>\d+) from .*");
            var match = pattern.Match(commit.Message);
            if (match.Groups.Count <= 0 || !match.Groups["pullRequestNumber"].Success)
                return null;
            return int.Parse(match.Groups["pullRequestNumber"].Value);
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