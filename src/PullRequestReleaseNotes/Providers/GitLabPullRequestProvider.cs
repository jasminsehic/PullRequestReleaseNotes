using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using LibGit2Sharp;
using Newtonsoft.Json;
using RestSharp;
using PullRequestReleaseNotes.Models;

namespace PullRequestReleaseNotes.Providers
{
    public class GitLabPullRequestProvider : IPullRequestProvider
    {
        private readonly ProgramArgs _programArgs;
        private readonly RestClient _restClient;

        public GitLabPullRequestProvider(ProgramArgs programArgs)
        {
            _programArgs = programArgs;
            DiscoverToken();
            _restClient = new RestClient($"{_programArgs.GitLabApiUrl}/api/v3");
        }

        private void DiscoverToken()
        {
            if (!string.IsNullOrWhiteSpace(_programArgs.GitLabToken))
                return;
            if (_programArgs.VerboseOutput)
                Console.WriteLine($"GitLabToken was not supplied. Trying PRRN_GITLAB_TOKEN environment variable.");
            _programArgs.GitHubToken = Environment.GetEnvironmentVariable("PRRN_GITLAB_TOKEN");
            if (!string.IsNullOrWhiteSpace(_programArgs.GitLabToken))
                return;
            Console.WriteLine($"GitLabToken was not supplied and could not be found.");
        }

        public PullRequestDto Get(string commitMessage)
        {
            var pullRequestId = ExtractPullRequestNumber(commitMessage);
            if (pullRequestId == null)
                return null;
            var mergeRequest = GetMergeRequest((int) pullRequestId);
            if (mergeRequest == null)
                return null;
            if (_programArgs.VerboseOutput)
                Console.WriteLine($"Found #{mergeRequest.Iid}: {mergeRequest.Title}");
            return GetPullRequestDto(mergeRequest);
        }

        private MergeRequest GetMergeRequest(int pullRequestId)
        {
            List<MergeRequest> mergeRequests;
            var request = PrepareGitLabRequest("projects/{project_id}/merge_requests", pullRequestId);
            var response = _restClient.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    mergeRequests = JsonConvert.DeserializeObject<List<MergeRequest>>(response.Content);
                }
                catch (JsonReaderException)
                {
                    Console.WriteLine($"Error finding GitLab merge request. Response content:\n{response.Content}");
                    throw;
                }
                if (response.StatusCode != HttpStatusCode.OK || mergeRequests == null || !mergeRequests.Any())
                    return null;
            } 
            else
                return null;
            return mergeRequests.First();
        }

        private PullRequestDto GetPullRequestDto(MergeRequest mergeRequest)
        {
            var pullRequestDto = new PullRequestDto
            {
                Number = mergeRequest.Iid,
                Title = mergeRequest.Title,
                CreatedAt = mergeRequest.CreatedAt,
                MergedAt = mergeRequest.UpdatedAt,
                Author = mergeRequest.Author.Name,
                AuthorUrl = mergeRequest.Author.WebUrl,
                Url = PullRequestUrl(mergeRequest.Iid),
                DocumentUrl = mergeRequest.Description.ExtractDocumentUrl(),
                Labels = new List<string>()
            };
            foreach (var label in mergeRequest.Labels)
            {
                // filter out any unwanted pull requests
                if (label.CaseInsensitiveContains(_programArgs.ExcludeLabel))
                {
                    if (_programArgs.VerboseOutput)
                        Console.WriteLine($"   - Excluding Merge Request");
                    return null;
                }
                pullRequestDto.Labels.Add(label);
                if (_programArgs.VerboseOutput)
                    Console.WriteLine($"   - Label : {label}");
            }
            return pullRequestDto;
        }

        public string PullRequestUrl(int pullRequestId)
        {
            return $@"{_programArgs.GitLabApiUrl}/{_programArgs.GitLabOwner}/{_programArgs.GitLabRepository}/merge_requests/{pullRequestId}";
        }

        public string PrefixedPullRequest(int pullRequestId)
        {
            return $@"\!{pullRequestId}";
        }

        private RestRequest PrepareGitLabRequest(string relativeUrl, int pullRequestId)
        {
            var request = new RestRequest(relativeUrl, Method.GET);
            request.AddUrlSegment("project_id", _programArgs.GitLabProjectId);
            request.AddQueryParameter("iid", $"{pullRequestId}");
            request.AddQueryParameter("private_token", $"{_programArgs.GitLabToken}");
            return request;
        }

        private static int? ExtractPullRequestNumber(string commitMessage)
        {
            var pattern = new Regex(@"See merge request !(?<pullRequestNumber>\d+).*");
            var match = pattern.Match(commitMessage);
            if (match.Groups.Count <= 0 || !match.Groups["pullRequestNumber"].Success)
                return null;
            return int.Parse(match.Groups["pullRequestNumber"].Value);
        }

        public bool DiscoverRemote()
        {
            var remoteDomain = new Uri(_programArgs.GitLabApiUrl).DnsSafeHost;
            Remote remote = null;
            if (!string.IsNullOrWhiteSpace(_programArgs.GitLabOwner) && !string.IsNullOrWhiteSpace(_programArgs.GitLabRepository))
                return true;
            if (_programArgs.VerboseOutput)
                Console.WriteLine($"GitLabOwner and GitLabRepository were not supplied. Trying to discover it from remotes.");
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
            _programArgs.GitLabOwner = remoteUrl.Segments[1].Replace(@"/", string.Empty);
            _programArgs.GitLabRepository = remoteUrl.Segments[2].Replace(@".git", string.Empty);
            return true;
        }
    }

    [DataContract]
    internal class MergeRequest
    {
        [DataMember(Name = "id")]
        public int Id;

        [DataMember(Name = "iid")]
        public int Iid;

        [DataMember(Name = "state")]
        public string State;

        [DataMember(Name = "title")]
        public string Title;

        [DataMember(Name = "assignee")]
        public User Assignee;

        [DataMember(Name = "author")]
        public User Author;

        [DataMember(Name = "created_at")]
        public DateTime CreatedAt;

        [DataMember(Name = "description")]
        public string Description;

        [DataMember(Name = "downvotes")]
        public int Downvotes;

        [DataMember(Name = "upvotes")]
        public int Upvotes;

        [DataMember(Name = "updated_at")]
        public DateTime UpdatedAt;

        [DataMember(Name = "target_branch")]
        public string TargetBranch;

        [DataMember(Name = "source_branch")]
        public string SourceBranch;

        [DataMember(Name = "project_id")]
        public int ProjectId;

        [DataMember(Name = "source_project_id")]
        public int SourceProjectId;

        [DataMember(Name = "target_project_id")]
        public int TargetProjectId;

        [DataMember(Name = "work_in_progress")]
        public bool? WorkInProgress;

        [DataMember(Name = "labels")]
        public Collection<string> Labels;
    }
    
    [DataContract]
    internal class User
    {
        [DataMember(Name = "id")]
        public int Id;

        [DataMember(Name = "username")]
        public string Username;

        [DataMember(Name = "email")]
        public string Email;

        [DataMember(Name = "name")]
        public string Name;

        [DataMember(Name = "skype")]
        public string Skype;

        [DataMember(Name = "linkedin")]
        public string Linkedin;

        [DataMember(Name = "twitter")]
        public string Twitter;

        [DataMember(Name = "provider")]
        public string Provider;

        [DataMember(Name = "state")]
        public string State;

        [DataMember(Name = "blocked")]
        public bool Blocked;

        [DataMember(Name = "created_at")]
        public DateTime CreatedAt;

        [DataMember(Name = "avatar_url")]
        public string AvatarUrl;

        [DataMember(Name = "bio")]
        public string Bio;

        [DataMember(Name = "color_scheme_id")]
        public int ColorSchemeId;

        [DataMember(Name = "theme_id")]
        public int ThemeId;

        [DataMember(Name = "extern_uid")]
        public string ExternUid;

        [DataMember(Name = "web_url")]
        public string WebUrl;

        [DataMember(Name = "is_admin")]
        public bool IsAdmin;

        [DataMember(Name = "can_create_group")]
        public bool CanCreateGroup;

        [DataMember(Name = "can_create_project")]
        public bool CanCreateProject;
    }

    [DataContract]
    internal class GitLabCommit
    {
        [DataMember(Name = "id")]
        public string Id;

        [DataMember(Name = "title")]
        public string Title;

        [DataMember(Name = "short_id")]
        public string ShortId;

        [DataMember(Name = "author_name")]
        public string AuthorName;

        [DataMember(Name = "author_email")]
        public string AuthorEmail;

        [DataMember(Name = "created_at")]
        public DateTime CreatedAt;
    }
}