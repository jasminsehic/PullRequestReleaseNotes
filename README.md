![Icon](https://raw.github.com/jasminsehic/PullRequestReleaseNotes/master/docs/img/logo.png)

PullRequestReleaseNotes
=======================

[![Build status](https://ci.appveyor.com/api/projects/status/github/jasminsehic/pullrequestreleasenotes?svg=true)](https://ci.appveyor.com/project/jasminsehic/unreleasedgithubhistory)
[![Chocolatey](https://img.shields.io/chocolatey/vpre/PullRequestReleaseNotes.svg)](https://chocolatey.org/packages/PullRequestReleaseNotes)
[![Nuget](https://img.shields.io/nuget/v/PullRequestReleaseNotes.DotNetCore.svg)](https://www.nuget.org/packages/PullRequestReleaseNotes.DotNetCore)
[![Release](https://img.shields.io/github/release/jasminsehic/PullRequestReleaseNotes.svg)](https://github.com/jasminsehic/PullRequestReleaseNotes/releases)
[![License](https://img.shields.io/github/license/jasminsehic/PullRequestReleaseNotes.svg)](https://github.com/jasminsehic/PullRequestReleaseNotes/blob/master/LICENSE)
[![Gitter](https://badges.gitter.im/jasminsehic/PullRequestReleaseNotes.svg)](https://gitter.im/jasminsehic/PullRequestReleaseNotes?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

Pull Request Release Notes utility generates release notes for all merged pull requests, on a specific branch, that have not yet been released relying solely on pull request titles and labels to generate the release notes. 

Supported Pull Request providers are [GitHub](https://github.com/), [GitLab](https://gitlab.com/), [Azure DevOps Services / Server](https://azure.microsoft.com/en-au/services/devops/), [BitBucket Cloud](https://bitbucket.org/) and [Bitbucket Server](https://www.atlassian.com/software/bitbucket/download). 

Intention is to run this utility as part of a continuous integration process and generate notes automatically as part of every release branch build. Optionally the utility can also publish the notes to a markdown file, [Atlassian Confluence](https://www.atlassian.com/software/confluence) page or a [Slack](https://slack.com/) post. 

Utility outputs release notes following the [Semantic Release Notes](https://web.archive.org/web/20161013175123/http://www.semanticreleasenotes.org/) format and extracts semantic release note sections, categories and summaries from the pull request title and labels. For example all pull requests with `Bug` label can be grouped under `Fixes` section and pull requests with `Enhancement` label can be grouped under `Enhancements` section. Category grouping is possible through use of the `#Label` where `#` character is used to denote a category label as opposed to a section label. Azure DevOps Service / Server and BitBucket Cloud / Server pull request providers do not have a label concept yet so for those providers you can type `[#section]` and `[##category]` either in the title or the description of the pull request as a pseudo-label.

Release note formatting can be further customised where you can turn off grouping by section and category, order the release notes based on merged or created time of pull request and the format of the release note itself. Version number can be supplied via [GitVersion](https://github.com/GitTools/GitVersion) tool. 

You can also define a label to exclude pull request from release notes. Also you can define a label that when not added to a pull request will add a release note highlighted as code. This can be useful for scenarios such as QA team adding a QC label to a pull request so then it is easy to spot which items haven't gone through QC.

See [HELP.md](https://github.com/jasminsehic/PullRequestReleaseNotes/blob/master/docs/HELP.md) for all the details on how perform these customisations. 

## .NET Core Global Tool Install

    dotnet tool install -g PullRequestReleaseNotes.DotNetCore
	
## .NET Core Global Tool Usage

While inside a git working directory run the application

    $ dotnet-pullrequestreleasenotes

### Linux note

Only tested on Ubuntu 18.04 (Bionic). You may need to run `sudo apt-get install libgit2-dev` and `sudo ln -s /usr/lib/x86_64-linux-gnu/libgit2.so /usr/lib/x86_64-linux-gnu/libgit2-572e4d8.so` to ensure libgit2 library can be found by the app. This is expected to be resolved in a future version of LibGit2Sharp.

## Chocolatey Install

    choco install PullRequestReleaseNotes
	
## Chocolatey Usage

While inside a git working directory run the application

    $ PullRequestReleaseNotes

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

## Command line and YAML file parameters
See [HELP.md](https://github.com/jasminsehic/PullRequestReleaseNotes/blob/master/docs/HELP.md) for details on parameters.

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
