namespace RestSharp
{
	public interface IHttpFactory
	{
		IHttp Create();
	}

	public class SimpleFactory<T> : IHttpFactory where T : IHttp, new()
	{
		public IHttp Create()
		{
			return new T();
		}
	}
}
