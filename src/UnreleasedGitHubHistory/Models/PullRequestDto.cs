using System;
using System.Collections.Generic;
using System.Linq;

namespace UnreleasedGitHubHistory.Models
{
    public class PullRequestDto
    {
        public int Number { get; set; }
        public string Title { get; set; }
        public List<string> Labels { get; set; }
        public bool Bug()
        {
            return Labels.Any(l => l.CaseInsensitiveContains("bug"));
        }
        public bool Enhancement()
        {
            return Labels.Any(l => l.CaseInsensitiveContains("enhancement"));
        }
        public bool Unclassified()
        {
            return !Bug() && !Enhancement();
        }
        public bool Applicationless()
        {
            return !Labels.Exists(l => l.StartsWith("#"));
        }
    }
}
