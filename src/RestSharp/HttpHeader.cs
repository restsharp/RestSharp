namespace RestSharp; 

/// <summary>
/// Representation of an HTTP header
/// </summary>
[PublicAPI]
public class HttpHeader {
    /// <summary>
    /// Creates a new instance of HttpHeader
    /// </summary>
    /// <param name="name">Header name</param>
    /// <param name="value">Header value</param>
    public HttpHeader(string name, string? value) {
        Name  = name;
        Value = value ?? "";
    }

    /// <summary>
    /// Creates a new instance of HttpHeader with value conversion
    /// </summary>
    /// <param name="name">Header name</param>
    /// <param name="value">Header value, which has to implement ToString() properly</param>
    public HttpHeader(string name, object? value) : this(name, value?.ToString()) { }

    /// <summary>
    /// Creates a new instance of HttpHeader. Remember to assign properties!
    /// </summary>
    public HttpHeader() { }

    /// <summary>
    /// Name of the header
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Value of the header
    /// </summary>
    public string Value { get; set; }
}