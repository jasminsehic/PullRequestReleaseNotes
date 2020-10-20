using System.Text.RegularExpressions;

namespace PullRequestReleaseNotes.Providers
{
    public static class TfsPullRequestNumberExtractor
    {
        public static int? Extract(string commitMessage)
        {
            var pattern = new Regex(@"[Merge pull request|Merged PR] (?<pullRequestNumber>\d+).*");
            var match = pattern.Match(commitMessage);
            if (match.Groups.Count <= 0 || !match.Groups["pullRequestNumber"].Success)
                return null;
            return int.Parse(match.Groups["pullRequestNumber"].Value);
        }
    }
}
