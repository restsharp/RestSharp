using System.Collections.Specialized;
using System.Net;

namespace RestSharp.Tests.Integrated.Fixtures; 

public class CaptureFixture
{
    public CaptureFixture() => RequestHeadCapturer.Initialize();

    protected class RequestHeadCapturer
    {
        public const string Resource = "Capture";

        public static NameValueCollection? CapturedHeaders { get; set; }

        public static void Initialize() => CapturedHeaders = null;

        public static void Capture(HttpListenerContext context)
        {
            var request = context.Request;

            CapturedHeaders = request.Headers;
        }
    }
        
}