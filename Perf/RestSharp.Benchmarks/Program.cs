using BenchmarkDotNet.Running;

namespace RestSharp.Benchmarks
{
    class Program
    {
        public static void Main(string[] args) => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
