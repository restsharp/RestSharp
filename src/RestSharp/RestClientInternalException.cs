using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RestSharp;

/// <summary>
/// This exception SHOULD only be used for catching and throwing internal
/// exceptions within RestSharp.
/// </summary>
[Serializable]
public class RestClientInternalException : Exception {
    public RestClientInternalException() {
    }

    public RestClientInternalException(string? message, Exception? innerException) : base(message, innerException) {
    }

    protected RestClientInternalException(SerializationInfo info, StreamingContext context) : base(info, context) {
    }
}
