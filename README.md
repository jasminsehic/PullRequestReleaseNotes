# UnreleasedGitHubHistory

[![Build status](https://ci.appveyor.com/api/projects/status/github/jasminsehic/unreleasedgithubhistory?svg=true)](https://ci.appveyor.com/project/jasminsehic/unreleasedgithubhistory)
[![Chocolatey](https://img.shields.io/chocolatey/v/unreleasedgithubhistory.portable.svg)](https://chocolatey.org/packages/UnreleasedGitHubHistory.Portable)
[![Release](https://img.shields.io/github/release/jasminsehic/unreleasedgithubhistory.svg)]()
[![License](https://img.shields.io/github/license/jasminsehic/unreleasedgithubhistory.svg)]()

UnreleasedGitHubHistory is a utility which generates release notes for all merged GitHub pull requests that have not yet been released (since last tag) from a specific branch and optionally publishes it to a markdown file and/or posts it to Atlassian Confluence page which is then rendered using the Render Markdown plugin. Intention is to run this utility as part of a CI process and generate notes automatically as part of every build of a head branch.

Utility will use GitHub pull request titles and labels to group and sort the release notes. For example all pull requests with Bug label will be grouped under Fixes section in notes and pull requests with Enhancement label will be grouped under Enhancements section. Secondary level of grouping is possible through use of the #Label where # character is used to signify second level of grouping. You can supply the utility with secondary label descriptions so that you can turn label CompA into Component A description. Pull requests without relevant labels will be grouped under Unclassified and Undefined sections. Pull requests labeled with multiple secondary labels will cause notes to appear in multiple sections.

## Sample output

```markdown
## Enhancements
### Component A
- Awesome new feature [\#1854](https://github.com/org/repo/pull/1854)

### Undefined
- Special feature for Acme Co [\#1855](https://github.com/org/repo/pull/1855)

## Fixes
### Component Z
- Fixed problem with widget [\#1792](https://github.com/org/repo/pull/1792)

### Component Y
- Fixed problem with widget [\#1792](https://github.com/org/repo/pull/1792)
- Fixed exception with view layout [\#1848](https://github.com/org/repo/pull/1848)

### Undefined
- Fixed spelling mistake [\#1833](https://github.com/org/repo/pull/1833)

## Unclassified
### Undefined
- Added new Component H [\#1843](https://github.com/org/repo/pull/1843)
```
## Usage
```{r, engine='bash', count_lines}
$ UnreleasedGitHubHistory -ghpt 30aee6853987d30da50732c4f849bfbfd24c091e -ghld "CompA=Component A,CompZ=Component Z" -gho organisation -ghr repo -ghb "refs/heads/master" -grp "D:\Dev\Repo" -ptc -cpp 328432 -cu confluenceUser -cp confluencePwd -csk SPCKEY -cau "https://company.atlassian.net/wiki/rest/api"
```

Only required parameter is the GitHubToken. It can be supplied via command line or via UNRELEASED_HISTORY_GITHUB_TOKEN environment variable. Other parameters will be automatically determined from the Git repository if you run UnreleasedGitHubHistory application within a directory inside a Git working directory.

### Command Line Arguments
- GitHubToken (-ghpt)
- GitHubOwner (-gho)
- GitHubRepository (-ghr)
- GitHubLabelDescriptionList (-ghld)
- ReleaseBranchRef (-ghb)
- GitRepositoryPath (-grp)
- GitRemote (-gr)
- PublishToConfluence (-ptc)
- ConfluenceReleaseParentPageId (-cpp)
- ConfluenceSpaceKey (-csk)
- ConfluenceUser (-cu)
- ConfluencePassword (-cp)
- ConfluenceApiUrl (-cau)
- VerboseOutput (-v)
- AcceptInvalidCertificates (-aic)
- PublishToFile (-ptf)
- OutputFileName (-o)