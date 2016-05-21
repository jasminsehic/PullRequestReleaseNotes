using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnreleasedGitHubHistory.Models;

namespace UnreleasedGitHubHistory
{
    public static class ReleaseNoteFormatter
    {
        public static string MarkdownNotes(List<PullRequestDto> releaseHistory, ProgramArgs programArgs)
        {
            var markdown = new StringBuilder();
            var enhancements = releaseHistory.Where(n => n.Enhancement()).ToList();
            var fixes = releaseHistory.Where(n => n.Bug()).ToList();
            var unclassified = releaseHistory.Where(n => n.Unclassified()).ToList();
            var releasedApplications = releaseHistory.SelectMany(c => c.Labels).Distinct().Where(c => c.StartsWith("#"));

            var releasedApplicationLabelDescriptionMap = new Dictionary<string, string>();
            var userSuppliedLabelDescriptionMap = new Dictionary<string, string>();
            if (programArgs.GitHubLabelDescriptionList != null)
              userSuppliedLabelDescriptionMap = programArgs.GitHubLabelDescriptionList.ToDictionary(label => label.Split('=').First(), label => label.Split('=').Last());

            foreach (var releasedApp in releasedApplications.Select(releasedApp => releasedApp.Replace("#", string.Empty)))
            {
                if (userSuppliedLabelDescriptionMap.ContainsKey(releasedApp))
                    releasedApplicationLabelDescriptionMap.Add(releasedApp, userSuppliedLabelDescriptionMap[releasedApp]);
                else
                    releasedApplicationLabelDescriptionMap.Add(releasedApp, releasedApp); // if no label description was supplied then default to label itself
            }

            markdown.AppendLine(FormatReleaseNotes(enhancements, releasedApplicationLabelDescriptionMap, "Enhancements", programArgs.GitHubOwner, programArgs.GitHubRepository));
            markdown.AppendLine(FormatReleaseNotes(fixes, releasedApplicationLabelDescriptionMap, "Fixes", programArgs.GitHubOwner, programArgs.GitHubRepository));
            markdown.AppendLine(FormatReleaseNotes(unclassified, releasedApplicationLabelDescriptionMap, "Unclassified", programArgs.GitHubOwner, programArgs.GitHubRepository));

            return markdown.ToString();
        }

        public static string EscapeMarkdown(string markdown)
        {
            const string specialMarkdownCharaters = @"\`*_{}[]()#>+-.!";
            var escapedMarkdown = string.Empty;
            foreach (var character in markdown.ToCharArray())
            {
                if (specialMarkdownCharaters.Contains(character))
                    escapedMarkdown += $@"\{character}";
                else
                    escapedMarkdown += character;
            }
            return escapedMarkdown;
        }

        private static string FormatReleaseNotes(List<PullRequestDto> pullRequests, Dictionary<string, string> applicationsLabelMap, string classificationHeading, string owner, string repoName)
        {
            var gitHubPullRequestUrl = $@"https://github.com/{owner}/{repoName}/pull/";
            var markdown = new StringBuilder();

            if (pullRequests.Any())
                markdown.AppendLine($"## {classificationHeading}");

            foreach (var app in applicationsLabelMap.OrderBy(a => a.Value))
            {
                var pullRequestsWithApplicationLabels = pullRequests.Where(n => !n.Applicationless()).ToList();
                if (pullRequestsWithApplicationLabels.Any())
                {
                    var pullRequestsWithLabelsThatContainApplication = pullRequestsWithApplicationLabels.Where(pr => pr.Labels.Contains($"#{app.Key}")).ToList();
                    if (pullRequestsWithLabelsThatContainApplication.Any())
                        markdown.AppendLine().AppendLine($@"### {app.Value}");
                    foreach (var pullRequest in pullRequestsWithLabelsThatContainApplication)
                        markdown.AppendLine($@"- {EmphasiseSquareBraces(EscapeMarkdown(pullRequest.Title))} [\#{pullRequest.Number}]({gitHubPullRequestUrl}{pullRequest.Number})");
                }
            }
            var pullRequestsWithoutComponents = pullRequests.Where(n => n.Applicationless()).ToList();
            if (pullRequestsWithoutComponents.Any())
            {
                markdown.AppendLine().AppendLine(@"### Undefined");
                foreach (var note in pullRequestsWithoutComponents)
                    markdown.AppendLine($@"- {EmphasiseSquareBraces(EscapeMarkdown(note.Title))} [\#{note.Number}]({gitHubPullRequestUrl}{note.Number})");
            }
            return markdown.ToString();
        }

        /// <summary>
        /// Add markup to the outermost square braces.
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        private static string EmphasiseSquareBraces(string title)
        {
            // Lazily match the first pair of square braces. Test / explanation here; https://regex101.com/r/kU1pJ7/2
            Regex braceFinder = new Regex(@"(?<prefix>.*)\[(?<contents>.*?)\](?<suffix>.*)");
            Match match = braceFinder.Match(title);
            if (match.Success)
                title = string.Format($"{match.Groups["prefix"]}**[{match.Groups["contents"]}]**{match.Groups["suffix"]}");

            return title;
        }
    }
}