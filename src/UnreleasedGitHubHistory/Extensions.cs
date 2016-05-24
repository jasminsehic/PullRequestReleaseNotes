using System;

namespace UnreleasedGitHubHistory
{
    public static class Extensions
    {
        public static bool CaseInsensitiveContains(this string target, string value)
        {
            return (target.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) >= 0);
        }
    }
}