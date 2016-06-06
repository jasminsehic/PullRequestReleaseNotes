using System.Net;
using RestSharp;
using PullRequestReleaseNotes.Models;

namespace PullRequestReleaseNotes.Publishers
{
    public class SlackPublisher
    {
        public static bool PublishPost(string pageTitle, string markdownNotes, ProgramArgs programArgs)
        {
            var restClient = new RestClient("https://slack.com/api");
            var request = new RestRequest("files.upload", Method.POST);
            request.AddQueryParameter("filename", pageTitle);
            request.AddQueryParameter("filetype", "post");
            request.AddQueryParameter("title", pageTitle);
            request.AddQueryParameter("channels", programArgs.SlackChannels);
            request.AddQueryParameter("token", programArgs.SlackToken);
            request.AddQueryParameter("content", markdownNotes);
            var response = restClient.Execute(request);
            return response.StatusCode == HttpStatusCode.OK && response.Content.Contains("\"ok\":true");
        }
    }
}