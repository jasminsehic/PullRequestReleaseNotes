namespace PullRequestReleaseNotes.Models
{
    public class PullRequestCommitDto
    {
        public bool Merge { get; set; }
        public string Message { get; set; }
        public string Sha { get; set; }
    }
}