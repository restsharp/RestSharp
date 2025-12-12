namespace RestSharp.Tests.Integrated.HttpTracer;

public interface IHttpTracerLogger {
    void Log(string message);
}

public class OutputHttpTracerLogger(ITestOutputHelper output) : IHttpTracerLogger {
    public void Log(string message) => output.WriteLine(message);
}
