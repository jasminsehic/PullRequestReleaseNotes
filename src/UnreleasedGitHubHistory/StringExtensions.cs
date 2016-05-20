using System;

namespace UnreleasedGitHubHistory
{
    public static class StringExtensions
    {
        public static bool CaseInsensitiveContains(this string target, string value)
        {
            return (target.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) >= 0);
        }
    }
}