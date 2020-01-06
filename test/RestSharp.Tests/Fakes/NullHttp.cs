namespace RestSharp.Tests.Fakes
{
    public class NullHttp : Http
    {
        public new HttpResponse Get() => new HttpResponse();
    }
}