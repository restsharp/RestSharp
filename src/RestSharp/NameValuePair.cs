namespace RestSharp;

[PublicAPI]
public class NameValuePair {
    public static NameValuePair Empty = new(null, null);

    public NameValuePair(string? name, string? value) {
        Name  = name;
        Value = value;
    }

    public string? Name  { get; }
    public string? Value { get; }

    public bool IsEmpty => Name == null;
}