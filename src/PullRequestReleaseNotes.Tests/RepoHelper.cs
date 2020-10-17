using System;
using System.IO;
using System.Threading;
using LibGit2Sharp;

namespace PullRequestReleaseNotes.Tests
{
    internal class RepoHelper : IDisposable
    {
        private readonly string _repoFolder;
        public readonly Repository Repo;
        private static Signature Author => new Signature("user", "email@email.com", DateTime.Now);


        public RepoHelper(string repoFolder)
        {
            _repoFolder = repoFolder;
            Repository.Init(_repoFolder);
            Repo = new Repository(_repoFolder);
        }


        public void MakeACommit(string commitMsg)
        {
            var file = Path.Combine(_repoFolder, Guid.NewGuid().ToString());
            File.WriteAllText(file, commitMsg);
            CommitFile(commitMsg, file);
        }

        private void CommitFile(string commitMsg, string file)
        {
            Commands.Stage(Repo, file);
            Repo.Commit(commitMsg, Author, Author);
        }

        public void CreateTag(string tag)
        {
            Repo.ApplyTag(tag);
            // we may sort by tag creation time, so sleep to make sure we don't create tags at exactly the same second
            Thread.Sleep(TimeSpan.FromSeconds(1)); 
        }

        public void CreateAnnotatedTag(string tag)
        {
            Repo.ApplyTag(tag, Repo.Head.Tip.Sha, Author, "message for tag");
            Thread.Sleep(TimeSpan.FromSeconds(1));
        }

        public string CreateAndCheckoutBranch(string branch)
        {
            Repo.CreateBranch(branch);
            Commands.Checkout(Repo, branch);
            return branch;
        }

        public void CheckoutBranch(string branch)
        {
            Commands.Checkout(Repo, branch);
        }
        public void Dispose()
        {
            Repo.Dispose();
            var directory = new DirectoryInfo(_repoFolder) { Attributes = FileAttributes.Normal };

            foreach (var info in directory.GetFileSystemInfos("*", SearchOption.AllDirectories))
            {
                info.Attributes = FileAttributes.Normal;
            }

            directory.Delete(true);
        }

        public Commit MergeBranch(string branch)
        {
            var result = Repo.Merge(Repo.Branches[branch], Author,
                new MergeOptions {FastForwardStrategy = FastForwardStrategy.NoFastForward});
            return result.Commit;
        }
    }
}
