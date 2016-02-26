#if DNXCORE50
namespace System.Net
{
    public abstract class WebRequest
    {
        public virtual void Abort()
        {
            throw new NotImplementedException();
        }
    }
}
#endif