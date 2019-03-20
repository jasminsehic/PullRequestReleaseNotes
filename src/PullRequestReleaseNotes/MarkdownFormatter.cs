using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PullRequestReleaseNotes.Models;

namespace PullRequestReleaseNotes
{
    public class MarkdownFormatter
    {
        private readonly ProgramArgs _programArgs;

        public MarkdownFormatter(ProgramArgs programArgs)
        {
            _programArgs = programArgs;
        }

        public string Format(SemanticReleaseNotes releaseNotes)
        {
            if (!_programArgs.ReleaseNoteSectioned.Value)
                return FormatNotes(releaseNotes.Sections.SelectMany(n => n.Items));

            var markdown = new StringBuilder();

            if (!_programArgs.ReleaseNoteCategorised.Value)
            { 
                foreach (var section in releaseNotes.Sections)
                {
                    markdown.AppendLine($"## {section.Name}");
                    markdown.Append(FormatNotes(section.Items));
                }
                return markdown.ToString();
            }

            foreach (var section in releaseNotes.Sections)
            {
                markdown.AppendLine().AppendLine($"## {section.Name}");
                var categories = section.Items.SelectMany(x => x.Categories).Distinct().OrderBy(x => x).ToList();
                foreach (var category in categories)
                {
                    markdown.AppendLine().AppendLine($"### {category}");
                    markdown.Append(FormatNotesUnderCategory(section.Items, category));
                }
                var itemsWithoutCategories = section.Items.Where(i => !i.Categories.Any()).ToList();
                if (!itemsWithoutCategories.Any())
                    continue;
                markdown.AppendLine().AppendLine($@"### {_programArgs.ReleaseNoteUncategorisedDescription}");
                markdown.Append(FormatNotes(itemsWithoutCategories));
            }
            return markdown.ToString();
        }

        public static string EscapeMarkdown(string markdown)
        {
            const string specialMarkdownCharacters = @"\`*_{}[]()#>+-.!";
            var escapedMarkdown = new StringBuilder();
            foreach (var character in markdown)
            {
                if (specialMarkdownCharacters.Contains(character))
                    escapedMarkdown.Append($@"\{character}");
                else
                    escapedMarkdown.Append(character);
            }
            return escapedMarkdown.ToString();
        }

        private string FormatNotes(IEnumerable<SemanticReleaseItem> releaseNotes)
        {
            var markdown = new StringBuilder();
            var items = OrderReleaseItems(releaseNotes);
            foreach (var item in items.Select(i => i.Summary))
                markdown.AppendLine(FormatReleaseItem(item));
            return markdown.ToString();
        }

        private string FormatNotesUnderCategory(IEnumerable<SemanticReleaseItem> releaseNotes, string category)
        {
            var markdown = new StringBuilder();
            var items = OrderReleaseItems(releaseNotes);
            foreach (var item in items.Where(x => x.Categories.Contains(category, StringComparer.InvariantCultureIgnoreCase)).Select(i => i.Summary))
                markdown.AppendLine(FormatReleaseItem(item));
            return markdown.ToString();
        }

        private IEnumerable<SemanticReleaseItem> OrderReleaseItems(IEnumerable<SemanticReleaseItem> releaseItems)
        {
            var orderWhenKey = OrderWhenKey();
            if (_programArgs.ReleaseNoteOrderAscending.Value)
                return releaseItems.OrderByDescending(orderWhenKey).ToList();
            return releaseItems.OrderBy(orderWhenKey).ToList();
        }

        private Func<SemanticReleaseItem, DateTimeOffset?> OrderWhenKey()
        {
            if (_programArgs.ReleaseNoteOrderWhen.CaseInsensitiveContains("created"))
                return r => r.Summary.CreatedAt;
            return r => r.Summary.MergedAt;
        }

        private string FormatReleaseItem(SemanticReleaseItemSummary item)
        {
            var pullRequestTitle = FormatReleaseNoteTitle(item);
            var pullRequestUrl = $@"[{_programArgs.PullRequestProvider.PrefixedPullRequest(item.Number)}]({_programArgs.PullRequestProvider.PullRequestUrl(item.Number)})";
            var pullRequestNumber = item.Number;
            var pullRequestCreatedAt = item.CreatedAt.ToString(_programArgs.ReleaseNoteDateFormat);
            var pullRequestMergedAt = item.MergedAt?.ToString(_programArgs.ReleaseNoteDateFormat);
            var pullRequestAuthor = item.Author;
            var pullRequestAuthorUrl = $@"[{item.Author}]({item.AuthorUrl})";
            var pullRequestDocumentUrl = string.Empty;
            if (!string.IsNullOrWhiteSpace(item.DocumentUrl))
				pullRequestDocumentUrl = $@"[Docs]({item.DocumentUrl})";
            return string.Format($@"- {_programArgs.ReleaseNoteFormat}", pullRequestTitle, pullRequestUrl, pullRequestNumber, pullRequestCreatedAt, pullRequestMergedAt, pullRequestAuthor, pullRequestAuthorUrl, pullRequestDocumentUrl);
        }

        private static string FormatReleaseNoteTitle(SemanticReleaseItemSummary item)
        {
            if (item.Highlight)
                return $"`{item.Title}`";
            var escapedTitle = EscapeMarkdown(item.Title);
            return EmphasiseSquareBraces(escapedTitle);
        }

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