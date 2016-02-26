#if DNXCORE50
namespace System.Net
{
    public abstract class WebResponse
    {
        public virtual void Close()
        {
            throw new NotImplementedException();
        }
    }
}
#endif