![Icon](https://raw.github.com/jasminsehic/PullRequestReleaseNotes/master/logo.png)

PullRequestReleaseNotes
=======================

[![Join the chat at https://gitter.im/jasminsehic/PullRequestReleaseNotes](https://badges.gitter.im/jasminsehic/PullRequestReleaseNotes.svg)](https://gitter.im/jasminsehic/PullRequestReleaseNotes?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

[![Build status](https://ci.appveyor.com/api/projects/status/github/jasminsehic/pullrequestreleasenotes?svg=true)](https://ci.appveyor.com/project/jasminsehic/unreleasedgithubhistory)
[![Chocolatey](https://img.shields.io/chocolatey/vpre/PullRequestReleaseNotes.portable.svg)](https://chocolatey.org/packages/PullRequestReleaseNotes.Portable)
[![Release](https://img.shields.io/github/release/jasminsehic/PullRequestReleaseNotes.svg)]()
[![License](https://img.shields.io/github/license/jasminsehic/PullRequestReleaseNotes.svg)]()

PullRequestReleaseNotes generates release notes for all merged pull requests on a specific branch that have not yet been released or since last release tag. Supported Pull Request providers are [GitHub](https://github.com/), [GitLab](https://gitlab.com/), [TFS / Team Services](https://www.visualstudio.com/en-us/products/visual-studio-team-services-vs.aspx), [BitBucket Cloud](https://bitbucket.org/) and [Bitbucket Server](https://www.atlassian.com/software/bitbucket/download). 

Intention is to run this utility as part of a continuous integration process and generate notes automatically as part of every release branch build. Optionally the utility can also publish the notes to a markdown file, [Atlassian Confluence](https://www.atlassian.com/software/confluence) page or a [Slack](https://slack.com/) post. 

Utility outputs release notes following the [SemanticReleaseNotes.org](http://www.semanticreleasenotes.org/) format and uses the pull request titles and labels to extract semantic release note sections, categories and summaries. For example all pull requests with `Bug` label can be grouped under `Fixes `section and pull requests with `Enhancement` label can be grouped under `Enhancements` section. Category grouping is possible through use of the `#Label` where `#` character is used to signify a category. You can supply the utility with category descriptions so that you can turn label `CategoryA` into `Category A` description. Pull requests without relevant labels will be grouped under `Unclassified` and `Undefined` sections and categories. Pull requests labeled with multiple category labels will cause notes to appear in multiple categories. 

NOTE: TFS / Team Services and BitBucket Cloud / Server pull request providers to not have a label/tag concept on pull requests so for those providers you can type `[#section]` and `[##category]` either in the title or the description of the pull request as a pseudo-label/tag.

## Install

    choco install PullRequestReleaseNotes.Portable
    
## Usage

```{r, engine='bat', count_lines}
$ PullRequestReleaseNotes
```

Utility can have command line parameters passed to it or have the parameters supplied via a YAML based config or a mixture of both. You can generate a sample YAML file by passing -init parameter to the utility.

### Command Line Parameters
- `-PullRequestProviderName` (`-prpn`) : Default is `github`. Other providers supported are `gitlab`, `tfs`, `bitbucket` and `bitbucketserver`.
- `-GitHubToken` (`-ghpt`) : Required parameter if PullRequestProviderName is `github`. Can be supplied as parameter or `UNRELEASED_HISTORY_GITHUB_TOKEN` environment variable.
- `-GitHubOwner` (`-gho`) : Default is extracted from remote url.
- `-GitHubRepository` (`-ghr`) : Default is extracted from remote url.
- `-GitHubApiUrl` (`-glau`) : Default is https://github.com
- `-GitLabToken` (`-glpt`) : Required parameter if PullRequestProviderName is `gitlab`. Can be supplied as parameter or `UNRELEASED_HISTORY_GITLAB_TOKEN` environment variable.
- `-GitLabOwner` (`-glo`) : Default is extracted from remote url.
- `-GitLabRepository` (`-glr`) : Default is extracted from remote url.
- `-GitLabApiUrl` (`-glau`) : Default is https://gitlab.com
- `-GitLabProjectId` (`-glpi`) : Required parameter if PullRequestProviderName is `gitlab`. Set it to your GitLab project identifier.
- `-TfsUsername` (`-tu`) : Required parameter if PullRequestProviderName is `tfs`. For Team Services personal tokens use anything, for Team Services alternate credentials and on-premise TFS use the username.
- `-TfsToken` (`-tpt`) : Required parameter if PullRequestProviderName is `tfs`. Can be supplied as parameter or `UNRELEASED_HISTORY_TFS_TOKEN` environment variable. For Team Services personal tokens use the token itself and for Team Services alternate credentials and on-premise TFS use the user password.
- `-TfsCollection` (`-tc`) : Default is extracted from remote url.
- `-TfsRepository` (`-tr`) : Default is extracted from remote url.
- `-TfsApiUr`l (`-tau`) : Required parameter if PullRequestProviderName is `tfs`.
- `-BitBucketServerUrl` (`-bbsu`) : Required parameter if PullRequestProviderName is `bitbucketserver`.
- `-BitBucketServerUsername` (`-bbsun`) : Required parameter if PullRequestProviderName is `bitbucketserver`.
- `-BitBucketServerPassword` (`-bbsp`) : Required parameter if PullRequestProviderName is `bitbucketserver`. Can be supplied as parameter or `UNRELEASED_HISTORY_BITBUCKETSERVER_PASSWORD` environment variable.
- `-BitBucketServerProject` (`-bbspk`) : Required parameter if PullRequestProviderName is `bitbucketserver`.
- `-BitBucketServerRepository` (`-bbsr`) : Required parameter if PullRequestProviderName is `bitbucketserver`. 
- `-BitBucketApiKey` (`-bbak`) : Required parameter if PullRequestProviderName is `bitbucket`.
- `-BitBucketApiSecret` (`-bbas`) : Required parameter if PullRequestProviderName is `bitbucket`. Can be supplied as parameter or `UNRELEASED_HISTORY_BITBUCKET_SECRET` environment variable.
- `-BitBucketAccount` (`-bba`) : Required parameter if PullRequestProviderName is `bitbucket`.
- `-BitBucketRepository` (`-bbr`) : Required parameter if PullRequestProviderName is `bitbucket`. 
- `-GitRemote` (`-gr`) : Default ("origin"). If not found it will search through all remotes.
- `-GitVersion` (`-gv`) : Default ("Unreleased"). Can be supplied as parameter or `GITVERSION_MAJORMINORPATCH` environment variable.
- `-GitTagsAnnotated` (`-gta`) : Default ("false"). Set to "true" to only consider annotated tags as releases.
- `-ReleaseBranchRef` (`-ghb`) : Default is head branch.
- `-ReleaseBranchHeadsOnly` (`-rbho`) : Default is ("true"). Set to false to generate notes from any branch.
- `-ReleaseNoteSectioned` (`-rns`) : Default ("false"). Set to "true" to enable note sections.
- `-ReleaseNoteSections`(`-rnsl`) : Default ("bug=Fixes,enhancement=Enhancements"). Key value pairs of pull request labels and their descriptions used for note sections.
- `-ReleaseNoteSectionlessDescription` (`-rnsd`) : Default ("Undefined").
- `-ReleaseNoteUncategorisedDescription` (`-rnud`) : Default ("Unclassified").
- `-ReleaseNoteCategorised` (`-rnc`) : Default ("false"). Set to "true" to enable note categorisation.
- `-ReleaseNoteCategories` (`-rncl`) : Example ("CatA=Category A,catB=Category B"). Key value pairs of pull request labels and their descriptions used for note categorisation.
- `-ReleaseNoteCategoryPrefix` (`-rncp`) : Default ("#"). Used to differentiate category labels from section labels.
- `-ReleaseNoteOrderAscending` (`-rnoa`) : Default ("false"). Used to determine the sort order of the release notes.
- `-ReleaseNoteOrderWhen` (`-rnow`) : Default ("merged"). Set to "created" to order release notes based on pull request creation time rather than merge time.
- `-ReleaseNoteFormat` (`-rnf`) : Default ("{0} {1}"). Available fields are {0} pull request title, {1} pull request url, {2} pull request number, {3} pull request created date/time, {4} pull request merged date/time, {5} pull request author username, {6} pull request author URL
- `-ReleaseNoteDateFormat` (`-rndf`) : Default ("MMM dd, yyyy HH:mm"). You can use any [.NET standard](https://msdn.microsoft.com/en-us/library/az4se3k1(v=vs.110).aspx) or [custom date and time format](https://msdn.microsoft.com/en-us/library/8kb3ddd4(v=vs.110).aspx) strings.
- `-ReleaseNoteHighlightLabels` (`-rnhl`) : Default is (""). Comma-separated list of labels which a pull request without will be marked up as code to highlight the item in release notes.'
- `-PublishToConfluence` (`-ptc`) : Default ("false"). Set to "true" for all other Confluence related parameters to become active.
- `-ConfluenceReleaseParentPageId` (`-cpp`) : Confluence parent page identifer. Pulished page will be its child page.
- `-ConfluenceSpaceKey` (`-csk`) : Required parameter if `PublishToConfluence` is true.
- `-ConfluenceUser` (`-cu`) : Required parameter if `PublishToConfluence` is true.
- `-ConfluencePassword` (`-cp`) : Required parameter if `PublishToConfluence` is true.
- `-ConfluenceApiUrl` (`-cau`) : Required parameter if `PublishToConfluence` is true.
- `-PublishToSlack` (`-pts`) : Default ("false"). Set to "true" for all other Slack related parameters to become active.
- `-SlackToken` (`-st`) : Required parameter if `PublishToSlack` is true. Set to your personal Slack token.
- `-SlackChannels` (`-cau`) : Required parameter if `PublishToSlack` is true. Set to a comma-separated list of channel identifiers.
- `-VerboseOutput` (`-v`) : Default ("false"). Set to "true" to output more information about what the utility is doing.
- `-AcceptInvalidCertificates` (`-aic`) : Default ("false"). Set to "true" to help when using Fiddler to debug HTTP responses.
- `-PublishToFile` (`-ptf`) : Default ("false"). Set to "true" to output markdown to a local filename supplied by `OutputFileName` parameter.
- `-OutputFileName` (`-o`) : Default ("Unreleased.md").
- `-ExcludeLabel` (`-el`) : Default ("Exclude Note"). Pull request label which once found will cause the entire pull request to be excluded from release notes.
- `FollowLabel` (`-fl`) : Default ("Follow Note"). Pull request label which once found will cause the tool to recursively follow all other pull request merge commits within the pull request.
- `-Init` (`-init`) : When provided the utility will generate a sample `PullRequestReleaseNotes.yml` file at the root of the Git repository and not generate any notes.

### YAML File Parameters

See Command Line Parameters for details on default values or parameter usage

- `pull-request-provider-name`
- `release-note-sections`
- `release-note-sectionless-description`
- `release-note-uncategorised-description`
- `release-note-sectioned`
- `release-note-categorised`
- `release-note-categories`
- `release-note-category-prefix`
- `release-note-order-ascending`
- `release-note-order-when`
- `release-note-exclude`
- `release-note-follow`
- `release-note-date-format`
- `release-note-format`
- `release-branch-heads-only`
- `release-note-highlight-labels`
- `git-branch-ref`
- `git-repo-path`
- `git-remote-name`
- `git-version`
- `git-tags-annotated`
- `confluence-publish`
- `confluence-release-parent-page-id`
- `confluence-space-key`
- `confluence-api-url`
- `confluence-username`
- `confluence-password`
- `slack-publish`
- `slack-token`
- `slack-channels`
- `verbose`
- `accept-invalid-certificates`
- `file-publish`
- `file-name`
- `github-api-url`
- `github-token`
- `github-owner`
- `github-repo`
- `gitlab-token`
- `gitlab-owner`
- `gitlab-repo`
- `gitlab-api-url`
- `gitlab-project-id`
- `tfs-api-url`
- `tfs-collection`
- `tfs-repository`
- `tfs-username`
- `tfs-token`
- `bitbucketserver-username`
- `bitbucketserver-password`
- `bitbucketserver-url`
- `bitbucketserver-project-key`
- `bitbucketserver-repository`
- `bitbucket-api-key`
- `bitbucket-api-secret`
- `bitbucket-account`
- `bitbucket-repository`

### Config File Sample

```yaml
pull-request-provider-name: github | gitlab | tfs | bitbucketserver | bitbucket
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
