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
                    var lastCommit = GetLastTaggedCommit(localGitRepository, programArgs.ReleaseBranchRef);
                    if (lastCommit == null)
                    {
                        Console.WriteLine($"Couldn't find the last commit to build history from");
                        return null;
                    }
                    if (programArgs.VerboseOutput)
                        Console.WriteLine($"Building history for {programArgs.ReleaseBranchRef} down to commit {lastCommit.Sha}");
                    var unreleasedCommits = GetUnreleasedCommits(programArgs, localGitRepository, lastCommit);
                    foreach (var mergeCommit in unreleasedCommits.Where(commit => commit.Parents.Count() > 1))
                    {
                        PullRequest pullRequest;
                        var pullRequestDto = RetrieveGitHubPullRequest(gitHubClient, mergeCommit.Message, mergeCommit.Sha, programArgs, out pullRequest);
                        if (pullRequestDto == null) continue;
                        if (pullRequestDto.Labels.Contains(programArgs.FollowLabel, StringComparer.InvariantCultureIgnoreCase))
                            FollowChildPullRequests(programArgs, gitHubClient, pullRequest, releaseHistory);
                        else
                            releaseHistory.Add(pullRequestDto);
                    }
                    return OrderReleaseNotes(releaseHistory.Distinct(new PullRequestDtoEqualityComparer()).ToList(), programArgs);
                }
            }
            catch (Exception ex) when (ex is ArgumentNullException || ex is ArgumentException || ex is RepositoryNotFoundException)
            {
                Console.WriteLine("GitRepositoryPath was not supplied or is invalid.");
                return null;
            }
        }

        private static PullRequestDto RetrieveGitHubPullRequest(GitHubClient gitHubClient, string commitMessage, string commitSha, ProgramArgs programArgs, out PullRequest pullRequest)
        {
            pullRequest = null;
            var pullRequestNumber = ExtractPullRequestNumber(commitMessage);
            if (pullRequestNumber == null) return null;
            pullRequest = gitHubClient.PullRequest.Get(programArgs.GitHubOwner, programArgs.GitHubRepository, (int)pullRequestNumber).Result;
            if (pullRequest == null) return null;
            if (programArgs.VerboseOutput)
                Console.WriteLine($"Found #{pullRequest.Number}: {pullRequest.Title}: {commitSha}");
            return GetPullRequestWithLabels(programArgs, pullRequest, gitHubClient);
        }

        private static void FollowChildPullRequests(ProgramArgs programArgs, GitHubClient gitHubClient, PullRequest parentPullRequest, List<PullRequestDto> releaseHistory)
        {
            var commits = gitHubClient.PullRequest.Commits(programArgs.GitHubOwner, programArgs.GitHubRepository, parentPullRequest.Number).Result;
            foreach (var commit in commits.Where(c => c.Parents.Count > 1))
            {
                var pullRequestDto = RetrieveGitHubPullRequest(gitHubClient, commit.Commit.Message, commit.Sha, programArgs, out parentPullRequest);
                if (pullRequestDto == null)
                    continue;
                if (pullRequestDto.Labels.Contains(programArgs.FollowLabel, StringComparer.InvariantCultureIgnoreCase))
                    FollowChildPullRequests(programArgs, gitHubClient, parentPullRequest, releaseHistory);
                else
                    releaseHistory.Add(pullRequestDto);
            }
        }

        private static List<PullRequestDto> OrderReleaseNotes(List<PullRequestDto> releaseHistory, ProgramArgs programArgs)
        {
            var orderWhenKey = OrderWhenKey(programArgs);
            if (programArgs.ReleaseNoteOrderDescending)
                return releaseHistory.OrderByDescending(orderWhenKey).ToList();
            return releaseHistory.OrderBy(orderWhenKey).ToList();
        }

        private static Func<PullRequestDto, DateTimeOffset?> OrderWhenKey(ProgramArgs programArgs)
        {
            if (programArgs.ReleaseNoteOrderWhen.CaseInsensitiveContains("created"))
                return r => r.CreatedAt;
            return r => r.MergedAt;
        }

        private static bool DiscoverGitHubSettings(ProgramArgs programArgs, IRepository localGitRepository)
        {
            if (!string.IsNullOrWhiteSpace(programArgs.GitHubOwner) && !string.IsNullOrWhiteSpace(programArgs.GitHubRepository))
                return true;

            Remote remote = null;
            if (localGitRepository.Network.Remotes.Any())
                remote = localGitRepository.Network.Remotes[programArgs.GitRemote] ?? localGitRepository.Network.Remotes.First(r => r.Url.CaseInsensitiveContains("github.com"));
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
                CreatedAt = pullRequest.CreatedAt,
                MergedAt = pullRequest.MergedAt,
                Author = pullRequest.User.Name,
                AuthorUrl = pullRequest.User.Url,
                Labels = new List<string>()
            };
            foreach (var label in issue.Labels)
            {
                // filter out any unwanted pull requests
                if (label.Name.CaseInsensitiveContains(programArgs.ExcludeLabel))
                {
                    if (programArgs.VerboseOutput)
                        Console.WriteLine($"   - Excluding Pull Request");
                    return null;
                }
                pullRequestDto.Labels.Add(label.Name);
                if (programArgs.VerboseOutput)
                    Console.WriteLine($"   - Label : {label.Name}");
            }
            return pullRequestDto;
        }

        private static int? ExtractPullRequestNumber(string commitMessage)
        {
            var pattern = new Regex(@"Merge pull request #(?<pullRequestNumber>\d+) from .*");
            var match = pattern.Match(commitMessage);
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
            var olderThan = branch?.Tip.Author.When;
            var commitFilter = new CommitFilter { FirstParentOnly = true };
            var queriableCommits = branch?.Commits as IQueryableCommitLog;
            var lastTaggedCommit = queriableCommits?.QueryBy(commitFilter).FirstOrDefault(c => c.Author.When <= olderThan && tags.Any(a => a.Target.Sha == c.Sha));
            return lastTaggedCommit ?? branch?.Commits.Last();
        }
    }
}