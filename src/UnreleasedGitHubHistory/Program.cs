using System;
using System.Linq;
using System.Net;
using UnreleasedGitHubHistory.Models;
using UnreleasedGitHubHistory.Publishers;
using PowerArgs;

namespace UnreleasedGitHubHistory
{
    // FEATURE: find page on confluence based on partial name so we can have actual date in title
    // FEATURE: add support for other pull request services (TFS, GitLab, BitBucket/Stash)
    // FEATURE: publish notes to (TFS, GitLab, GitHub)
    // FEATURE: rename to GitPullRequestNotes?

    public static class Program
    {
        private static void Main(string[] args)
        {
            int exitCode;
            const int successExitCode = 0;
            const int failureExitCode = -1;
            ProgramArgs programArgs;

            if (!RetrieveProgramArgs(args, out programArgs))
                Environment.Exit(failureExitCode);

            if (programArgs.InitConfig)
            {
                new Config(programArgs).WriteSampleConfig();
                Console.WriteLine($"Created a sample UnreleasedGitHubHistory.yml file ...");
                Environment.Exit(successExitCode);
            }

            if (programArgs.HeadBranchRestrictionApplies())
            {
                Console.WriteLine($"Detected a non-head branch {programArgs.ReleaseBranchRef}. Aborting ...");
                Environment.Exit(successExitCode);
            }

            if (programArgs.AcceptInvalidCertificates)
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var releaseHistory = UnreleasedGitHubHistoryBuilder.BuildReleaseHistory(programArgs);

            if (releaseHistory == null)
            {
                exitCode = failureExitCode;
            }
            else if (!releaseHistory.Any())
            {
                exitCode = successExitCode;
            }
            else
            {
                exitCode = successExitCode;

                var buildVersion = BuildVersion(programArgs);
                var releaseHistoryMarkdown = ReleaseNoteFormatter.MarkdownNotes(releaseHistory, programArgs);
                var combinedMarkdown = $"# {ReleaseNoteFormatter.EscapeMarkdown(buildVersion)}\n{releaseHistoryMarkdown}";

                // always output markdown to stdout by default
                Console.WriteLine(combinedMarkdown);

                // optionally publish to file
                if (programArgs.PublishToFile)
                {
                    if (FilePublisher.PublishMarkdownReleaseHistoryFile(combinedMarkdown, programArgs))
                        exitCode = successExitCode;
                    else
                        exitCode = failureExitCode;
                }

                // optionally publish to Confluence
                if (exitCode == successExitCode && programArgs.PublishToConfluence)
                {
                    if (ConfluencePublisher.PublishMarkdownReleaseHistoryPage(buildVersion, releaseHistoryMarkdown, programArgs))
                        exitCode = successExitCode;
                    else
                        exitCode = failureExitCode;
                }
            }
            Environment.Exit(exitCode);
        }

        private static bool RetrieveProgramArgs(string[] args, out ProgramArgs programArgs)
        {
            try
            {
                programArgs = Args.Parse<ProgramArgs>(args);
            }
            catch (ArgException e)
            {
                Console.WriteLine($"Error: {e.Message}");
                Console.WriteLine(ArgUsage.GenerateUsageFromTemplate<ProgramArgs>());
                programArgs = null;
                return false;
            }
            return new Config(programArgs).MergeWithDefaults();
        }

        private static string BuildVersion(ProgramArgs programArgs)
        {
            if (string.IsNullOrWhiteSpace(programArgs.GitVersion))
                programArgs.GitVersion = Environment.GetEnvironmentVariable("GITVERSION_MAJORMINORPATCH");
            var versionText = !string.IsNullOrWhiteSpace(programArgs.GitVersion) ? programArgs.GitVersion : "Unreleased";
            return $"{versionText} ({programArgs.ReleaseBranchRef.Replace("refs/heads/", string.Empty).ToUpper()}) - XX XXX {DateTime.Now:yyyy}";
        }
    }
}

