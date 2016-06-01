# UnreleasedGitHubHistory

[![Build status](https://ci.appveyor.com/api/projects/status/github/jasminsehic/unreleasedgithubhistory?svg=true)](https://ci.appveyor.com/project/jasminsehic/unreleasedgithubhistory)
[![Chocolatey](https://img.shields.io/chocolatey/vpre/unreleasedgithubhistory.portable.svg)](https://chocolatey.org/packages/UnreleasedGitHubHistory.Portable)
[![Release](https://img.shields.io/github/release/jasminsehic/unreleasedgithubhistory.svg)]()
[![License](https://img.shields.io/github/license/jasminsehic/unreleasedgithubhistory.svg)]()

UnreleasedGitHubHistory is a utility which generates release notes for all merged pull requests that have not yet been released (since last tag) from a specific branch and optionally publishes it to a markdown file and/or posts it to Atlassian Confluence page which is then rendered using the Render Markdown plugin. Intention is to run this utility as part of a CI process and generate notes automatically as part of every build of a head branch.

Utility will use pull request titles and labels to group and sort the release notes. For example all pull requests with Bug label will be grouped under Fixes section in notes and pull requests with Enhancement label will be grouped under Enhancements section. Secondary level (categories) of grouping is possible through use of the #Label where # character is used to signify second level of grouping. You can supply the utility with category descriptions so that you can turn label CategoryA into Category A description. Pull requests without relevant labels will be grouped under Unclassified and Undefined sections. Pull requests labeled with multiple category labels will cause notes to appear in multiple categories.

## Install

    choco install UnreleasedGitHubHistory.Portable
    
## Usage

Utility can have command line parameters passed to it or have the parameters supplied via a YAML based config. You can generate a sample YAML file by passing -init parameter to the utility.

```{r, engine='bat', count_lines}
$ UnreleasedGitHubHistory -ghpt 30aee6853987d30da50732c4f849bfbfd24c091e -ptc -cpp 328432 -cu confluenceUser -cp confluencePwd -csk SPCKEY -cau "https://company.atlassian.net/wiki/rest/api"
```

### Command Line Parameters
- PullRequestProviderName (-prpn) : Default is github. gitlab and tfs is also supported.
- GitHubToken (-ghpt) : Required parameter if PullRequestProviderName is github. Can be supplied as parameter or UNRELEASED_HISTORY_GITHUB_TOKEN environment variable.
- GitHubOwner (-gho) : Default is extracted from remote url.
- GitHubRepository (-ghr) : Default is extracted from remote url.
- GitHubApiUrl (-glau) : Default is https://github.com
- GitLabToken (-glpt) : Required parameter if PullRequestProviderName is gitlab. Can be supplied as parameter or UNRELEASED_HISTORY_GITLAB_TOKEN environment variable.
- GitLabOwner (-glo) : Default is extracted from remote url.
- GitLabRepository (-glr) : Default is extracted from remote url.
- GitLabApiUrl (-glau) : Default is https://gitlab.com
- GitLabProjectId (-glpi) : Required parameter if PullRequestProviderName is gitlab. Set it to your GitLab project identifier.
- TfsUsername (-tu) : Required parameter if PullRequestProviderName is tfs. For VSO personal tokens use anything, for VSO alternate credentials and on-prem TFS use the username.
- TfsToken (-tpt) : Required parameter if PullRequestProviderName is tfs. Can be supplied as parameter or UNRELEASED_HISTORY_TFS_TOKEN environment variable. For VSO personal tokens use the token itself and for VSO alternate credentials and on-prem TFS use the user password.
- TfsCollection (-tc) : Default is extracted from remote url.
- TfsRepository (-tr) : Default is extracted from remote url.
- TfsApiUrl (-tau) : Required parameter if PullRequestProviderName is tfs.
- GitRemote (-gr) : Default ("origin"). If not found it will search through all remotes.
- GitVersion (-gv) : Default ("Unreleased"). Can be supplied as parameter or GITVERSION_MAJORMINORPATCH environment variable.
- ReleaseBranchRef (-ghb) : Default is head branch.
- ReleaseBranchHeadsOnly (-rbho) : Default is ("true"). Set to false to generate notes from any branch.
- ReleaseNoteSectioned (-rns) : Default ("false"). Set to "true" to enable note sections.
- ReleaseNoteSections (-rnsl) : Default ("bug=Fixes,enhancement=Enhancements"). Key value pairs of pull request labels and their descriptions used for note sections.
- ReleaseNoteSectionlessDescription (-rnsd) : Default ("Undefined").
- ReleaseNoteUncategorisedDescription (-rnud) : Default ("Unclassified").
- ReleaseNoteCategorised (-rnc) : Default ("false"). Set to "true" to enable note categorisation.
- ReleaseNoteCategories (-rncl) : Example ("CatA=Category A,catB=Category B"). Key value pairs of pull request labels and their descriptions used for note categorisation.
- ReleaseNoteCategoryPrefix (-rncp) : Default ("#"). Used to differentiate category labels from section labels.
- ReleaseNoteOrderAscending (-rnoa) : Default ("false"). Used to determine the sort order of the release notes.
- ReleaseNoteOrderWhen (-rnow) : Default ("merged"). Set to "created" to order release notes based on pull request creation time rather than merge time.
- ReleaseNoteFormat (-rnf) : Default ("{0} {1}"). Available fields are {0} pull request title, {1} pull request url, {2} pull request number, {3} pull request created date/time, {4} pull request merged date/time, {5} pull request author username, {6} pull request author URL
- ReleaseNoteDateFormat (-rndf) : Default ("MMM dd, yyyy HH:mm"). You can use any .NET standard or custom date and time format strings.
- PublishToConfluence (-ptc) : Default ("false"). Set to "true" for all other Confluence related parameters to become active.
- ConfluenceReleaseParentPageId (-cpp) : Confluence parent page identifer. Pulished page will be its child page.
- ConfluenceSpaceKey (-csk) : Required parameter if PublishToConfluence is true.
- ConfluenceUser (-cu) : Required parameter if PublishToConfluence is true.
- ConfluencePassword (-cp) : Required parameter if PublishToConfluence is true.
- ConfluenceApiUrl (-cau) : Required parameter if PublishToConfluence is true.
- VerboseOutput (-v) : Default ("false"). Set to "true" to output more information about what the utility is doing.
- AcceptInvalidCertificates (-aic) : Default ("false"). Set to "true" to help when using Fiddler to debug HTTP responses.
- PublishToFile (-ptf) : Default ("false"). Set to "true" to output markdown to a local filename supplied by OutputFileName parameter.
- OutputFileName (-o) : Default ("Unreleased.md").
- ExcludeLabel (-el) : Default ("Exclude Note"). Pull request label which once found will cause the entire pull request to be excluded from release notes.
- FollowLabel (-fl) : Default ("Follow Note"). Pull request label which once found will cause the tool to recursively follow all other pull request merge commits within the pull request.
- Init (-init) : When provided the utility will generate a sample UnreleasedGitHubHistory.yml file at the root of the Git repository and not generate any notes.

### YAML File Parameters

See Command Line Parameters for details on default values or parameter usage

- release-note-sections
- release-note-sectionless-description
- release-note-uncategorised-description
- release-note-sectioned
- release-note-categorised
- release-note-categories
- release-note-category-prefix
- release-note-order-ascending
- release-note-order-when
- git-branch-ref
- git-repo-path
- confluence-publish
- confluence-release-parent-page-id
- confluence-space-key
- confluence-username
- confluence-password
- verbose
- accept-invalid-certificates
- file-publish
- file-name
- confluence-api-url
- git-remote-name
- release-note-exclude
- release-note-follow
- git-version
- release-note-date-format
- release-note-format
- release-branch-heads-only
- pull-request-provider-name
- github-api-url
- github-token
- github-owner
- github-repo
- gitlab-token
- gitlab-owner
- gitlab-repo
- gitlab-api-url
- gitlab-project-id
- tfs-api-url
- tfs-collection
- tfs-repository
- tfs-username
- tfs-token


### Config File Sample

```yaml
pull-request-provider-name: github | gitlab | tfs
release-branch-heads-only: true
release-note-exclude: Exclude Note
release-note-follow: Follow Note
release-note-format: "{0} {1}"
release-note-date-format: "MMM dd, yyyy HH:mm"
release-note-sectioned: true
release-note-order-when: merged
release-note-sectionless-description: Undefined
release-note-sections:
  - bug=Fixes
  - enhancement=Enhancements
```

## Sample output

```markdown
# 1.2.1 (MASTER) - XX XXX 2016
## Enhancements
### Category A
- Awesome new feature [\#1854](https://github.com/org/repo/pull/1854)

### Undefined
- Special feature for Acme Co [\#1855](https://github.com/org/repo/pull/1855)

## Fixes
### Category Z
- Fixed problem with widget [\#1792](https://github.com/org/repo/pull/1792)

### Category Y
- Fixed problem with widget [\#1792](https://github.com/org/repo/pull/1792)
- Fixed exception with view layout [\#1848](https://github.com/org/repo/pull/1848)

### Undefined
- Fixed spelling mistake [\#1833](https://github.com/org/repo/pull/1833)

## Unclassified
### Undefined
- Added new Category H [\#1843](https://github.com/org/repo/pull/1843)
```
