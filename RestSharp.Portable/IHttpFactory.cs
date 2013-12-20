using System;
namespace RestSharp
{
	public interface IHttpFactory
	{
		IHttp Create();
	}

	public class SimpleHttpFactory<T> : IHttpFactory where T : IHttp, new()
	{
		public IHttp Create()
		{
			return new T();
            //return (T)Activator.CreateInstance(typeof(T), request);
		}
	}

    public interface IMessageHandlerFactory
    {
        IMessageHandler Create();
    }

    public class SimpleMessageHandlerFactory<T> : IMessageHandlerFactory where T : IMessageHandler, new()
	{
		public IMessageHandler Create()
		{
			return new T();
		}
	}
}
