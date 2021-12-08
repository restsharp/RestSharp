namespace RestSharp.Extensions;

static class WithExtensions {
    internal static T With<T>(this T self, Action<T> @do) {
        @do(self);
        return self;
    }
}