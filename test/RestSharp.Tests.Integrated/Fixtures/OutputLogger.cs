using HttpTracer.Logger;

namespace RestSharp.Tests.Integrated.Fixtures; 

public class OutputLogger(ITestOutputHelper output) : ILogger {
    public void Log(string message) => output.WriteLine(message);
}