using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LibGit2Sharp;
using SharpBucket.V2;
using SharpBucket.V2.Pocos;
using PullRequestReleaseNotes.Models;
using Group = System.Text.RegularExpressions.Group;

namespace PullRequestReleaseNotes.Providers
{
    public class BitBucketPullRequestProvider : IPullRequestProvider
    {
        private readonly ProgramArgs _programArgs;
        private readonly SharpBucketV2 _bitBucketClient = new SharpBucketV2();
        private const string BitBucketUrl = "https://bitbucket.org";

        // Formerly known as Stash
        public BitBucketPullRequestProvider(ProgramArgs programArgs)
        {
            _programArgs = programArgs;
            DiscoverBitBucketServerCredentials();
            _bitBucketClient.OAuth2LeggedAuthentication(_programArgs.BitBucketApiKey, _programArgs.BitBucketApiSecret);
        }

        private void DiscoverBitBucketServerCredentials()
        {
            if (!string.IsNullOrWhiteSpace(_programArgs.BitBucketApiSecret))
                return;
            if (_programArgs.VerboseOutput)
                Console.WriteLine($"BitBucket consumer secret was not supplied. Trying PRRN_BITBUCKET_SECRET environment variable.");
            _programArgs.BitBucketApiSecret = Environment.GetEnvironmentVariable("PRRN_BITBUCKET_SECRET");
            if (!string.IsNullOrWhiteSpace(_programArgs.BitBucketApiSecret))
                return;
            Console.WriteLine($"BitBucket consumer secret was not supplied and could not be found.");
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
                Console.WriteLine($"Found #{pullRequest.id}: {pullRequest.title}");
            return GetPullRequestDto(pullRequest);
        }

        private PullRequest GetPullRequest(int pullRequestId)
        {
            var pullRequests = _bitBucketClient.RepositoriesEndPoint()
                .PullReqestsResource(_programArgs.BitBucketAccount, _programArgs.BitBucketRepository);
            return pullRequests.PullRequestResource(pullRequestId).GetPullRequest() as PullRequest;
        }

        private PullRequestDto GetPullRequestDto(PullRequest pullRequest)
        {
            var pullRequestDto = new PullRequestDto
            {
                Number = (int) pullRequest.id,
                Title = RemoveTags(pullRequest.title),
                CreatedAt = Convert.ToDateTime(pullRequest.created_on),
                MergedAt = Convert.ToDateTime(pullRequest.updated_on),
                Author = pullRequest.author.display_name,
                AuthorUrl = pullRequest.author.links.self.href,
                Url = PullRequestUrl((int)pullRequest.id),
                DocumentUrl = pullRequest.description.ExtractDocumentUrl(),
                Labels = new List<string>()
            };

            // extract labels from title and description following pattern [#Section] ... [##Category]
            var labelPattern = new Regex(@"\#(?:[^\[\]]+)*");
            var matches = labelPattern.Matches($"{pullRequest.title}{pullRequest.description}");
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
            return $@"{BitBucketUrl}/{_programArgs.BitBucketAccount}/{_programArgs.BitBucketRepository}/pull-requests/{pullRequestId}";
        }

        public string PrefixedPullRequest(int pullRequestId)
        {
            return $"#{pullRequestId}";
        }

        public bool DiscoverRemote()
        {
            var remoteDomain = new Uri(BitBucketUrl).DnsSafeHost;
            Remote remote = null;
            if (!string.IsNullOrWhiteSpace(_programArgs.BitBucketAccount) && !string.IsNullOrWhiteSpace(_programArgs.BitBucketRepository))
                return true;
            if (_programArgs.VerboseOutput)
                Console.WriteLine($"BitBucketAccount and BitBucketRepository were not supplied. Trying to discover it from remotes.");
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
                Console.WriteLine($"BitBucketAccount and BitBucketRepository were not supplied and could not be discovered");
                return false;
            }
            var remoteUrl = new Uri(remote.Url);
            _programArgs.BitBucketAccount = remoteUrl.Segments[1].Replace(@"/", string.Empty);
            _programArgs.BitBucketRepository = remoteUrl.Segments[2].Replace(@".git", string.Empty);
            return true;
        }

        private static int? ExtractPullRequestNumber(string commitMessage)
        {
            //Merged in dev1 (pull request #1)
            var pattern = new Regex(@"Merged in .* \(pull request #(?<pullRequestNumber>\d+)\).*");
            var match = pattern.Match(commitMessage);
            if (match.Groups.Count <= 0 || !match.Groups["pullRequestNumber"].Success)
                return null;
            return int.Parse(match.Groups["pullRequestNumber"].Value);
        }
    }
}
