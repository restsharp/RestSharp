using System;

namespace RestSharp.Authenticators.OAuth.Extensions
{
    internal static class TimeExtensions
    {
        public static long ToUnixTime(this DateTime dateTime)
        {
            var timeSpan  = dateTime - new DateTime(1970, 1, 1);
            var timestamp = (long) timeSpan.TotalSeconds;

            return timestamp;
        }
    }
}