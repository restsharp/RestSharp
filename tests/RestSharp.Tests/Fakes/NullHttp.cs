namespace RestSharp.Tests.Fakes
{
    public class NullHttp : Http
    {
        public HttpResponse Get()
        {
            return new HttpResponse();
        }
    }
}
