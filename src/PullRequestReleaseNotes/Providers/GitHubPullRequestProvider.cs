using System;
using Octokit;
using Octokit.Internal;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LibGit2Sharp;
using PullRequestReleaseNotes.Models;
using Credentials = Octokit.Credentials;

namespace PullRequestReleaseNotes.Providers
{
    public class GitHubPullRequestProvider : IPullRequestProvider
    {
        private readonly ProgramArgs _programArgs;
        private readonly GitHubClient _gitHubClient;

        public GitHubPullRequestProvider(ProgramArgs programArgs)
        {
            _programArgs = programArgs;
            DiscoverToken();
            var gitHubCredentials = new InMemoryCredentialStore(new Credentials(_programArgs.GitHubToken));
            _gitHubClient = new GitHubClient(new ProductHeaderValue("PullRequestReleaseNotes"), gitHubCredentials);
        }

        private void DiscoverToken()
        {
            if (!string.IsNullOrWhiteSpace(_programArgs.GitHubToken))
                return;
            if (_programArgs.VerboseOutput)
                Console.WriteLine($"GitHubToken was not supplied. Trying UNRELEASED_HISTORY_GITHUB_TOKEN environment variable.");
            _programArgs.GitHubToken = Environment.GetEnvironmentVariable("UNRELEASED_HISTORY_GITHUB_TOKEN");
            if (!string.IsNullOrWhiteSpace(_programArgs.GitHubToken))
                return;
            Console.WriteLine($"GitHubToken was not supplied and could not be found.");
        }

        public PullRequestDto Get(string commitMessage)
        {
            var pullRequestId = ExtractPullRequestNumber(commitMessage);
            if (pullRequestId == null)
                return null;
            var pullRequest = _gitHubClient.PullRequest.Get(_programArgs.GitHubOwner, _programArgs.GitHubRepository, (int) pullRequestId).Result;
            if (pullRequest == null)
                return null;
            if (_programArgs.VerboseOutput)
                Console.WriteLine($"Found #{pullRequest.Number}: {pullRequest.Title}");
            return GetPullRequestWithLabels(pullRequest);
        }

        private PullRequestDto GetPullRequestWithLabels(PullRequest pullRequest)
        {
            // pull requests are actually GitHub issues so we have to use the issue API to get labels
            var issueNumber = int.Parse(pullRequest.IssueUrl.Segments.Last());
            var issue = _gitHubClient.Issue.Get(_programArgs.GitHubOwner, _programArgs.GitHubRepository, issueNumber).Result;
            var pullRequestDto = new PullRequestDto
            {
                Number = pullRequest.Number,
                Title = pullRequest.Title,
                CreatedAt = pullRequest.CreatedAt,
                MergedAt = pullRequest.MergedAt,
                Author = pullRequest.User.Login,
                AuthorUrl = pullRequest.User.Url,
                Url = PullRequestUrl(pullRequest.Number),
                Labels = new List<string>()
            };
            foreach (var label in issue.Labels)
            {
                // filter out any unwanted pull requests
                if (label.Name.CaseInsensitiveContains(_programArgs.ExcludeLabel))
                {
                    if (_programArgs.VerboseOutput)
                        Console.WriteLine($"   - Excluding Pull Request");
                    return null;
                }
                pullRequestDto.Labels.Add(label.Name);
                if (_programArgs.VerboseOutput)
                    Console.WriteLine($"   - Label : {label.Name}");
            }
            return pullRequestDto;
        }

        public List<PullRequestCommitDto> Commits(int pullRequestId)
        {
            var commits = _gitHubClient.PullRequest.Commits(_programArgs.GitHubOwner, _programArgs.GitHubRepository, pullRequestId).Result;
            return commits.Select(commit => new PullRequestCommitDto
            {
                Merge = commit.Parents.Count > 1,
                Message = commit.Commit.Message,
                Sha = commit.Sha
            }).ToList();
        }

        public string PullRequestUrl(int pullRequestId)
        {
            return $@"{_programArgs.GitHubApiUrl}/{_programArgs.GitHubOwner}/{_programArgs.GitHubRepository}/pull/{pullRequestId}";
        }

        public string PrefixedPullRequest(int pullRequestId)
        {
            return $@"\#{pullRequestId}";
        }

        private static int? ExtractPullRequestNumber(string commitMessage)
        {
            var pattern = new Regex(@"Merge pull request #(?<pullRequestNumber>\d+) from .*");
            var match = pattern.Match(commitMessage);
            if (match.Groups.Count <= 0 || !match.Groups["pullRequestNumber"].Success)
                return null;
            return int.Parse(match.Groups["pullRequestNumber"].Value);
        }

        public bool DiscoverRemote()
        {
            Remote remote = null;
            var remoteDomain = new Uri(_programArgs.GitHubApiUrl).DnsSafeHost;
            if (!string.IsNullOrWhiteSpace(_programArgs.GitHubOwner) && !string.IsNullOrWhiteSpace(_programArgs.GitHubRepository))
                return true;
            if (_programArgs.VerboseOutput)
                Console.WriteLine($"GitHubOwner and GitHubRepository were not supplied. Trying to discover it from remotes.");
            if (!_programArgs.LocalGitRepository.Network.Remotes.Any(r => r.Url.CaseInsensitiveContains(remoteDomain)))
                return false;
            if (!string.IsNullOrWhiteSpace(_programArgs.GitRemote))
                remote = _programArgs.LocalGitRepository.Network.Remotes[_programArgs.GitRemote] ?? _programArgs.LocalGitRepository.Network.Remotes.First(r => r.Url.CaseInsensitiveContains(remoteDomain));
            // prefer origin and upstream
            if (remote == null)
                remote = _programArgs.LocalGitRepository.Network.Remotes
                    .Where(r => r.Name.CaseInsensitiveContains("origin") || r.Name.CaseInsensitiveContains("upstream"))
                    .OrderBy(r => r.Name).First();
            // fallback to any remaining one
            if (remote == null)
                remote = _programArgs.LocalGitRepository.Network.Remotes.First(r => r.Url.CaseInsensitiveContains(remoteDomain));
            if (remote == null)
            {
                Console.WriteLine($"GitHubOwner and GitHubRepository were not supplied and could not be discovered");
                return false;
            }
            var remoteUrl = new Uri(remote.Url);
            _programArgs.GitHubOwner = remoteUrl.Segments[1].Replace(@"/", string.Empty);
            _programArgs.GitHubRepository = remoteUrl.Segments[2].Replace(@".git", string.Empty);
            return true;
        }
    }
}