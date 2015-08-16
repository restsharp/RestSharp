using System;

namespace RestSharp.Authenticators.OAuth.Extensions
{
    internal static class TimeExtensions
    {
        public static DateTime FromNow(this TimeSpan value)
        {
            return new DateTime((DateTime.Now + value).Ticks);
        }

        public static DateTime FromUnixTime(this long seconds)
        {
            DateTime time = new DateTime(1970, 1, 1);

            time = time.AddSeconds(seconds);

            return time.ToLocalTime();
        }

        public static long ToUnixTime(this DateTime dateTime)
        {
            TimeSpan timeSpan = (dateTime - new DateTime(1970, 1, 1));
            long timestamp = (long) timeSpan.TotalSeconds;

            return timestamp;
        }
    }
}
