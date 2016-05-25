using System;
using System.Linq;
using System.Net;
using UnreleasedGitHubHistory.Models;
using UnreleasedGitHubHistory.Publishers;
using PowerArgs;

namespace UnreleasedGitHubHistory
{
    // FEATURE: have a yml settings option so that we can version parameters
    // FEATURE: find page on confluence based on partial name so we can have actual date in title
    // FEATURE: add note format template so things like dates, contributors, pull request properties can be controlled to customise the note format
    // FEATURE: add support for other pull request services (TFS, GitLab, BitBucket/Stash)
    // FEATURE: publish notes to (TFS, GitLab, GitHub)
    // FEATURE: rename to GitPullRequestNotes

    public static class Program
    {
        private static void Main(string[] args)
        {
            ProgramArgs programArgs = null;
            const int successExitCode = 0;
            const int failureExitCode = -1;
            var exitCode = failureExitCode;

            try
            {
                programArgs = Args.Parse<ProgramArgs>(args);
            }
            catch (ArgException e)
            {
                Console.WriteLine($"Error: {e.Message}");
                Console.WriteLine(ArgUsage.GenerateUsageFromTemplate<ProgramArgs>());
                Environment.Exit(exitCode);
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

        private static string BuildVersion(ProgramArgs programArgs)
        {
            if (string.IsNullOrWhiteSpace(programArgs.GitVersion))
                programArgs.GitVersion = Environment.GetEnvironmentVariable("GITVERSION_MAJORMINORPATCH");
            var versionText = !string.IsNullOrWhiteSpace(programArgs.GitVersion) ? programArgs.GitVersion : "Unreleased";
            return $"{versionText} ({programArgs.ReleaseBranchRef.Replace("refs/heads/", string.Empty).ToUpper()}) - XX XXX {DateTime.Now:yyyy}";
        }
    }
}

