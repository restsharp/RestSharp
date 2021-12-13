using System.Runtime.CompilerServices;

namespace RestSharp;

static class Ensure {
    public static T NotNull<T>(T? value, string name)
        => value ?? throw new ArgumentNullException(name);

    public static string NotEmpty(string? value, string name)
        => string.IsNullOrWhiteSpace(value) ? throw new ArgumentNullException(name) : value!;

    public static string NotEmptyString(object? value, string name) {
        var s = value as string;
        return string.IsNullOrWhiteSpace(s) ? throw new ArgumentNullException(name) : s!;
    }
}