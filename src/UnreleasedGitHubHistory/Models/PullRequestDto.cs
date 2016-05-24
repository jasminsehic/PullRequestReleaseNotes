using System;
using System.Collections.Generic;

namespace UnreleasedGitHubHistory.Models
{
    public class PullRequestDto
    {
        public int Number { get; set; }
        public string Title { get; set; }
        public List<string> Labels { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? MergedAt { get; set; }
    }
}
