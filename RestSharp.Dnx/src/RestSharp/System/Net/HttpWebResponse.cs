#if DNXCORE50
namespace System.Net
{
    public class HttpWebResponse : WebResponse
    {
        public override void Close()
        {
        }
    }
}
#endif