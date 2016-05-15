using System;
using System.Net;
using UnreleasedGitHubHistory.Models;
using UnreleasedGitHubHistory.Publishers;
using PowerArgs;

namespace UnreleasedGitHubHistory
{
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
            else if (releaseHistory.Count == 0)
            {
                exitCode = successExitCode;
            }
            else
            {
                exitCode = successExitCode;
                var releaseHistoryMarkdown = ReleaseNoteFormatter.Markdown(releaseHistory, programArgs);

                // always output markdown to stdout by default
                Console.WriteLine(releaseHistoryMarkdown);

                // optionally publish to file
                if (programArgs.PublishToFile)
                {
                    if (FilePublisher.PublishMarkdownReleaseHistoryFile(releaseHistoryMarkdown, programArgs))
                        exitCode = successExitCode;
                    else
                        exitCode = failureExitCode;
                }

                // optionally publish to Confluence
                if (exitCode == successExitCode && programArgs.PublishToConfluence)
                {
                    if (ConfluencePublisher.PublishMarkdownReleaseHistoryPage(releaseHistoryMarkdown, programArgs))
                        exitCode = successExitCode;
                    else
                        exitCode = failureExitCode;
                }
            }
            Environment.Exit(exitCode);
        }
    }
}

