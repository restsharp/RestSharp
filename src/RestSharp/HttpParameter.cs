namespace RestSharp;

/// <summary>
/// Representation of an HTTP parameter (QueryString or Form value)
/// </summary>
[PublicAPI]
public class HttpParameter {
    /// <summary>
    /// Creates a new instance of HttpParameter
    /// </summary>
    /// <param name="name">Header name</param>
    /// <param name="value">Header value</param>
    /// <param name="contentType">Parameter content type</param>
    public HttpParameter(string name, string? value, string? contentType = null) {
        Name        = name;
        ContentType = contentType;
        Value       = value ?? "";
    }

    /// <summary>
    /// Creates a new instance of HttpParameter with value conversion
    /// </summary>
    /// <param name="name">Header name</param>
    /// <param name="value">Header value, which has to implement ToString() properly</param>
    /// <param name="contentType">Parameter content type</param>
    public HttpParameter(string name, object? value, string? contentType = null) : this(name, value?.ToString(), contentType) { }

    public HttpParameter() { }

    /// <summary>
    /// Name of the parameter
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Value of the parameter
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Content-Type of the parameter
    /// </summary>
    public string? ContentType { get; set; }
}