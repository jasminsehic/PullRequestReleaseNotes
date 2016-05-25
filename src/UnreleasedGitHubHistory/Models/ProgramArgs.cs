using System;
using System.Collections.Generic;
using PowerArgs;

namespace UnreleasedGitHubHistory.Models
{
    public class ProgramArgs
    {
        [ArgShortcut("-rnsl")]
        [DefaultValue("bug=Fixes,enhancement=Enhancements")]
        [ArgExample("'Section1=Description For Section1,Section2=Description For Section2'", "Dictionary of GitHub Pull Request Labels and their descriptions which will be used for release note sections")]
        public List<string> ReleaseNoteSections { get; set; }

        [ArgShortcut("-rnsd")]
        [DefaultValue("Undefined")]
        public string ReleaseNoteSectionlessDescription { get; set; }

        [ArgShortcut("-rnud")]
        [DefaultValue("Unclassified")]
        public string ReleaseNoteUncategorisedDescription { get; set; }

        [DefaultValue(true)]
        [ArgShortcut("-rns")]
        public bool ReleaseNoteSectioned { get; set; }

        [DefaultValue(true)]
        [ArgShortcut("-rnc")]
        public bool ReleaseNoteCategorised { get; set; }

        [ArgShortcut("-rncl")]
        [ArgExample("'Category1=Description For Category1,Category2=Description For Category2'", "Dictionary of GitHub Pull Request Labels and their descriptions which will be used for release note categorisation")]
        public List<string> ReleaseNoteCategories { get; set; }

        [DefaultValue("#")]
        [ArgShortcut("-rncp")]
        public string ReleaseNoteCategoryPrefix { get; set; }

        [DefaultValue(true)]
        [ArgShortcut("-rnod")]
        public bool ReleaseNoteOrderDescending { get; set; }

        [DefaultValue("merged")] // or created
        [ArgShortcut("-rnow")]
        public string ReleaseNoteOrderWhen { get; set; }

        [ArgShortcut("-ghpt")]
        [ArgExample("30aee2825c48560da50732c4f849bfbfd24c091e", "GitHub Personal Token")]
        public string GitHubToken { get; set; }

        [ArgShortcut("-gho")]
        [ArgExample("company", "GitHub Organisational Account")]
        public string GitHubOwner { get; set; }

        [ArgShortcut("-ghr")]
        [ArgExample("repo", "GitHub Repository Name")]
        public string GitHubRepository { get; set; }

        [ArgShortcut("-ghb")]
        [ArgExample("refs/heads/master", "Git head branch reference")]
        public string ReleaseBranchRef { get; set; }

        [ArgShortcut("-grp")]
        [ArgExample(@"D:\Dev\Repo", "Local Git repository path")]
        public string GitRepositoryPath { get; set; }
    
        [DefaultValue(false)]
        [ArgShortcut("-ptc")]
        public bool PublishToConfluence { get; set; }

        [ArgExample("1223543", "Reference to Confluence release page parent ID")]
        [ArgShortcut("-cpp")]
        public string ConfluenceReleaseParentPageId { get; set; }

        [ArgExample("ABC", "Confluence space key")]
        [ArgShortcut("-csk")]
        public string ConfluenceSpaceKey { get; set; }

        [ArgExample("johndoe", "Confluence username under which the page will be published")]
        [ArgShortcut("-cu")]
        public string ConfluenceUser { get; set; }

        [ArgExample("******", "Confluence user password under which the page will be published")]
        [ArgShortcut("-cp")]
        public string ConfluencePassword { get; set; }

        [ArgShortcut("-v")]
        public bool VerboseOutput { get; set; }

        [DefaultValue(false)]
        [ArgShortcut("-aic")]
        public bool AcceptInvalidCertificates { get; set; }

        [DefaultValue(false)]
        [ArgShortcut("-ptf")]
        public bool PublishToFile { get; set; }

        [DefaultValue("Unreleased.md")]
        [ArgShortcut("-o")]
        public string OutputFileName { get; set; }

        [ArgShortcut("-cau")]
        public Uri ConfluenceApiUrl { get; set; }

        [DefaultValue("origin")]
        [ArgShortcut("-gr")]
        public string GitRemote { get; set; }

        [DefaultValue("Exclude Note")]
        [ArgShortcut("-el")]
        public string ExcludeLabel { get; set; }

        [DefaultValue("Follow Note")]
        [ArgShortcut("-fl")]
        public string FollowLabel { get; set; }

        [ArgShortcut("-gv")]
        public string GitVersion { get; set; }
    }
}