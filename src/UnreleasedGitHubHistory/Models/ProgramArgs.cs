using System;
using System.Collections.Generic;
using PowerArgs;

namespace UnreleasedGitHubHistory.Models
{
    public class ProgramArgs
    {
        [ArgRequired]
        [ArgShortcut("-ghpt")]
        [ArgExample("30aee2825c48560da50732c4f849bfbfd24c091e", "GitHub Personal Token")]
        public string GitHubToken { get; set; }

        [ArgRequired]
        [ArgShortcut("-gho")]
        [ArgExample("company", "GitHub Organisational Account")]
        public string GitHubOwner { get; set; }

        [ArgRequired]
        [ArgShortcut("-ghr")]
        [ArgExample("repo", "GitHub Repository Name")]
        public string GitHubRepository { get; set; }

        [ArgShortcut("-ghld")]
        [ArgExample("'Label1=Description For Label1,Label2=Description For Label2'", "Hash map of GitHub Pull Request Labels and their descriptions")]
        public List<string> GitHubLabelDescriptionList { get; set; }

        [ArgRequired]
        [DefaultValue("refs/heads/master")]
        [ArgShortcut("-ghb")]
        [ArgExample("refs/heads/master", "Git head branch reference")]
        public string ReleaseBranchRef { get; set; }

        [DefaultValue(".")]
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
    }
}