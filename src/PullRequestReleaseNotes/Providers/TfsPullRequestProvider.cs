using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Infinity;
using Infinity.Models;
using LibGit2Sharp;
using PullRequestReleaseNotes.Models;

namespace PullRequestReleaseNotes.Providers
{
    public class TfsPullRequestProvider : IPullRequestProvider
    {
        private readonly ProgramArgs _programArgs;
        private readonly TfsClient _tfsClient;

        public TfsPullRequestProvider(ProgramArgs programArgs)
        {
            _programArgs = programArgs;
            DiscoverTfsCredentials();
            _tfsClient = new TfsClient(new TfsClientConfiguration
            {
                Url = new Uri($"{_programArgs.TfsApiUrl}/{_programArgs.TfsCollection}", UriKind.Absolute),
                Credentials = new NetworkCredential(_programArgs.TfsUsername, _programArgs.TfsToken),
            });
        }

        private void DiscoverTfsCredentials()
        {
            if (!string.IsNullOrWhiteSpace(_programArgs.TfsToken))
                return;
            if (_programArgs.VerboseOutput)
                Console.WriteLine($"TFS password was not supplied. Trying PRRN_TFS_TOKEN environment variable.");
            _programArgs.TfsToken = Environment.GetEnvironmentVariable("PRRN_TFS_TOKEN");
            if (!string.IsNullOrWhiteSpace(_programArgs.TfsToken))
                return;
            Console.WriteLine($"TfsToken was not supplied and could not be found.");
        }

        public PullRequestDto Get(string commitMessage)
        {
            var pullRequestId = ExtractPullRequestNumber(commitMessage);
            if (pullRequestId == null)
                return null;
            var pullRequest = GetPullRequest((int)pullRequestId);
            if (pullRequest == null)
                return null;
            if (_programArgs.VerboseOutput)
                Console.WriteLine($"Found #{pullRequest.Id}: {pullRequest.Title}");
            return GetPullRequestDto(pullRequest);
        }

        private PullRequest GetPullRequest(int pullRequestId)
        {
            var repository = _tfsClient.Git.GetRepositories().Result
                .First(r => string.Equals(r.Name, _programArgs.TfsRepository, StringComparison.CurrentCultureIgnoreCase));
            if (repository == null)
                return null;
            return _tfsClient.Git.GetPullRequest(repository.Id, pullRequestId).Result;
        }

        private PullRequestDto GetPullRequestDto(PullRequest pullRequest)
        {
            var pullRequestDto = new PullRequestDto
            {
                Number = pullRequest.Id,
                Title = RemoveTags(pullRequest.Title),
                CreatedAt = pullRequest.CreationDate,
                MergedAt = pullRequest.ClosedDate,
                Author = pullRequest.CreatedBy.DisplayName,
                AuthorUrl = pullRequest.CreatedBy.Url.ToString(),
                Url = PullRequestUrl(pullRequest.Id),
                DocumentUrl = pullRequest.Description.ExtractDocumentUrl(),
                Labels = new List<string>()
            };
            // extract labels from title and description following pattern [#Section] ... [##Category]
            var labelPattern = new Regex(@"\#(?:[^\[\]]+)*");
            var matches = labelPattern.Matches($"{pullRequest.Title}{pullRequest.Description}");
            if (matches.Count <= 0)
                return null;
            foreach (Match match in matches)
            {
                foreach (var group in match.Groups.Cast<Group>().Distinct().ToList())
                {
                    var label = group.Value.Substring(1);
                    // filter out any unwanted pull requests
                    if (label.CaseInsensitiveContains(_programArgs.ExcludeLabel))
                    {
                        if (_programArgs.VerboseOutput)
                            Console.WriteLine($"   - Excluding Pull Request");
                        return null;
                    }
                    pullRequestDto.Labels.Add(label);
                    if (_programArgs.VerboseOutput)
                        Console.WriteLine($"   - Label : {label}");
                }
            }
            pullRequestDto.Labels = pullRequestDto.Labels.Distinct().ToList();
            return pullRequestDto;
        }

        private static string RemoveTags(string title)
        {
            var labelPattern = new Regex(@"\[\#(?:[^\[\]]+)*\]");
            var matches = labelPattern.Matches(title);
            if (matches.Count <= 0)
                return title;
            return matches.Cast<Match>()
                .SelectMany(match => match.Groups.Cast<Group>().ToList())
                .Aggregate(title, (current, @group) => current.Replace(@group.Value, string.Empty)).Trim();
        }

        public string PullRequestUrl(int pullRequestId)
        {
            return $@"{_programArgs.TfsApiUrl}/{_programArgs.TfsCollection}/_git/{_programArgs.TfsRepository}/pullrequest/{pullRequestId}";
        }

        public string PrefixedPullRequest(int pullRequestId)
        {
            return $"{pullRequestId}";
        }

        public bool DiscoverRemote()
        {
            var remoteDomain = new Uri(_programArgs.TfsApiUrl).DnsSafeHost;
            Remote remote = null;
            if (!string.IsNullOrWhiteSpace(_programArgs.TfsCollection) && !string.IsNullOrWhiteSpace(_programArgs.TfsRepository))
                return true;
            if (_programArgs.VerboseOutput)
                Console.WriteLine($"TfsCollection and TfsRepository were not supplied. Trying to discover it from remotes.");
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
                Console.WriteLine($"TfsCollection and TfsRepository were not supplied and could not be discovered");
                return false;
            }
            var remoteUrl = new Uri(remote.Url);
            _programArgs.TfsCollection = remoteUrl.Segments[2].Replace(@"/", string.Empty);
            _programArgs.TfsRepository = remoteUrl.Segments[4];
            return true;
        }

        private static int? ExtractPullRequestNumber(string commitMessage)
        {
            var pattern = new Regex(@"Merge pull request (?<pullRequestNumber>\d+).*");
            var match = pattern.Match(commitMessage);
            if (match.Groups.Count <= 0 || !match.Groups["pullRequestNumber"].Success)
                return null;
            return int.Parse(match.Groups["pullRequestNumber"].Value);
        }
    }
}
