namespace RestSharp;

/// <summary>
/// Types of parameters that can be added to requests
/// </summary>
public enum ParameterType {
    /// <summary>
    /// Cookie parameter
    /// </summary>
    Cookie, GetOrPost, UrlSegment, HttpHeader, RequestBody, QueryString, QueryStringWithoutEncode
}

/// <summary>
/// Data formats
/// </summary>
public enum DataFormat { Json, Xml, None }

/// <summary>
/// HTTP method to use when making requests
/// </summary>
public enum Method {
    Get, Post, Put, Delete, Head, Options,
    Patch, Merge, Copy
}

/// <summary>
/// Format strings for commonly-used date formats
/// </summary>
public struct DateFormat {
    /// <summary>
    /// .NET format string for ISO 8601 date format
    /// </summary>
    public const string ISO_8601 = "s";

    /// <summary>
    /// .NET format string for roundtrip date format
    /// </summary>
    public const string ROUND_TRIP = "u";
}

/// <summary>
/// Status for responses (surprised?)
/// </summary>
public enum ResponseStatus { None, Completed, Error, TimedOut, Aborted }