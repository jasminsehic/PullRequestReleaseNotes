using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnreleasedGitHubHistory.Models;

namespace UnreleasedGitHubHistory
{
    public static class ReleaseNoteFormatter
    {
        public static string MarkdownNotes(List<PullRequestDto> pullRequests, ProgramArgs programArgs)
        {
            var markdown = new StringBuilder();

            if (!programArgs.ReleaseNoteSectioned.Value)
                return markdown.AppendLine(FormatReleaseNotes(pullRequests, null, null, programArgs)).ToString();

            var userSuppliedSectionDescriptions = new Dictionary<string, string>();
            if (programArgs.ReleaseNoteSections != null && programArgs.ReleaseNoteSections.Any())
                userSuppliedSectionDescriptions = programArgs.ReleaseNoteSections.ToDictionary(label => label.Split('=').First(), label => label.Split('=').Last(), StringComparer.InvariantCultureIgnoreCase);
            var sections = pullRequests.SelectMany(c => c.Labels).Distinct().Where(c => userSuppliedSectionDescriptions.ContainsKey(c)).OrderBy(c => c).ToList();
            var pullRequestsWithoutSection = pullRequests.Where(pr => !userSuppliedSectionDescriptions.Keys.Intersect(pr.Labels, StringComparer.InvariantCultureIgnoreCase).Any()).ToList();
            if (pullRequestsWithoutSection.Any())
            {
                foreach (var pullRequestWithoutSection in pullRequestsWithoutSection)
                    pullRequestWithoutSection.Labels.Add(programArgs.ReleaseNoteSectionlessDescription);
                sections.Add(programArgs.ReleaseNoteSectionlessDescription);
                userSuppliedSectionDescriptions.Add(programArgs.ReleaseNoteSectionlessDescription, programArgs.ReleaseNoteSectionlessDescription);
            }

            var categoryDescriptions = BuildCategoryDescriptions(pullRequests, programArgs);
            foreach (var sectionDescription in sections.Select(section => userSuppliedSectionDescriptions[section]).OrderBy(d => d).ToList())
            {
                var section = userSuppliedSectionDescriptions.FirstOrDefault(x => x.Value == sectionDescription).Key;
                var sectionPullRequests = pullRequests.Where(pr => pr.Labels.Contains(section, StringComparer.InvariantCultureIgnoreCase)).ToList();
                markdown.AppendLine(FormatReleaseNotes(sectionPullRequests, sectionDescription, categoryDescriptions, programArgs));
            }

            return markdown.ToString();
        }

        public static string EscapeMarkdown(string markdown)
        {
            const string specialMarkdownCharaters = @"\`*_{}[]()#>+-.!";
            var escapedMarkdown = new StringBuilder();
            foreach (var character in markdown)
            {
                if (specialMarkdownCharaters.Contains(character))
                    escapedMarkdown.Append($@"\{character}");
                else
                    escapedMarkdown.Append(character);
            }
            return escapedMarkdown.ToString();
        }

        private static Dictionary<string, string> BuildCategoryDescriptions(List<PullRequestDto> pullRequests, ProgramArgs programArgs)
        {
            var categoryDescriptions = new Dictionary<string, string>();
            var userSuppliedCategoryDescriptions = new Dictionary<string, string>();
            var categories = pullRequests.SelectMany(c => c.Labels).Distinct().Where(c => c.StartsWith(programArgs.ReleaseNoteCategoryPrefix));
            if (programArgs.ReleaseNoteCategories != null && programArgs.ReleaseNoteCategories.Any())
              userSuppliedCategoryDescriptions = programArgs.ReleaseNoteCategories.ToDictionary(label => label.Split('=').First(), label => label.Split('=').Last(), StringComparer.InvariantCultureIgnoreCase);

            foreach (var category in categories.Select(c => c.Replace(programArgs.ReleaseNoteCategoryPrefix, string.Empty)))
            {
                if (userSuppliedCategoryDescriptions.ContainsKey(category))
                    categoryDescriptions.Add(category, userSuppliedCategoryDescriptions[category]);
                else
                    // if no label description was supplied then default to label itself
                    categoryDescriptions.Add(category, category);
            }
            return categoryDescriptions;
        }

        private static string FormatReleaseNotes(IReadOnlyCollection<PullRequestDto> pullRequests, string sectionDescription, Dictionary<string, string> categories, ProgramArgs programArgs)
        {
            var markdown = new StringBuilder();

            if (string.IsNullOrWhiteSpace(sectionDescription) && categories == null)
            {
                AppendMarkdownNotes(pullRequests, markdown, programArgs);
                return markdown.ToString();
            }

            if (pullRequests.Any())
                markdown.AppendLine($"## {sectionDescription}");
            else
                return string.Empty;

            if (!programArgs.ReleaseNoteCategorised.Value)
            {
                AppendMarkdownNotes(pullRequests, markdown, programArgs);
                return markdown.ToString();
            }

            foreach (var category in categories.OrderBy(a => a.Value))
            {
                var pullRequestsWithCategories = pullRequests.Where(c => c.Labels.Any(label => categories.ContainsKey(label.Replace(programArgs.ReleaseNoteCategoryPrefix, string.Empty)))).ToList();
                if (!pullRequestsWithCategories.Any())
                    continue;
                var pullRequestsWithLabelsThatContainCategory = pullRequestsWithCategories.Where(pr => pr.Labels.Contains($"#{category.Key}")).ToList();
                if (pullRequestsWithLabelsThatContainCategory.Any())
                    markdown.AppendLine().AppendLine($@"### {category.Value}");
                AppendMarkdownNotes(pullRequestsWithLabelsThatContainCategory, markdown, programArgs);
            }
            var pullRequestsWithoutCategories = pullRequests.Where(c => !c.Labels.Any(label => categories.ContainsKey(label.Replace(programArgs.ReleaseNoteCategoryPrefix, string.Empty)))).ToList();
            if (!pullRequestsWithoutCategories.Any())
                return markdown.ToString();

            markdown.AppendLine().AppendLine($@"### {programArgs.ReleaseNoteUncategorisedDescription}");
            AppendMarkdownNotes(pullRequestsWithoutCategories, markdown, programArgs);
            return markdown.ToString();
        }

        private static void AppendMarkdownNotes(IEnumerable<PullRequestDto> pullRequests, StringBuilder markdown, ProgramArgs programArgs)
        {
            foreach (var pullRequest in pullRequests)
            {
                var pullRequestTitle = FormatTitle(pullRequest.Title, pullRequest, programArgs);
                var pullRequestUrl = $@"[{programArgs.PullRequestProvider.PrefixedPullRequest(pullRequest.Number)}]({programArgs.PullRequestProvider.PullRequestUrl(pullRequest.Number)})";
                var pullRequestNumber = pullRequest.Number;
                var pullRequestCreatedAt = pullRequest.CreatedAt.ToString(programArgs.ReleaseNoteDateFormat);
                var pullRequestMergedAt = pullRequest.MergedAt?.ToString(programArgs.ReleaseNoteDateFormat);
                var pullRequestAuthor = pullRequest.Author;
                var pullRequestAuthorUrl = $@"[{pullRequest.Author}]({pullRequest.AuthorUrl})";
                markdown.AppendLine(string.Format($@"- {programArgs.ReleaseNoteFormat}", pullRequestTitle, pullRequestUrl, pullRequestNumber, pullRequestCreatedAt, pullRequestMergedAt, pullRequestAuthor, pullRequestAuthorUrl));
            }
        }

        private static string FormatTitle(string title, PullRequestDto pullRequest, ProgramArgs programArgs)
        {
            if (!programArgs.ReleaseNoteHighlightlLabels.All(string.IsNullOrWhiteSpace))
            {
                if (pullRequest.Labels
                   .Intersect(programArgs.ReleaseNoteHighlightlLabels, StringComparer.InvariantCultureIgnoreCase)
                   .Count() != programArgs.ReleaseNoteHighlightlLabels.Count)
                    return $"`{title}`";
            }
            var escapedTitle = EscapeMarkdown(title);
            return EmphasiseSquareBraces(escapedTitle);
        }

        /// <summary>
        /// Add markup to any outermost square braces.
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        private static string EmphasiseSquareBraces(string title)
        {
            // Test / explanation here; https://regex101.com/r/kU1pJ7/4
            var braceFinder = new Regex(@"\\\[\\\#(?:[^\[\]]+)*\]");
            var matches = braceFinder.Matches(title);
            if (matches.Count <= 0)
                return title;
            return matches.Cast<Match>()
                .SelectMany(match => match.Groups.Cast<Group>().ToList())
                .Aggregate(title, (current, @group) => current.Replace(@group.Value, $"**{@group.Value}**"));
        }
    }
}