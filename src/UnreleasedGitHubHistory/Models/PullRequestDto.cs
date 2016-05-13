using System.Collections.Generic;

namespace UnreleasedGitHubHistory.Models
{
    public class PullRequestDto
    {
        public int Number { get; set; }
        public string Title { get; set; }
        public List<string> Labels { get; set; }
        public bool Bug()
        {
            return Labels.Contains("Bug");
        }
        public bool Enhancement()
        {
            return Labels.Contains("Enhancement");
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
