### Command Line Parameters
Utility can have command line parameters passed to it or have the parameters supplied via a YAML based config or a mixture of both. Command line parameter will win if the same parameter is also supplied via YAML. You can generate a sample YAML file by passing -init parameter to the utility.

- `-PullRequestProviderName` (`-prpn`) : Default is `github`. Other providers supported are `gitlab`, `tfs`, `bitbucket` and `bitbucketserver`.
- `-GitHubToken` (`-ghpt`) : Required parameter if PullRequestProviderName is `github`. Can be supplied as parameter or `PRRN_GITHUB_TOKEN` environment variable.
- `-GitHubOwner` (`-gho`) : Default is extracted from remote url.
- `-GitHubRepository` (`-ghr`) : Default is extracted from remote url.
- `-GitHubApiUrl` (`-glau`) : Default is https://github.com
- `-GitLabToken` (`-glpt`) : Required parameter if PullRequestProviderName is `gitlab`. Can be supplied as parameter or `PRRN_GITLAB_TOKEN` environment variable.
- `-GitLabOwner` (`-glo`) : Default is extracted from remote url.
- `-GitLabRepository` (`-glr`) : Default is extracted from remote url.
- `-GitLabApiUrl` (`-glau`) : Default is https://gitlab.com
- `-GitLabProjectId` (`-glpi`) : Required parameter if PullRequestProviderName is `gitlab`. Set it to your GitLab project identifier.
- `-TfsUsername` (`-tu`) : Required parameter if PullRequestProviderName is `tfs`. For Team Services personal tokens use anything, for Team Services alternate credentials and on-premise TFS use the username.
- `-TfsToken` (`-tpt`) : Required parameter if PullRequestProviderName is `tfs`. Can be supplied as parameter or `PRRN_TFS_TOKEN` environment variable. For Team Services personal tokens use the token itself and for Team Services alternate credentials and on-premise TFS use the user password.
- `-TfsCollection` (`-tc`) : Default is extracted from remote url.
- `-TfsRepository` (`-tr`) : Default is extracted from remote url.
- `-TfsApiUr`l (`-tau`) : Required parameter if PullRequestProviderName is `tfs`.
- `-BitBucketServerUrl` (`-bbsu`) : Required parameter if PullRequestProviderName is `bitbucketserver`.
- `-BitBucketServerUsername` (`-bbsun`) : Required parameter if PullRequestProviderName is `bitbucketserver`.
- `-BitBucketServerPassword` (`-bbsp`) : Required parameter if PullRequestProviderName is `bitbucketserver`. Can be supplied as parameter or `PRRN_BITBUCKETSERVER_PASSWORD` environment variable.
- `-BitBucketServerProject` (`-bbspk`) : Required parameter if PullRequestProviderName is `bitbucketserver`.
- `-BitBucketServerRepository` (`-bbsr`) : Required parameter if PullRequestProviderName is `bitbucketserver`. 
- `-BitBucketApiKey` (`-bbak`) : Required parameter if PullRequestProviderName is `bitbucket`.
- `-BitBucketApiSecret` (`-bbas`) : Required parameter if PullRequestProviderName is `bitbucket`. Can be supplied as parameter or `PRRN_BITBUCKET_SECRET` environment variable.
- `-BitBucketAccount` (`-bba`) : Required parameter if PullRequestProviderName is `bitbucket`.
- `-BitBucketRepository` (`-bbr`) : Required parameter if PullRequestProviderName is `bitbucket`. 
- `-GitRemote` (`-gr`) : Default ("origin"). If not found it will search through all remotes.
- `-GitVersion` (`-gv`) : Default ("Unreleased"). Can be supplied as parameter or `GITVERSION_MAJORMINORPATCH` environment variable.
- `-GitTagsAnnotated` (`-gta`) : Default ("false"). Set to "true" to only consider annotated tags as releases.
- `-ReleaseBranchRef` (`-ghb`) : Default is head branch.
- `-ReleaseBranchHeadsOnly` (`-rbho`) : Default is ("true"). When set to true it will only generate notes from any head branch and not pull request branches. When set to false it will generate notes for any branch.
- `-ReleaseNoteSectioned` (`-rns`) : Default ("false"). Set to "true" to enable note sections.
- `-ReleaseNoteSections`(`-rnsl`) : Default ("bug=Fixes,enhancement=Enhancements"). Key value pairs of pull request labels and their descriptions used for note sections.
- `-ReleaseNoteSectionlessDescription` (`-rnsd`) : Default ("Undefined").
- `-ReleaseNoteUncategorisedDescription` (`-rnud`) : Default ("Unclassified").
- `-ReleaseNoteCategorised` (`-rnc`) : Default ("false"). Set to "true" to enable note categorisation.
- `-ReleaseNoteCategories` (`-rncl`) : Example ("CatA=Category A,catB=Category B"). Key value pairs of pull request labels and their descriptions used for note categorisation.
- `-ReleaseNoteCategoryPrefix` (`-rncp`) : Default ("#"). Used to differentiate category labels from section labels.
- `-ReleaseNoteOrderAscending` (`-rnoa`) : Default ("false"). Used to determine the sort order of the release notes.
- `-ReleaseNoteOrderWhen` (`-rnow`) : Default ("merged"). Set to "created" to order release notes based on pull request creation time rather than merge time.
- `-ReleaseNoteFormat` (`-rnf`) : Default ("{0} {1}"). Available fields are {0} pull request title, {1} pull request url, {2} pull request number, {3} pull request created date/time, {4} pull request merged date/time, {5} pull request author username, {6} pull request author URL, {7} pull request documentation URL
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
release-note-format: "{0} {1}"
release-note-date-format: "MMM dd, yyyy HH:mm"
release-note-sectioned: true
release-note-order-when: merged
release-note-sectionless-description: Undefined
release-note-sections:
  - bug=Fixes
  - enhancement=Enhancements
```
