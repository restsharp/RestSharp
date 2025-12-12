namespace RestSharp.Tests.Integrated.Fixtures;

static class CookieExtensions {
    public static Cookie? Find(this CookieCollection? cookieCollection, string name) {
        if (cookieCollection == null) return null;
        for (var i = 0; i < cookieCollection.Count; i++) {
            var cookie = cookieCollection[i];
            if (cookie.Name == name) return cookie;
        }

        return null;
    }
}