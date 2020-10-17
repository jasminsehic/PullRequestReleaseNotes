using System.Linq;
using NUnit.Framework;
using Shouldly;

namespace PullRequestReleaseNotes.Tests
{
    [TestFixture]
    internal class UnreleasedCommitsProviderTests : TestBase
    {
        private UnreleasedCommitsProvider _unreleasedCommitsProvider;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _unreleasedCommitsProvider = new UnreleasedCommitsProvider();
        }

        /// <summary>
        ///  init
        ///   |-----(create feature1 branch from here)
        ///   |   |
        ///   |   |-feature 1 (commit a feature)
        ///   |   /
        ///   |  /
        ///   | /
        ///   |/ (merge feature1 into release-1)
        /// </summary>
        [Test]
        public void SimplestCase()
        {
            var main = RepoHelper.CreateAndCheckoutBranch("main");
            var feature1 = CreateBranchAndCommit("feature-1");

            RepoHelper.CheckoutBranch(main);

            var theMergeCommit = RepoHelper.MergeBranch(feature1);

            var commits = _unreleasedCommitsProvider
                .GetAllUnreleasedMergeCommits(RepoHelper.Repo, main, annotatedTagOnly: false)
                .ToList();

            commits.Count.ShouldBe(1);
            commits[0].Sha.ShouldBe(theMergeCommit.Sha);
        }

        /// <summary>
        /// init
        ///   |-----(create feature1 branch from here)
        ///   |   |
        ///   |   |-feature 1 commit
        ///   |   /
        ///   |  /
        ///   | /
        ///   |/ (merge feature1)
        ///   |
        ///   |-----(create feature2 branch from here)
        ///   |   |
        ///   |   |-feature 2 commit
        ///   |  /
        ///   | /
        ///   |/ (merge feature2)
        /// </summary>
        [Test]
        public void AbleToGetMultipleCommitsMergedFromDifferentBranches()
        {
            var main = RepoHelper.CreateAndCheckoutBranch("main");

            var feature1Branch = CreateBranchAndCommit("feature1-branch");

            RepoHelper.CheckoutBranch(main);
            var feature2Branch = CreateBranchAndCommit("feature2-branch");

            RepoHelper.CheckoutBranch(main);
            var mergeFeature1 = RepoHelper.MergeBranch(feature1Branch);
            var mergeFeature2 = RepoHelper.MergeBranch(feature2Branch);

            var commits= _unreleasedCommitsProvider.GetAllUnreleasedMergeCommits(RepoHelper.Repo, main,
                annotatedTagOnly: false).ToList();
            commits.Count().ShouldBe(2);
            commits.Single(x => x.Sha == mergeFeature1.Sha).ShouldNotBeNull();
            commits.Single(x => x.Sha == mergeFeature2.Sha).ShouldNotBeNull();
        }

        private string CreateBranchAndCommit(string branch)
        {
            RepoHelper.CreateAndCheckoutBranch(branch);
            RepoHelper.MakeACommit($"{branch} implemented");
            return branch;
        }

        [Test]
        public void ExcludeCommitsNotInCurrentBranch()
        {
            var main = RepoHelper.CreateAndCheckoutBranch("main");

            var feature1Branch = CreateBranchAndCommit("feature1-branch");

            RepoHelper.CheckoutBranch(main);
            var feature2Branch = CreateBranchAndCommit("feature2-branch");

            RepoHelper.CheckoutBranch(main);
            var mergeFeature2 = RepoHelper.MergeBranch(feature2Branch);

            var commits = _unreleasedCommitsProvider.GetAllUnreleasedMergeCommits(RepoHelper.Repo, main,
                annotatedTagOnly: false).ToList();

            commits.Count().ShouldBe(1);
            commits.Single(x => x.Sha == mergeFeature2.Sha).ShouldNotBeNull();
        }

        /// <summary>
        /// init
        ///   |-----(create feature branch from here)
        ///   |   |
        ///   |   |-feature 1 commit
        ///   |   /
        ///   |  /
        ///   | /
        ///   |/ (merge feature1)
        ///   |
        ///   - tag 1.0.0 here
        ///   |-----(create feature2 branch from here)
        ///   |   |
        ///   |   |-feature 2 commit
        ///   |  /
        ///   | /
        ///   |/ (merge feature2)
        /// </summary>
        [Test]
        public void ExcludeCommitsAlreadyReleased()
        {
            var main = RepoHelper.CreateAndCheckoutBranch("main");

            var feature1Branch = CreateBranchAndCommit("feature1-branch");
            RepoHelper.CheckoutBranch(main);
            var mergeFeature1 = RepoHelper.MergeBranch(feature1Branch);
            RepoHelper.CreateTag("1.0.0");

            var feature2Branch = CreateBranchAndCommit("feature2-branch");
            RepoHelper.CheckoutBranch(main);
            var mergeFeature2 = RepoHelper.MergeBranch(feature2Branch);

            var commits = _unreleasedCommitsProvider.GetAllUnreleasedMergeCommits(RepoHelper.Repo, main,
                annotatedTagOnly: false).ToList();

            commits.Count().ShouldBe(1);
            commits.ShouldContain(x => x.Sha == mergeFeature2.Sha);
        }

        /// <summary>
        /// init
        ///   |-------------------------------------------------------
        ///   |   |                                                 |
        ///   |   |-feature 1 commit                                |
        ///   |   /                                                 |-feature2 commit  
        ///   |  /                                                  /
        ///   | /                                                  /
        ///   |/ (merge feature1)                                 /
        ///   |                                                  /
        ///   - tag 1.0.0 here for release1                     / 
        ///   |                                                /
        ///   |                                               /
        ///   |                                              /
        ///   |                                             /
        ///   |                                            /
        ///   |______________merge feature2 to main ______/
        ///   |
        ///   |
        /// </summary>
        [Test]
        public void ExcludeCommitsAlreadyReleasedWhenMultipleFeaturesInParallel()
        {
            var main = RepoHelper.CreateAndCheckoutBranch("main");

            var feature1Branch = CreateBranchAndCommit("feature1-branch");
            var feature2Branch = CreateBranchAndCommit("feature2-branch");
            
            RepoHelper.CheckoutBranch(main);
            var mergeFeature1 = RepoHelper.MergeBranch(feature1Branch);
            RepoHelper.CreateTag("1.0.0");

            RepoHelper.CheckoutBranch(main);
            var mergeFeature2 = RepoHelper.MergeBranch(feature2Branch);

            var commits = _unreleasedCommitsProvider.GetAllUnreleasedMergeCommits(RepoHelper.Repo, main,
                annotatedTagOnly: false).ToList();

            commits.Count().ShouldBe(1);
            commits.ShouldNotContain(x => x.Sha == mergeFeature1.Sha);
            commits.ShouldContain(x => x.Sha == mergeFeature2.Sha);
        }

        /// <summary>
        /// init
        ///   |----
        ///   |   |                         
        ///   |   |-feature 1 commit       
        ///   |   /                      
        ///   |  /                        
        ///   | /                       
        ///   |/ (merge feature1)      
        ///   |                       
        ///   - tag 1.0.0 here for release1
        ///   |   |                          
        ///   |   |-feature 2 commit        
        ///   |   /                        
        ///   |  /                        
        ///   | /                        
        ///   |/ (merge feature2)       
        ///   - tag 2.0.0 here for release2
        ///   |   |
        ///   |   |                          
        ///   |   |-feature 3 commit        
        ///   |   /                        
        ///   |  /                        
        ///   | /                        
        ///   |/ (merge feature3)       
        /// </summary>
        [Test]
        public void CanExcludeFromMultipleTags()
        {
            var main = RepoHelper.CreateAndCheckoutBranch("main");

            var feature1Branch = CreateBranchAndCommit("feature1-branch");
            RepoHelper.CheckoutBranch(main);
            var mergeFeature1 = RepoHelper.MergeBranch(feature1Branch);
            RepoHelper.CreateTag("1.0.0");

            var feature2Branch = CreateBranchAndCommit("feature2-branch");
            RepoHelper.CheckoutBranch(main);
            var mergeFeature2 = RepoHelper.MergeBranch(feature2Branch);
            RepoHelper.CreateTag("2.0.0");

            var feature3Branch = CreateBranchAndCommit("feature3-branch");
            RepoHelper.CheckoutBranch(main);
            var mergeFeature3 = RepoHelper.MergeBranch(feature3Branch);

            var commits = _unreleasedCommitsProvider.GetAllUnreleasedMergeCommits(RepoHelper.Repo, main,
                annotatedTagOnly: false).ToList();

            commits.Count().ShouldBe(1);
            commits.ShouldContain(x => x.Sha == mergeFeature3.Sha);
        }

        /// <summary>
        /// init
        ///   | tag 1.0.0
        ///   |--------------------------------------------------
        ///   |   |                                        |    |
        ///   |   |-feature 2 commit                       |    |
        ///   |   /                                        |    | - fix 1
        ///   |  /               release branch for defect |    |
        ///   | /                                          |   /
        ///   |/ (merge feature2)                          |  /
        ///   |                                            | /
        ///   |---- tag 2.0.0                              |/
        ///   |   |                                        | tag 1.0.1
        ///   |   |                                       /
        ///   |   |                                      /
        ///   |   |                                     /
        ///   |____________merge fix 1 to main_________/
        ///   |   |                          
        ///   |   |-feature 3 commit        
        ///   |   /                        
        ///   |  /                        
        ///   | /                        
        ///   |/ (merge feature3)
        ///   |
        /// </summary>
        [Test]
        public void CanExcludeFromMultipleTagsWhenMultipleReleasesInParallel()
        {
            var main = RepoHelper.CreateAndCheckoutBranch("main");
            RepoHelper.CreateTag("1.0.0");

            var releaseBranchForBugFix = RepoHelper.CreateAndCheckoutBranch("release-branch-for-bug1");
            var bugFix1Branch = CreateBranchAndCommit("bug-fix1-branch");
            RepoHelper.CheckoutBranch(releaseBranchForBugFix);
            var mergeBugFix1 = RepoHelper.MergeBranch(bugFix1Branch);
            RepoHelper.CreateTag("1.0.1");

            RepoHelper.CheckoutBranch(main);
            var feature2Branch = CreateBranchAndCommit("feature2-branch");
            RepoHelper.CheckoutBranch(main);
            var mergeFeature2 = RepoHelper.MergeBranch(feature2Branch);
            RepoHelper.CreateTag("2.0.0");

            var mergeBugFixReleaseToMain = RepoHelper.MergeBranch(releaseBranchForBugFix);

            var feature3Branch = CreateBranchAndCommit("feature3-branch");
            RepoHelper.CheckoutBranch(main);
            var mergeFeature3 = RepoHelper.MergeBranch(feature3Branch);

            var commits = _unreleasedCommitsProvider.GetAllUnreleasedMergeCommits(RepoHelper.Repo, main,
                annotatedTagOnly: false).ToList();

            commits.Count().ShouldBe(2);
            commits.ShouldNotContain(x => x.Sha == mergeFeature2.Sha);
            commits.ShouldNotContain(x => x.Sha == mergeBugFix1.Sha);
            commits.ShouldContain(x => x.Sha == mergeFeature3.Sha);
            commits.ShouldContain(x => x.Sha == mergeBugFixReleaseToMain.Sha);
        }

        /// <summary>
        /// init
        ///   | tag 1.0.0
        ///   |-----
        ///   |    |
        ///   |    |
        ///   |    | - fix 1
        ///   |    |
        ///   |   /
        ///   |  /
        ///   | /
        ///   |/
        ///   | tag 1.0.1
        ///   |\         
        ///   | \__________________
        ///   |                    \  
        ///   |                     |
        ///   |                     |
        ///   |                     | - fix 2
        ///   |                     |
        ///   |                     | tag 1.0.2 
        ///   | - head is here
        /// </summary>
        [Test]
        public void TagShouldBeIncludedIfReachableFromAnotherUnreachableTag()
        {
            var main = RepoHelper.CreateAndCheckoutBranch("main");
            RepoHelper.CreateTag("1.0.0");
            var fix1 = CreateBranchAndCommit("fix-1");
            RepoHelper.CheckoutBranch(main);
            RepoHelper.MergeBranch(fix1);
            RepoHelper.CreateTag("1.0.1");

            var fix2 = CreateBranchAndCommit("fix-2");
            RepoHelper.CreateTag("1.0.2");

            RepoHelper.CheckoutBranch(main);

            var commits = _unreleasedCommitsProvider.GetAllUnreleasedMergeCommits(RepoHelper.Repo, main,
                annotatedTagOnly: false).ToList();

            commits.ShouldBeEmpty();
        }

        /// <summary>
        /// init
        ///   |-----------
        ///   |          |                
        ///   |          |-feature 1 commit
        ///   |         /|               
        ///   |        / | 
        ///   |       /  |
        ///   |      /   | release feature on non-main branch
        ///   |     /    | tag 1.0.0       
        ///   |____/     
        ///   |             
        /// </summary>
        [Test]
        public void IncludeEvenReleasedInAnotherBranch()
        {
            var main = RepoHelper.CreateAndCheckoutBranch("main");
            var feature1Branch = CreateBranchAndCommit("feature1-branch");
            RepoHelper.CheckoutBranch(main);
            var mergeFeature1 = RepoHelper.MergeBranch(feature1Branch);

            RepoHelper.CheckoutBranch(feature1Branch);
            RepoHelper.CreateTag("1.0.0");

            var commits = _unreleasedCommitsProvider.GetAllUnreleasedMergeCommits(RepoHelper.Repo, main,
                annotatedTagOnly: false).ToList();

            commits.Count().ShouldBe(1);
            commits.ShouldContain(x => x.Sha == mergeFeature1.Sha);
        }

        [Test]
        public void AbleToWorkWithAnnotatedTag()
        {
            var main = RepoHelper.CreateAndCheckoutBranch("main");
            var feature1Branch = CreateBranchAndCommit("feature1-branch");
            RepoHelper.CheckoutBranch(main);
            var mergeFeature1 = RepoHelper.MergeBranch(feature1Branch);
            RepoHelper.CreateAnnotatedTag("1.0.0");
            var feature2Branch = CreateBranchAndCommit("feature2-branch");
            RepoHelper.CheckoutBranch(main);
            var mergeFeature2 = RepoHelper.MergeBranch(feature2Branch);

            var commits = _unreleasedCommitsProvider.GetAllUnreleasedMergeCommits(RepoHelper.Repo, main,
                annotatedTagOnly: false).ToList();

            commits.Count().ShouldBe(1);
            commits.ShouldContain(x => x.Sha == mergeFeature2.Sha);
        }

        [Test]
        public void IgnoreLightTagWhenInAnnotatedOnlyMode()
        {
            var main = RepoHelper.CreateAndCheckoutBranch("main");
            var feature1Branch = CreateBranchAndCommit("feature1-branch");
            RepoHelper.CheckoutBranch(main);
            var mergeFeature1 = RepoHelper.MergeBranch(feature1Branch);
            RepoHelper.CreateTag("1.0.0");
            var feature2Branch = CreateBranchAndCommit("feature2-branch");
            RepoHelper.CheckoutBranch(main);
            var mergeFeature2 = RepoHelper.MergeBranch(feature2Branch);

            var commits = _unreleasedCommitsProvider.GetAllUnreleasedMergeCommits(RepoHelper.Repo, main,
                annotatedTagOnly: true).ToList();

            commits.Count().ShouldBe(2);
            commits.ShouldContain(x => x.Sha == mergeFeature1.Sha);
            commits.ShouldContain(x => x.Sha == mergeFeature2.Sha);
        }
    }
}
