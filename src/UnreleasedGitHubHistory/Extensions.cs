using System;

namespace UnreleasedGitHubHistory
{
    public static class Extensions
    {
        private const string InvalidUnixEpochErrorMessage = "Unix epoc starts January 1st, 1970";

        private static DateTime m_epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime FromTimestamp(this long timestamp)
        {
            return m_epoch.AddMilliseconds(timestamp);
        }

        public static long ToTimestamp(this DateTime dateTime)
        {
            if (dateTime == DateTime.MinValue)
                return 0;
            var delta = dateTime.ToUniversalTime() - m_epoch;
            if (delta.TotalMilliseconds < 0)
                throw new ArgumentOutOfRangeException(InvalidUnixEpochErrorMessage);
            return (long)delta.TotalMilliseconds;
        }

        public static bool CaseInsensitiveContains(this string target, string value)
        {
            return (target.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) >= 0);
        }
    }
}