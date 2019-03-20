using System;
using System.Collections.Generic;
using System.Linq;

namespace PullRequestReleaseNotes.Models
{
    public class PullRequestDto
    {
        public int Number { get; set; }
        public string Title { get; set; }
        public List<string> Labels { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? MergedAt { get; set; }
        public string Author { get; set; }
        public string AuthorUrl { get; set; }
        public string Url { get; set; }
        public string DocumentUrl { get; set; }

        public List<string> Categories(string categoryPrefix, Dictionary<string, string> categoryDescriptions)
        {
            return Labels.Where(l => l.StartsWith(categoryPrefix)).ToList()
                .Select(category => categoryDescriptions[category.Replace(categoryPrefix, string.Empty)]).ToList();
        }

        public bool Highlighted(List<string> highlightLabels)
        {
            if (highlightLabels == null || highlightLabels.All(string.IsNullOrWhiteSpace))
                return false;
            return Labels.Intersect(highlightLabels, StringComparer.InvariantCultureIgnoreCase).Count() != highlightLabels.Count;
        }
    }
}
