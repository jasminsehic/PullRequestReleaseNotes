using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using PullRequestReleaseNotes.Models;
using PullRequestReleaseNotes.Publishers;

namespace PullRequestReleaseNotes
{
    public static class Program
    {
        private static ProgramArgs _programArgs;
        private const int SuccessExitCode = 0;
        private const int FailureExitCode = -1;

        private static void Main(string[] args)
        {
            var exitCode = SuccessExitCode;
            _programArgs = ValidateConfiguration(args);
            if (_programArgs.AcceptInvalidCertificates)
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            var releaseHistory = new PullRequestHistoryBuilder(_programArgs).BuildHistory();
            if (releaseHistory == null)
                exitCode = FailureExitCode;
            else if (!releaseHistory.Any())
                exitCode = SuccessExitCode;
            else
                exitCode = BuildAndPublish(releaseHistory, exitCode);
            Environment.Exit(exitCode);
        }

        private static int BuildAndPublish(List<PullRequestDto> releaseHistory, int exitCode)
        {
            var buildVersion = BuildVersion();
            var semanticReleaseNotes = new SemanticReleaseNotesBuilder(releaseHistory, buildVersion, _programArgs).Build();
            var releaseHistoryMarkdown = new MarkdownFormatter(_programArgs).Format(semanticReleaseNotes);
            var combinedMarkdown = $"# {MarkdownFormatter.EscapeMarkdown(buildVersion)}{releaseHistoryMarkdown}";
            // always output markdown to stdout by default
            Console.WriteLine(combinedMarkdown);
            return Publish(combinedMarkdown, buildVersion, releaseHistoryMarkdown, exitCode);
        }

        private static int Publish(string combinedMarkdown, string buildVersion, string releaseHistoryMarkdown, int exitCode)
        {
            exitCode = PublishFile(combinedMarkdown, exitCode);
            exitCode = PublishConfluence(buildVersion, releaseHistoryMarkdown, exitCode);
            exitCode = PublishSlack(buildVersion, releaseHistoryMarkdown, exitCode);
            return exitCode;
        }

        private static int PublishSlack(string buildVersion, string releaseHistoryMarkdown, int exitCode)
        {
            if (!_programArgs.PublishToSlack)
                return exitCode;
            if (SlackPublisher.PublishPost(buildVersion, releaseHistoryMarkdown, _programArgs))
                exitCode = SuccessExitCode;
            else
                exitCode = FailureExitCode;
            return exitCode;
        }

        private static int PublishConfluence(string buildVersion, string releaseHistoryMarkdown, int exitCode)
        {
            if (!_programArgs.PublishToConfluence)
                return exitCode;
            if (ConfluencePublisher.PublishMarkdownPage(buildVersion, releaseHistoryMarkdown, _programArgs))
                exitCode = SuccessExitCode;
            else
                exitCode = FailureExitCode;
            return exitCode;
        }

        private static int PublishFile(string combinedMarkdown, int exitCode)
        {
            if (!_programArgs.PublishToFile)
                return exitCode;
            if (FilePublisher.PublishMarkdownReleaseHistoryFile(combinedMarkdown, _programArgs))
                exitCode = SuccessExitCode;
            else
                exitCode = FailureExitCode;
            return exitCode;
        }

        private static ProgramArgs ValidateConfiguration(string[] args)
        {
            ProgramArgs programArgs;
            if (!Config.GetCommandLineInput(args, out programArgs))
                Environment.Exit(FailureExitCode);

            if (programArgs.InitConfig)
            {
                new Config(programArgs).WriteSampleConfig();
                Environment.Exit(SuccessExitCode);
            }

            if (programArgs.HeadBranchRestrictionApplies())
            {
                Console.WriteLine($"Detected a non-head branch {programArgs.ReleaseBranchRef}. Aborting ...");
                Environment.Exit(SuccessExitCode);
            }
            return programArgs;
        }

        private static string BuildVersion()
        {
            var versionText = !string.IsNullOrWhiteSpace(_programArgs.GitVersion) ? _programArgs.GitVersion : "Unreleased";
            return $"{versionText} ({_programArgs.ReleaseBranchRef.Replace("refs/heads/", string.Empty).ToUpper()}) - XX XXX {DateTime.Now:yyyy}";
        }
    }
}

