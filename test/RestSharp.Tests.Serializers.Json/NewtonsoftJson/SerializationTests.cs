using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp.Serializers.NewtonsoftJson;

namespace RestSharp.Tests.Serializers.Json.NewtonsoftJson;

public class SerializationTests {
    static readonly Fixture Fixture = new();

    readonly JsonSerializerSettings _jsonSerializerSettings = new() {
        ContractResolver = new DefaultContractResolver {
            NamingStrategy = new CamelCaseNamingStrategy()
        },
        Formatting = Formatting.None
    };

    [Fact]
    public void Serialize_multiple_objects_within_one_thread() {
        var serializer = new JsonNetSerializer();
        var dummy      = Fixture.CreateMany<TestClass>().ToArray();
        var expectedSerializations = dummy.Select(
            d => JsonConvert.SerializeObject(d, _jsonSerializerSettings)
        ).ToList();
        var actualSerializations = dummy.Select(serializer.Serialize).ToList();
        actualSerializations.Should().BeEquivalentTo(expectedSerializations);
    }

    [Fact]
    public void Serialize_within_multiple_threads() {
        var serializer = new JsonNetSerializer();

        Parallel.For(
            0,
            100,
            _ => {
                var dummy = Fixture.Create<TestClass>();

                var expectedSerialization = JsonConvert.SerializeObject(dummy, _jsonSerializerSettings);
                var actualSerialization   = serializer.Serialize(dummy);

                actualSerialization.Should().Be(expectedSerialization);
            }
        );
    }
}