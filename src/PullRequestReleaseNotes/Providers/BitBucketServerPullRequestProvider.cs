using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using LibGit2Sharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RestSharp;
using RestSharp.Authenticators;
using PullRequestReleaseNotes.Models;
using Group = System.Text.RegularExpressions.Group;

namespace PullRequestReleaseNotes.Providers
{
    public class BitBucketServerPullRequestProvider : IPullRequestProvider
    {
        private readonly ProgramArgs _programArgs;
        private readonly RestClient _restClient;

        // Formerly known as Stash
        public BitBucketServerPullRequestProvider(ProgramArgs programArgs)
        {
            _programArgs = programArgs;
            DiscoverBitBucketServerCredentials();
            _restClient = new RestClient($"{_programArgs.BitBucketServerUrl}/rest/api/1.0/projects/{_programArgs.BitBucketServerProject}/repos/{_programArgs.BitBucketServerRepository}");
            _restClient.Authenticator = new HttpBasicAuthenticator(programArgs.BitBucketServerUsername, programArgs.BitBucketServerPassword);
        }

        private void DiscoverBitBucketServerCredentials()
        {
            if (!string.IsNullOrWhiteSpace(_programArgs.BitBucketServerPassword))
                return;
            if (_programArgs.VerboseOutput)
                Console.WriteLine($"BitBucketServer password was not supplied. Trying PRRN_BITBUCKETSERVER_PASSWORD environment variable.");
            _programArgs.BitBucketServerPassword = Environment.GetEnvironmentVariable("PRRN_BITBUCKETSERVER_PASSWORD");
            if (!string.IsNullOrWhiteSpace(_programArgs.BitBucketServerPassword))
                return;
            Console.WriteLine($"BitBucketServer password was not supplied and could not be found.");
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

        private BitBucketServerPullRequest GetPullRequest(int pullRequestId)
        {
            BitBucketServerPullRequest pullRequest;
            var request = new RestRequest($"/pull-requests/{pullRequestId}", Method.GET);
            var response = _restClient.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    pullRequest = JsonConvert.DeserializeObject<BitBucketServerPullRequest>(response.Content);
                }
                catch (JsonReaderException)
                {
                    Console.WriteLine($"Error finding BitBucket Server pull request. Response content:\n{response.Content}");
                    throw;
                }
                if (response.StatusCode != HttpStatusCode.OK || pullRequest == null)
                    return null;
            }
            else
                return null;
            return pullRequest;
        }

        private PullRequestDto GetPullRequestDto(BitBucketServerPullRequest pullRequest)
        {
            var pullRequestDto = new PullRequestDto
            {
                Number = Convert.ToInt32(pullRequest.Id),
                Title = RemoveTags(pullRequest.Title),
                CreatedAt = Convert.ToInt64(pullRequest.CreatedDate).FromTimestamp(),
                MergedAt = Convert.ToInt64(pullRequest.UpdatedDate).FromTimestamp(),
                Author = pullRequest.Author.User.DisplayName,
                AuthorUrl = pullRequest.Author.User.Links.Self.First().Href.ToString(),
                Url = PullRequestUrl(Convert.ToInt32(pullRequest.Id)),
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
            return $@"{_programArgs.BitBucketServerUrl}/projects/{_programArgs.BitBucketServerProject}/repos/{_programArgs.BitBucketServerRepository}/pull-requests/{pullRequestId}";
        }

        public string PrefixedPullRequest(int pullRequestId)
        {
            return $"#{pullRequestId}";
        }

        public bool DiscoverRemote()
        {
            var remoteDomain = new Uri(_programArgs.BitBucketServerUrl).DnsSafeHost;
            Remote remote = null;
            if (!string.IsNullOrWhiteSpace(_programArgs.BitBucketServerProject) && !string.IsNullOrWhiteSpace(_programArgs.BitBucketServerRepository))
                return true;
            if (_programArgs.VerboseOutput)
                Console.WriteLine($"BitBucketServerProject and BitBucketServerRepository were not supplied. Trying to discover it from remotes.");
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
                Console.WriteLine($"BitBucketServerProject and BitBucketServerRepository were not supplied and could not be discovered");
                return false;
            }
            var remoteUrl = new Uri(remote.Url);
            _programArgs.BitBucketServerProject = remoteUrl.Segments[2].Replace(@"/", string.Empty);
            _programArgs.BitBucketServerRepository = remoteUrl.Segments[3].Replace(@".git", string.Empty);
            return true;
        }

        private static int? ExtractPullRequestNumber(string commitMessage)
        {
            var pattern = new Regex(@"Merge pull request #(?<pullRequestNumber>\d+).*");
            var match = pattern.Match(commitMessage);
            if (match.Groups.Count <= 0 || !match.Groups["pullRequestNumber"].Success)
                return null;
            return int.Parse(match.Groups["pullRequestNumber"].Value);
        }
    }

    public enum BitBucketServerPullRequestState
    {
        OPEN,
        DECLINED,
        MERGED,
        ALL
    }

    public class BitBucketServerPullRequest
    {
        public string Id { get; set; }

        public string Version { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("state")]
        [JsonConverter(typeof(StringEnumConverter))]
        public BitBucketServerPullRequestState State { get; set; }

        [JsonProperty("open")]
        public bool Open { get; set; }

        [JsonProperty("closed")]
        public bool Closed { get; set; }

        public string CreatedDate { get; set; }

        public string UpdatedDate { get; set; }

        [JsonProperty("fromRef")]
        public Ref FromRef { get; set; }

        [JsonProperty("toRef")]
        public Ref ToRef { get; set; }

        [JsonProperty("locked")]
        public bool Locked { get; set; }

        public AuthorWrapper Author { get; set; }

        [JsonProperty("reviewers")]
        public AuthorWrapper[] Reviewers { get; set; }

        public List<AuthorWrapper> Participants { get; set; }

        public Link Link { get; set; }

        public Links Links { get; set; }
    }

    public class AuthorWrapper
    {
        [JsonProperty("approved")]
        public bool Approved { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("user")]
        public Author User { get; set; }
    }

    public class Link
    {
        public string Url { get; set; }

        public string Rel { get; set; }
    }

    public class Links
    {
        public Self[] Self { get; set; }

        public Clone[] Clone { get; set; }
    }

    public class Ref
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        public string DisplayId { get; set; }

        public string LatestChangeset { get; set; }

        [JsonProperty("repository")]
        public BitBucketServerRepository Repository { get; set; }
    }

    public class BitBucketServerRepository
    {
        [JsonProperty("slug")]
        public string Slug { get; set; }

        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("scmId")]
        public string ScmId { get; set; }

        public string State { get; set; }

        public string StatusMessage { get; set; }

        [JsonProperty("forkable")]
        public bool Forkable { get; set; }

        [JsonProperty("public")]
        public bool Public { get; set; }

        public string CloneUrl { get; set; }

        public Link Link { get; set; }

        public Links Links { get; set; }

        [JsonProperty("project")]
        public Project Project { get; set; }
    }

    public class Self
    {
        public Uri Href { get; set; }
    }

    public class Project
    {
        public int Id { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        public bool Public { get; set; }

        public string Type { get; set; }

        public Link Link { get; set; }

        public Links Links { get; set; }
    }

    public class Author
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        public string EmailAddress { get; set; }

        public int Id { get; set; }

        public string DisplayName { get; set; }

        public bool Active { get; set; }

        public string Slug { get; set; }

        public string Type { get; set; }

        public Link Link { get; set; }

        public Links Links { get; set; }
    }

    public class Clone
    {
        public Uri Href { get; set; }

        public string Name { get; set; }
    }

    public class BitBucketServerCommit
    {
        public string Id { get; set; }
        public string DisplayId { get; set; }
        public Author Author { get; set; }
        [JsonConverter(typeof(TimestampConverter))]
        public DateTime AuthorTimestamp { get; set; }
        public string Message { get; set; }
        public Parent[] Parents { get; set; }
    }

    public class Parent
    {
        public string Id { get; set; }
        public string DisplayId { get; set; }
    }

    public class TimestampConverter : DateTimeConverterBase
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            long val;
            DateTime? dt = value as DateTime?;

            if (dt == null)
            {
                throw new InvalidOperationException("Expected DateTime got " + value.GetType().Name);
            }

            val = dt.Value.ToTimestamp();
            writer.WriteValue(val);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.Integer)
            {
                throw new Exception("Wrong Token Type");
            }

            long ticks = (long)reader.Value;
            return ticks.FromTimestamp();
        }
    }

    public class ResponseWrapper<T>
    {
        public int Size { get; set; }
        public int Limit { get; set; }
        public bool IsLastPage { get; set; }
        public IEnumerable<T> Values { get; set; }
        public int Start { get; set; }
        public int? NextPageStart { get; set; }
    }
}
