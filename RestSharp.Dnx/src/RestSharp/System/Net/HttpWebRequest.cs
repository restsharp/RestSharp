#if DNXCORE50
namespace System.Net
{
    public class HttpWebRequest : WebRequest
    {
        public override void Abort()
        {
        }
    }
}
#endif