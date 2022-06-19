![Icon](https://raw.github.com/jasminsehic/PullRequestReleaseNotes/master/docs/img/logo.png)

Pull Request Release Notes
==========================

[![GitHub Workflow Status (branch)](https://img.shields.io/github/workflow/status/jasminsehic/pullrequestreleasenotes/CI/master)](https://github.com/jasminsehic/PullRequestReleaseNotes/actions/workflows/main.yml)
[![Release](https://img.shields.io/github/release/jasminsehic/PullRequestReleaseNotes.svg)](https://github.com/jasminsehic/PullRequestReleaseNotes/releases)
[![License](https://img.shields.io/github/license/jasminsehic/PullRequestReleaseNotes.svg)](https://github.com/jasminsehic/PullRequestReleaseNotes/blob/master/LICENSE)
[![Gitter](https://badges.gitter.im/jasminsehic/PullRequestReleaseNotes.svg)](https://gitter.im/jasminsehic/PullRequestReleaseNotes)

Pull Request Release Notes utility generates release notes for all merged pull requests, on a specific branch, that have not yet been released relying solely on pull request titles and labels to generate the release notes and publish them in markdown format.

Supported Pull Request providers are [GitHub](https://github.com/), [GitLab](https://gitlab.com/), [Azure DevOps Services / Server](https://azure.microsoft.com/en-au/services/devops/), [BitBucket Cloud](https://bitbucket.org/) and [Bitbucket Server](https://www.atlassian.com/software/bitbucket/download). 

Intention is to run this utility as part of a continuous integration process and generate notes automatically as part of every release branch build. Optionally the utility can also publish the notes to a markdown file, [Atlassian Confluence](https://www.atlassian.com/software/confluence) page or a [Slack](https://slack.com/) post. 

## Command line, YAML file parameters and Environment variables
See [HELP.md](https://github.com/jasminsehic/PullRequestReleaseNotes/blob/master/docs/HELP.md) for details on parameters.

## Release Notes Format

Utility outputs release notes following the [Semantic Release Notes](https://web.archive.org/web/20161013175123/http://www.semanticreleasenotes.org/) format and extracts semantic release note sections, categories and summaries from the pull request title and labels. For example all pull requests with `Bug` label can be grouped under `Fixes` section and pull requests with `Enhancement` label can be grouped under `Enhancements` section. Category grouping is possible through use of the `#Label` where `#` character is used to denote a category label as opposed to a section label. BitBucket Cloud / Server pull request providers do not have a label concept yet so for those providers you can type `[#section]` and `[##category]` either in the title or the description of the pull request as a pseudo-label.

Release note formatting can be further customised where you can turn off grouping by section and category, order the release notes based on merged or created time of pull request and the format of the release note itself. Version number can be supplied via [GitVersion](https://github.com/GitTools/GitVersion) tool. 

You can also define a label to exclude pull request from release notes. Also you can define a label that when not added to a pull request will add a release note highlighted as code. This can be useful for scenarios such as QA team adding a QC label to a pull request so then it is easy to spot which items haven't gone through QC.

See [HELP.md](https://github.com/jasminsehic/PullRequestReleaseNotes/blob/master/docs/HELP.md) for all the details on how perform these customisations. 

## Docker Image

You can run `jasminsehic/pullrequestreleasenotes` Linux Docker image on Windows WSL2, Linux or MacOS.
While inside the root of a working git directory run the Docker image using below command examples.
GitHubToken used in the example is just an example.

To run on Windows run this from Command Prompt:
```
docker run --rm -it -v "%cd%:/repo" jasminsehic/pullrequestreleasenotes:latest -grp /repo -GitHubToken c03b77a4982d48f0af328312a9b99455
```
or run this from PowerShell:
```
docker run --rm -it -v "${pwd}:/repo" jasminsehic/pullrequestreleasenotes:latest -grp /repo -GitHubToken c03b77a4982d48f0af328312a9b99455
```
To run on Linux or MacOS:
```
docker run --rm -it -v "$(pwd):/repo" jasminsehic/pullrequestreleasenotes:latest -grp /repo -GitHubToken c03b77a4982d48f0af328312a9b99455
```

## PullRequestReleaseNotes in action

#### Sample Markdown Output

```markdown
# 1.2.5 (MASTER) - XX XXX 2016
## Enhancements
### Category A
- Awesome new feature [\#1854](https://github.com/org/repo/pull/1854)

### Undefined
- Special feature for Acme Co [\#1855](https://github.com/org/repo/pull/1855)

## Fixes
### Category Z
- Fixed problem with widget [\#1792](https://github.com/org/repo/pull/1792)

### Category Y
- Fixed exception with view layout [\#1848](https://github.com/org/repo/pull/1848)

### Undefined
- Fixed spelling mistake [\#1833](https://github.com/org/repo/pull/1833)

## Unclassified
### Undefined
- Added new Category H [\#1843](https://github.com/org/repo/pull/1843)
```
#### Sample Actual Markdown

# 1.2.5 (MASTER) - XX XXX 2016
## Enhancements
### Category A
- Awesome new feature [\#1854](https://github.com/org/repo/pull/1854)

### Undefined
- Special feature for Acme Co [\#1855](https://github.com/org/repo/pull/1855)

## Fixes
### Category Z
- Fixed problem with widget [\#1792](https://github.com/org/repo/pull/1792)

### Category Y
- Fixed exception with view layout [\#1848](https://github.com/org/repo/pull/1848)

### Undefined
- Fixed spelling mistake [\#1833](https://github.com/org/repo/pull/1833)

## Unclassified
### Undefined
- Added new Category H [\#1843](https://github.com/org/repo/pull/1843)

#### Sample GitHub Input
![GITHUB](https://raw.github.com/jasminsehic/PullRequestReleaseNotes/master/docs/img/github.png)

#### Other pull request provider samples
[GitLab](https://raw.github.com/jasminsehic/PullRequestReleaseNotes/master/docs/img/gitlab.png)

[BitBucket Cloud](https://raw.github.com/jasminsehic/PullRequestReleaseNotes/master/docs/img/bitbucket_cloud.png)

[BitBucket Server](https://raw.github.com/jasminsehic/PullRequestReleaseNotes/master/docs/img/bitbucket_server.png)

[Azure DevOps Services & Server](https://raw.github.com/jasminsehic/PullRequestReleaseNotes/master/docs/img/tfs.png)

#### Sample Confluence Output
![CONFLUENCE](https://raw.github.com/jasminsehic/PullRequestReleaseNotes/master/docs/img/confluence.png)

#### Sample Slack Output
![SLACK](https://raw.github.com/jasminsehic/PullRequestReleaseNotes/master/docs/img/slack.png)

## Thanks
Big thanks to [Jake Ginnivan](http://jake.ginnivan.net/) for inspiring this tool with his work on [GitReleaseNotes](https://github.com/GitTools/GitReleaseNotes) and [GitVersion](https://github.com/GitTools/GitVersion)

Also many thanks to:
- [Edward Thomson](https://github.com/ethomson) for [Infinity.NET](https://github.com/ethomson/infinity.net) that made it super-easy to connect to TFS. 
- [Adam Abdelhamed](https://github.com/adamabdelhamed) for [PowerArgs](https://github.com/adamabdelhamed/PowerArgs) that made command line argument parsing so easy it hurts.
- [Mitja Bezenšek](https://github.com/MitjaBezensek) for [SharpBucket](https://github.com/MitjaBezensek/SharpBucket) that provided a pain-free way to connect to BitBucket.
- [Antoine Aubry](https://github.com/aaubry) for [YamlDotNet](https://github.com/aaubry/YamlDotNet) which made use of YAML format for the application configuration file as trivial as a yawn.
- Anthony van der Hoorn & Nik Molnar for creating [Semantic Release Notes](https://web.archive.org/web/20161013175123/http://www.semanticreleasenotes.org/)
- All contributors of [RestSharp](https://github.com/restsharp/RestSharp) which is a swiss-army knife of REST clients and life without it would be meaningless. (bows)
- All contributors of [LibGit2Sharp](https://github.com/libgit2/libgit2sharp) which made it possible to traverse a Git repo history to find all unreleased commits
- [JetBrains](https://www.jetbrains.com/?from=PullRequestReleaseNotes) for supporting this open source project by donating a free ReSharper license 
<img src="./docs/img/jetbrains.svg">
