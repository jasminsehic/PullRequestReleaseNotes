using System;
using System.IO;
using PullRequestReleaseNotes.Models;

namespace PullRequestReleaseNotes.Publishers
{
    public static class FilePublisher
    {
        public static bool PublishMarkdownReleaseHistoryFile(string releaseHistoryMarkdown, ProgramArgs programArgs)
        {
            try
            {
                File.WriteAllText(programArgs.OutputFileName, releaseHistoryMarkdown);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to save release notes to file: {programArgs.OutputFileName}. Exception: {e.Message}");
                return false;
            }
            return true;
        }
    }
}