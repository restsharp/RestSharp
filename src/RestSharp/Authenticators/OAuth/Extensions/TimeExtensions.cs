namespace RestSharp.Authenticators.OAuth.Extensions; 

static class TimeExtensions {
    public static long ToUnixTime(this DateTime dateTime) {
        var timeSpan = dateTime - new DateTime(1970, 1, 1);
        return (long)timeSpan.TotalSeconds;
    }
}