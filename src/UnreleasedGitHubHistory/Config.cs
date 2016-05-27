using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using UnreleasedGitHubHistory.Models;
using YamlDotNet.Serialization;

namespace UnreleasedGitHubHistory
{
    public class Config
    {
        private readonly ProgramArgs _programArgs;
        private const string YamlSettingsFileName = "UnreleasedGitHubHistory.yml";

        public Config(ProgramArgs programArgs)
        {
            _programArgs = programArgs;
        }

        public bool MergeWithDefaults()
        {
            if (!DiscoverGitHubToken() || !DiscoverGitRepository())
                return false;
            var yamlSettingsFile = Path.Combine(_programArgs.LocalGitRepository.Info.WorkingDirectory, YamlSettingsFileName);
            if (File.Exists(yamlSettingsFile))
                using (var reader = File.OpenText(yamlSettingsFile))
                    MergeArgs((new Deserializer()).Deserialize<ProgramArgs>(reader));
            MergeDefaults();
            if (!DiscoverGitHubSettings())
                return false;
            DiscoverGitHead();
            return true;
        }

        private void MergeArgs(ProgramArgs args)
        {
            _programArgs.ConfluenceApiUrl = _programArgs.ConfluenceApiUrl ?? args.ConfluenceApiUrl;
            _programArgs.ConfluenceReleaseParentPageId = _programArgs.ConfluenceReleaseParentPageId ?? args.ConfluenceReleaseParentPageId;
            _programArgs.ConfluenceSpaceKey = _programArgs.ConfluenceSpaceKey ?? args.ConfluenceSpaceKey;
            _programArgs.ExcludeLabel = _programArgs.ExcludeLabel ?? args.ExcludeLabel;
            _programArgs.FollowLabel = _programArgs.FollowLabel ?? args.FollowLabel;
            _programArgs.GitHubOwner = _programArgs.GitHubOwner ?? args.GitHubOwner;
            _programArgs.GitHubRepository = _programArgs.GitHubRepository ?? args.GitHubRepository;
            _programArgs.GitRemote = _programArgs.GitRemote ?? args.GitRemote;
            _programArgs.GitRepositoryPath = _programArgs.GitRepositoryPath ?? args.GitRepositoryPath;
            _programArgs.GitVersion = _programArgs.GitVersion ?? args.GitVersion;
            _programArgs.OutputFileName = _programArgs.OutputFileName ?? args.OutputFileName;
            _programArgs.ReleaseBranchRef = _programArgs.ReleaseBranchRef ?? args.ReleaseBranchRef;
            _programArgs.ReleaseNoteCategoryPrefix = _programArgs.ReleaseNoteCategoryPrefix ?? args.ReleaseNoteCategoryPrefix;
            _programArgs.ReleaseNoteDateFormat = _programArgs.ReleaseNoteDateFormat ?? args.ReleaseNoteDateFormat;
            _programArgs.ReleaseNoteFormat = _programArgs.ReleaseNoteFormat ?? args.ReleaseNoteFormat;
            _programArgs.ReleaseNoteOrderWhen = _programArgs.ReleaseNoteOrderWhen ?? args.ReleaseNoteOrderWhen;
            _programArgs.ReleaseNoteSectionlessDescription = _programArgs.ReleaseNoteSectionlessDescription ?? args.ReleaseNoteSectionlessDescription;
            _programArgs.ReleaseNoteUncategorisedDescription = _programArgs.ReleaseNoteUncategorisedDescription ?? args.ReleaseNoteUncategorisedDescription;

            _programArgs.ReleaseNoteCategorised = _programArgs.ReleaseNoteCategorised ?? args.ReleaseNoteCategorised;
            _programArgs.ReleaseNoteOrderAscending = _programArgs.ReleaseNoteOrderAscending ?? args.ReleaseNoteOrderAscending;
            _programArgs.ReleaseNoteSectioned = _programArgs.ReleaseNoteSectioned ?? args.ReleaseNoteSectioned;
            _programArgs.ReleaseBranchHeadsOnly = _programArgs.ReleaseBranchHeadsOnly ?? args.ReleaseBranchHeadsOnly;

            _programArgs.AcceptInvalidCertificates = _programArgs.AcceptInvalidCertificates || args.AcceptInvalidCertificates;
            _programArgs.PublishToConfluence = _programArgs.PublishToConfluence || args.PublishToConfluence;
            _programArgs.PublishToFile = _programArgs.PublishToFile || args.PublishToFile;
            _programArgs.VerboseOutput = _programArgs.VerboseOutput || args.VerboseOutput;

            _programArgs.ReleaseNoteSections = _programArgs.ReleaseNoteSections ?? args.ReleaseNoteSections;
            _programArgs.ReleaseNoteCategories = _programArgs.ReleaseNoteCategories ?? args.ReleaseNoteCategories;
        }

        private void MergeDefaults()
        {
            _programArgs.ExcludeLabel = _programArgs.ExcludeLabel ?? "Exclude Note";
            _programArgs.FollowLabel = _programArgs.FollowLabel ?? "Follow Note";
            _programArgs.GitRemote = _programArgs.GitRemote ?? "origin";
            _programArgs.OutputFileName = _programArgs.OutputFileName ?? "Unreleased.md";
            _programArgs.ReleaseNoteCategoryPrefix = _programArgs.ReleaseNoteCategoryPrefix ?? "#";
            _programArgs.ReleaseNoteDateFormat = _programArgs.ReleaseNoteDateFormat ?? "MMM dd, yyyy HH:mm";
            _programArgs.ReleaseNoteFormat = _programArgs.ReleaseNoteFormat ?? "{0} {1}";
            _programArgs.ReleaseNoteOrderWhen = _programArgs.ReleaseNoteOrderWhen ?? "merged";
            _programArgs.ReleaseNoteSectionlessDescription = _programArgs.ReleaseNoteSectionlessDescription ?? "Undefined";
            _programArgs.ReleaseNoteUncategorisedDescription = _programArgs.ReleaseNoteUncategorisedDescription ?? "Unclassified";

            _programArgs.ReleaseNoteCategorised = _programArgs.ReleaseNoteCategorised ?? false;
            _programArgs.ReleaseNoteOrderAscending = _programArgs.ReleaseNoteOrderAscending ?? false;
            _programArgs.ReleaseNoteSectioned = _programArgs.ReleaseNoteSectioned ?? false;
            _programArgs.ReleaseBranchHeadsOnly = _programArgs.ReleaseBranchHeadsOnly ?? true;

            _programArgs.ReleaseNoteSections = _programArgs.ReleaseNoteSections ?? new List<string>() { "bug=Fixes", "enhancement=Enhancements" };
        }

        private bool DiscoverGitHubToken()
        {
            if (!string.IsNullOrWhiteSpace(_programArgs.GitHubToken))
                return true;
            if (_programArgs.VerboseOutput)
                Console.WriteLine("GitHubToken was not supplied. Trying UNRELEASED_HISTORY_GITHUB_TOKEN environment variable.");
            _programArgs.GitHubToken = Environment.GetEnvironmentVariable("UNRELEASED_HISTORY_GITHUB_TOKEN");
            if (!string.IsNullOrWhiteSpace(_programArgs.GitHubToken))
                return true;
            Console.WriteLine($"GitHubToken was not supplied and could not be found.");
            return false;
        }

        private bool DiscoverGitRepository()
        {
            if (string.IsNullOrWhiteSpace(_programArgs.GitRepositoryPath))
            {
                if (_programArgs.VerboseOutput)
                    Console.WriteLine($"GitRepositoryPath was not supplied. Trying to discover the Git repository from the current directory.");
                _programArgs.GitRepositoryPath = Directory.GetCurrentDirectory();
            }
            try
            {
                _programArgs.LocalGitRepository = new Repository(Repository.Discover(_programArgs.GitRepositoryPath));
            }
            catch (Exception ex) when(ex is ArgumentNullException || ex is ArgumentException || ex is RepositoryNotFoundException)
            {
                Console.WriteLine("GitRepositoryPath was not supplied or is invalid.");
                return false;
            }
            return true;
        }

        private void DiscoverGitHead()
        {
            if (!string.IsNullOrWhiteSpace(_programArgs.ReleaseBranchRef))
            {
                if (!_programArgs.ReleaseBranchRef.CaseInsensitiveContains("refs/heads") && !_programArgs.ReleaseBranchRef.CaseInsensitiveContains("/"))
                    _programArgs.ReleaseBranchRef = $"refs/heads/{_programArgs.ReleaseBranchRef}";
                return;
            }
                
            if (_programArgs.VerboseOutput)
                Console.WriteLine($"ReleaseBranchRef was not supplied. Using the current HEAD branch.");
            _programArgs.ReleaseBranchRef = _programArgs.LocalGitRepository.Head.CanonicalName;
        }

        private bool DiscoverGitHubSettings()
        {
            Remote remote = null;
            if (!string.IsNullOrWhiteSpace(_programArgs.GitHubOwner) && !string.IsNullOrWhiteSpace(_programArgs.GitHubRepository))
                return true;
            if (_programArgs.VerboseOutput)
                Console.WriteLine($"GitHubOwner and GitHubRepository were not supplied. Trying to discover it from remotes.");
            if (!_programArgs.LocalGitRepository.Network.Remotes.Any(r => r.Url.CaseInsensitiveContains("github.com")))
                return false;
            if (!string.IsNullOrWhiteSpace(_programArgs.GitRemote))
                remote = _programArgs.LocalGitRepository.Network.Remotes[_programArgs.GitRemote] ?? _programArgs.LocalGitRepository.Network.Remotes.First(r => r.Url.CaseInsensitiveContains("github.com"));
            // prefer origin and upstream
            if (remote == null)
                remote = _programArgs.LocalGitRepository.Network.Remotes
                    .Where(r => r.Name.CaseInsensitiveContains("origin") || r.Name.CaseInsensitiveContains("upstream"))
                    .OrderBy(r => r.Name).First();
            // fallback to any remaining one
            if (remote == null)
                remote = _programArgs.LocalGitRepository.Network.Remotes.First(r => r.Url.CaseInsensitiveContains("github.com"));
            if (remote == null)
            {
                Console.WriteLine($"GitHubOwner and GitHubRepository were not supplied and could not be discovered");
                return false;
            }
            var remoteUrl = new Uri(remote.Url);
            _programArgs.GitHubOwner = remoteUrl.Segments[1].Replace(@"/", string.Empty);
            _programArgs.GitHubRepository = remoteUrl.Segments[2].Replace(@".git", string.Empty);
            return true;
        }

        public void WriteSampleConfig()
        {
            var sampleConfigFile = Path.Combine(_programArgs.LocalGitRepository.Info.WorkingDirectory, YamlSettingsFileName);
            if (File.Exists(sampleConfigFile))
            {
                Console.WriteLine($"UnreleasedGitHubHistory.yml file already exists ...");
                return;
            }
            using (var writer = new StreamWriter(sampleConfigFile))
            {
                writer.WriteLine("# release-branch-heads-only: true");
                writer.WriteLine("# release-note-exclude: Exclude Note");
                writer.WriteLine("# release-note-follow: Follow Note");
                writer.WriteLine("# release-note-format: \"{0} {1}\"");
                writer.WriteLine("# release-note-date-format: \"MMM dd, yyyy HH:mm\"");
                writer.WriteLine("# release-note-sectioned: true");
                writer.WriteLine("# release-note-order-when: merged");
                writer.WriteLine("# release-note-sectionless-description: Undefined");
                writer.WriteLine("# release-note-sections:");
                writer.WriteLine("#   - bug=Fixes");
                writer.WriteLine("#   - enhancement=Enhancements");
            }
            Console.WriteLine($"Created a sample UnreleasedGitHubHistory.yml file ...");
        }
    }
}