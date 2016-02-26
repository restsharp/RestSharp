#if DNXCORE50
namespace System.Net
{
    public class WebException : InvalidOperationException
    {
        public WebExceptionStatus Status { get; } = WebExceptionStatus.UnknownError;

        public WebException()
        {
        }

        public WebException(string message) : this(message, null)
        {
        }

        public WebException(string message, Exception innerException) :
            base(message, innerException)
        {
        }

        public WebException(string message, WebExceptionStatus status) :
           base(message, null)
        {
            Status = status;
        }
    }
}
#endif