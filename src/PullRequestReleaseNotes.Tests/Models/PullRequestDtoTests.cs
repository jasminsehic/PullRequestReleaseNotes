using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PullRequestReleaseNotes.Models;
using Shouldly;

namespace PullRequestReleaseNotes.Tests.Models
{
    [TestFixture]
    public class PullRequestDtoTests
    {
        [TestCase("a,b,c,h", "h", false)]
        [TestCase("a,b,c", "h", true)]
        [TestCase("a,b,c,h", "h,a", false)]
        public void Highlight(string labels, string highlightWhenMissing, bool highlight)
        {
            var dto = new PullRequestDto
            {
                Labels = Split(labels)
            };

            dto.Highlighted(Split(highlightWhenMissing)).ShouldBe(highlight);
        }

        private static List<string> Split(string str)
        {
            return str.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
        }
    }
}
