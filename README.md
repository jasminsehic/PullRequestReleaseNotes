![Icon](https://raw.github.com/jasminsehic/PullRequestReleaseNotes/master/logo.png)

PullRequestReleaseNotes
=======================

[![Build status](https://ci.appveyor.com/api/projects/status/github/jasminsehic/pullrequestreleasenotes?svg=true)](https://ci.appveyor.com/project/jasminsehic/unreleasedgithubhistory)
[![Chocolatey](https://img.shields.io/chocolatey/vpre/PullRequestReleaseNotes.svg)](https://chocolatey.org/packages/PullRequestReleaseNotes)
[![Release](https://img.shields.io/github/release/jasminsehic/PullRequestReleaseNotes.svg)]()
[![License](https://img.shields.io/github/license/jasminsehic/PullRequestReleaseNotes.svg)]()
[![Gitter](https://badges.gitter.im/jasminsehic/PullRequestReleaseNotes.svg)](https://gitter.im/jasminsehic/PullRequestReleaseNotes?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

Pull Request Release Notes utility generates release notes for all merged pull requests, on a specific branch, that have not yet been released replying only on pull request titles and labels to generate the release notes. 

Supported Pull Request providers are [GitHub](https://github.com/), [GitLab](https://gitlab.com/), [TFS / Team Services](https://www.visualstudio.com/en-us/products/visual-studio-team-services-vs.aspx), [BitBucket Cloud](https://bitbucket.org/) and [Bitbucket Server](https://www.atlassian.com/software/bitbucket/download). 

Intention is to run this utility as part of a continuous integration process and generate notes automatically as part of every release branch build. Optionally the utility can also publish the notes to a markdown file, [Atlassian Confluence](https://www.atlassian.com/software/confluence) page or a [Slack](https://slack.com/) post. 

Utility outputs release notes following the [Semantic Release Notes](http://www.semanticreleasenotes.org/) format and extracts semantic release note sections, categories and summaries from the pull request title and labels. For example all pull requests with `Bug` label can be grouped under `Fixes` section and pull requests with `Enhancement` label can be grouped under `Enhancements` section. Category grouping is possible through use of the `#Label` where `#` character is used to denote a category label as opposed to a section label.

NOTE: TFS / Team Services and BitBucket Cloud / Server pull request providers do not have a label concept yet so for those providers you can type `[#section]` and `[##category]` either in the title or the description of the pull request as a pseudo-label.

## Install

    choco install PullRequestReleaseNotes
    
## Usage

While inside a git working directory run the application

    $ PullRequestReleaseNotes

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

## Command line and YAML file parameters
See [HELP.md](https://github.com/jasminsehic/PullRequestReleaseNotes/master/HELP.md) for details on parameters.

## Thanks
Big thanks to [Jake Ginnivan](http://jake.ginnivan.net/) for inspiring this tool with his work on [GitReleaseNotes](https://github.com/GitTools/GitReleaseNotes) and [GitVersion](https://github.com/GitTools/GitVersion)

Also many thanks to:
- [Edward Thomson](https://github.com/ethomson) for [Infinity.NET](https://github.com/ethomson/infinity.net) that made it super-easy to connect to TFS. 
- [Adam Abdelhamed](https://github.com/adamabdelhamed) for [PowerArgs](https://github.com/adamabdelhamed/PowerArgs) that made command line argument parsing so easy it hurts.
- [Mitja Bezenšek](https://github.com/MitjaBezensek) for [SharpBucket](https://github.com/MitjaBezensek/SharpBucket) that provided a pain-free way to connect to BitBucket.
- [Antoine Aubry](https://github.com/aaubry) for [YamlDotNet](https://github.com/aaubry/YamlDotNet) which made use of YAML format for the application configration file as trivial as a yawn
- [Simon Cropp](https://github.com/SimonCropp) for [PepitaPackage](https://github.com/SimonCropp/Pepita) which made NuGet packages for me before I even realised what was happening.
- Anthony van der Hoorn & Nik Molnar for creating [Semantic Release Notes](http://www.semanticreleasenotes.org/)
- All contributors of [RestSharp](https://github.com/restsharp/RestSharp) which is a swiss-army knife of REST clients and life without it would be meaningless. (bows)
- All contributors of [LibGit2Sharp](https://github.com/libgit2/libgit2sharp) which made it possible to traverse a Git repo history to find all unreleased commits
