using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace PullRequestReleaseNotes.Tests
{
    internal abstract class TestBase
    {
        protected RepoHelper RepoHelper;

        [SetUp]
        public virtual void SetUp()
        {
            var repoFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "PrReleaseNoteTestRepo", Guid.NewGuid().ToString());
            RepoHelper = new RepoHelper(repoFolder);
            RepoHelper.MakeACommit("init");
        }

        [TearDown]
        public virtual void TearDown()
        {
            RepoHelper.Dispose();
        }
    }
}
