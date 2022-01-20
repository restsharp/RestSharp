using HttpTracer.Logger;

namespace RestSharp.Tests.Integrated.Fixtures; 

public class OutputLogger  : ILogger{
    readonly ITestOutputHelper _output;

    public OutputLogger(ITestOutputHelper output) => _output = output;

    public void Log(string message) => _output.WriteLine(message);
}