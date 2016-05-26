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
        public string Author { get; set; }
        public string AuthorUrl { get; set; }
    }

    public sealed class PullRequestDtoEqualityComparer : IEqualityComparer<PullRequestDto>
    {
        public bool Equals(PullRequestDto x, PullRequestDto y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Number == y.Number;
        }

        public int GetHashCode(PullRequestDto obj)
        {
            return obj.Number;
        }
    }
}
