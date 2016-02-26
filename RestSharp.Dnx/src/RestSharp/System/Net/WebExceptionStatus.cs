#if DNXCORE50
namespace System.Net
{
    /// <summary>
    /// Defines status codes for the <see cref="T:System.Net.WebException"/> class.
    /// </summary>
    public enum WebExceptionStatus
    {
        Success,
        NameResolutionFailure,
        ConnectFailure,
        ReceiveFailure,
        SendFailure,
        PipelineFailure,
        RequestCanceled,
        ProtocolError,
        ConnectionClosed,
        TrustFailure,
        SecureChannelFailure,
        ServerProtocolViolation,
        KeepAliveFailure,
        Pending,
        Timeout,
        ProxyNameResolutionFailure,
        UnknownError,
        MessageLengthLimitExceeded,
        CacheEntryNotFound,
        RequestProhibitedByCachePolicy,
        RequestProhibitedByProxy,
    }
}
#endif