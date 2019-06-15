using System;
using System.Collections.Generic;
using System.Linq;
using PullRequestReleaseNotes.Models;

namespace PullRequestReleaseNotes
{
    public class SemanticReleaseNotesBuilder
    {
        private readonly List<PullRequestDto> _pullRequests;
        private readonly string _summary;
        private readonly ProgramArgs _programArgs;
        private readonly Dictionary<string, string> _categoryDescriptions;

        public SemanticReleaseNotesBuilder(List<PullRequestDto> pullRequests, string summary, ProgramArgs programArgs)
        {
            _pullRequests = pullRequests;
            _summary = summary;
            _programArgs = programArgs;
            _categoryDescriptions = BuildCategoryDescriptions();
        }

        public SemanticReleaseNotes Build()
        {
            var releaseNotes = new SemanticReleaseNotes { Summary = _summary };
            var userSuppliedSectionDescriptions = new Dictionary<string, string>();
            if (_programArgs.ReleaseNoteSections != null && _programArgs.ReleaseNoteSections.Any())
                userSuppliedSectionDescriptions = _programArgs.ReleaseNoteSections.ToDictionary(label => label.Split('=').First(), label => label.Split('=').Last(), StringComparer.InvariantCultureIgnoreCase);
            var sections = _pullRequests.SelectMany(c => c.Labels).Distinct().Where(c => userSuppliedSectionDescriptions.ContainsKey(c)).OrderBy(c => c).ToList();
            var pullRequestsWithoutSection = _pullRequests.Where(pr => !userSuppliedSectionDescriptions.Keys.Intersect(pr.Labels, StringComparer.InvariantCultureIgnoreCase).Any()).ToList();
            if (pullRequestsWithoutSection.Any())
            {
                foreach (var pullRequestWithoutSection in pullRequestsWithoutSection)
                    pullRequestWithoutSection.Labels.Add(_programArgs.ReleaseNoteSectionlessDescription);
                sections.Add(_programArgs.ReleaseNoteSectionlessDescription);
                userSuppliedSectionDescriptions.Add(_programArgs.ReleaseNoteSectionlessDescription, _programArgs.ReleaseNoteSectionlessDescription);
            }
            foreach (var sectionDescription in sections.Select(section => userSuppliedSectionDescriptions[section]).OrderBy(d => d).ToList())
            {
                var section = userSuppliedSectionDescriptions.FirstOrDefault(x => x.Value == sectionDescription).Key;
                var sectionPullRequests = _pullRequests.Where(pr => pr.Labels.Contains(section, StringComparer.InvariantCultureIgnoreCase)).ToList();
                AppendSection(releaseNotes, sectionPullRequests, sectionDescription);
            }
            return releaseNotes;
        }

        private void AppendSection(SemanticReleaseNotes releaseNotes, IReadOnlyCollection<PullRequestDto> pullRequests, string sectionDescription)
        {
            if (!pullRequests.Any())
                return;
            var section = new SemanticReleaseSection { Name = sectionDescription };
            AppendItems(section, pullRequests);
            releaseNotes.Sections.Add(section);
        }

        private void AppendItems(SemanticReleaseSection section, IEnumerable<PullRequestDto> pullRequests)
        {
            foreach (var pullRequest in pullRequests)
            {
                var item = new SemanticReleaseItem
                {
                    Summary = new SemanticReleaseItemSummary()
                    {
                        Title = pullRequest.Title,
                        Author = pullRequest.Author,
                        AuthorUrl = pullRequest.AuthorUrl,
                        CreatedAt = pullRequest.CreatedAt,
                        MergedAt = pullRequest.MergedAt,
                        Number = pullRequest.Number,
                        Url = pullRequest.Url,
                        Highlight = pullRequest.Highlighted(_programArgs.ReleaseNoteHighlightlLabels),
                        DocumentUrl = pullRequest.DocumentUrl
                    },
                    Categories = pullRequest.Categories(_programArgs.ReleaseNoteCategoryPrefix, _categoryDescriptions)
                };
                section.Items.Add(item);
            }
        }

        private Dictionary<string, string> BuildCategoryDescriptions()
        {
            var categoryDescriptions = new Dictionary<string, string>();
            var userSuppliedCategoryDescriptions = new Dictionary<string, string>();
            var categories = _pullRequests.SelectMany(c => c.Labels).Distinct().Where(c => c.StartsWith(_programArgs.ReleaseNoteCategoryPrefix));
            if (_programArgs.ReleaseNoteCategories != null && _programArgs.ReleaseNoteCategories.Any())
                userSuppliedCategoryDescriptions = _programArgs.ReleaseNoteCategories.ToDictionary(label => label.Split('=').First(), label => label.Split('=').Last(), StringComparer.InvariantCultureIgnoreCase);

            foreach (var category in categories.Select(c => c.Replace(_programArgs.ReleaseNoteCategoryPrefix, string.Empty)))
            {
                if (userSuppliedCategoryDescriptions.ContainsKey(category))
                    categoryDescriptions.Add(category, userSuppliedCategoryDescriptions[category]);
                else
                    // if no label description was supplied then default to label itself
                    categoryDescriptions.Add(category, category);
            }
            return categoryDescriptions;
        }
    }

    public class SemanticReleaseNotes
    {
        public string Summary { get; set; }
        public List<SemanticReleaseSection> Sections { get; set; }
        public SemanticReleaseNotes()
        {
            Sections = new List<SemanticReleaseSection>();
        }
    }

    public class SemanticReleaseSection
    {
        public string Summary { get; set; }
        public string Name { get; set; }
        public List<SemanticReleaseItem> Items { get; set; }
        public SemanticReleaseSection()
        {
            Items = new List<SemanticReleaseItem>();
        }
    }

    public class SemanticReleaseItem
    {
        public SemanticReleaseItemSummary Summary { get; set; }
        public List<string> Categories { get; set; }
        public SemanticReleaseItem()
        {
            Categories = new List<string>();
        }
    }

    public class SemanticReleaseItemSummary
    {
        public string Title;
        public string Url;
        public int Number;
        public DateTimeOffset CreatedAt;
        public DateTimeOffset? MergedAt;
        public string Author;
        public string AuthorUrl;
        public bool Highlight;
        public string DocumentUrl;
    }
}