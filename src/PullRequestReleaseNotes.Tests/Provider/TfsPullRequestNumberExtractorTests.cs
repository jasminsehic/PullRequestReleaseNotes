using NUnit.Framework;
using PullRequestReleaseNotes.Providers;
using Shouldly;

namespace PullRequestReleaseNotes.Tests.Provider
{
    [TestFixture]
    public class TfsPullRequestNumberExtractorTests
    {
        [TestCase("Merge pull request 123: fix a bug", 123)]
        [TestCase("Merged PR 123: fix a bug", 123)]
        [TestCase("t 123: number 123 was was extracted by the old code", null)]
        public void CanExtractPullRequestNumber(string commit, int? expected)
        {
            TfsPullRequestNumberExtractor.Extract(commit).ShouldBe(expected);
        }
    }
}
