#if NET48
using System.Runtime.CompilerServices;
using RestSharp.Serializers;
using RestSharp.Serializers.NewtonsoftJson;

namespace RestSharp.Tests.Serializers.Json;

public class SerializerAssemblyLoadingTests {
    [Fact]
    public void Defaults_do_not_load_json_libraries() {
        var loaded = RunIsolated(probe => probe.UseDefaults());

        loaded.Should().NotContain("System.Text.Json").And.NotContain("Newtonsoft.Json");
    }

    [Fact]
    public void Xml_only_does_not_load_json_libraries() {
        var loaded = RunIsolated(probe => probe.UseXmlOnly());

        loaded.Should().NotContain("System.Text.Json").And.NotContain("Newtonsoft.Json");
    }

    [Fact]
    public void Default_json_loads_only_system_text_json() {
        var loaded = RunIsolated(probe => probe.UseDefaultJson());

        loaded.Should().Contain("System.Text.Json").And.NotContain("Newtonsoft.Json");
    }

    [Fact]
    public void Newtonsoft_loads_only_newtonsoft_json() {
        var loaded = RunIsolated(probe => probe.UseNewtonsoft());

        loaded.Should().Contain("Newtonsoft.Json").And.NotContain("System.Text.Json");
    }

    [Fact]
    public void Newtonsoft_only_loads_only_newtonsoft_json() {
        var loaded = RunIsolated(probe => probe.UseNewtonsoftOnly());

        loaded.Should().Contain("Newtonsoft.Json").And.NotContain("System.Text.Json");
    }

    static string[] RunIsolated(Action<IsolatedSerializerConfigurationHelper> test) {
        var probeType = typeof(IsolatedSerializerConfigurationHelper);
        var setup = new AppDomainSetup {
            // CodeBase points to the original output directory even when xUnit shadow-copies the assembly.
            ApplicationBase    = Path.GetDirectoryName(new Uri(probeType.Assembly.CodeBase).LocalPath),
            ConfigurationFile  = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile,
            LoaderOptimization = LoaderOptimization.SingleDomain
        };
        var domain = AppDomain.CreateDomain($"Serializer loading {Guid.NewGuid():N}", null, setup);

        try {
            var probe = (IsolatedSerializerConfigurationHelper)domain.CreateInstanceAndUnwrap(probeType.Assembly.FullName, probeType.FullName);
            probe.GetLoadedAssemblies().Should().NotContain("System.Text.Json").And.NotContain("Newtonsoft.Json");

            test(probe);

            return probe.GetLoadedAssemblies();
        }
        finally {
            AppDomain.Unload(domain);
        }
    }

    /// <summary>
    /// Exercises serializer configurations in a separate AppDomain and reports its loaded assemblies.
    /// </summary>
    /// <remarks>
    /// Inherits from MarshalByRefObject so calls execute in the isolated domain.
    /// Avoid test framework calls here, as they could load unrelated dependencies and affect the assertions.
    /// </remarks>
    public class IsolatedSerializerConfigurationHelper : MarshalByRefObject {
        public string[] GetLoadedAssemblies() => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetName().Name).ToArray();

        public void UseDefaults() {
            using var client = new RestClient();
            RoundtripXml(client);
        }

        public void UseXmlOnly() {
            using var client = new RestClient(configureSerialization: config => config.UseXml());
            RoundtripXml(client);
        }

        public void UseDefaultJson() {
            using var client = new RestClient();
            RoundtripJson(client);
        }

        // Keep Newtonsoft references out of the default/XML path, including its JIT compilation.
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void UseNewtonsoft() {
            using var client = new RestClient(configureSerialization: config => config.UseNewtonsoftJson());
            RoundtripJson(client);
            RoundtripXml(client);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void UseNewtonsoftOnly() {
            using var client = new RestClient(configureSerialization: config => config.UseOnlySerializer(() => new JsonNetSerializer()));
            RoundtripJson(client);
        }

        static void RoundtripJson(RestClient client) {
            var serializer = client.Serializers.GetSerializer(DataFormat.Json);
            var body       = serializer.Serialize(new JsonParameter(new Payload { Value = "roundtrip" }));
            var result     = serializer.Deserializer.Deserialize<Payload>(new RestResponse(new RestRequest()) { Content = body });
            if (result?.Value != "roundtrip") throw new InvalidOperationException("JSON roundtrip failed.");
        }

        static void RoundtripXml(RestClient client) {
            var serializer = client.Serializers.GetSerializer(DataFormat.Xml);
            var body       = serializer.Serialize(new XmlParameter(new Payload { Value = "roundtrip" }));
            var result     = serializer.Deserializer.Deserialize<Payload>(new RestResponse(new RestRequest()) { Content = body });
            if (result?.Value != "roundtrip") throw new InvalidOperationException("XML roundtrip failed.");
        }
    }

    public class Payload {
        public string Value { get; set; }
    }
}
#endif
