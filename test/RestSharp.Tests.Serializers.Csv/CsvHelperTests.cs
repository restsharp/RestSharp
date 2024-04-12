using CsvHelper.Configuration;
using RestSharp.Serializers.CsvHelper;
using RestSharp.Serializers.Json;
using System.Globalization;

namespace RestSharp.Tests.Serializers.Csv;

public sealed class CsvHelperTests : IDisposable {
    static readonly Fixture Fixture = new();

    static CsvHelperTests() => Fixture.Customize(new TestObjectCustomization());

    readonly WireMockServer _server = WireMockServer.Start();

    void ConfigureResponse(object expected) {
        var serializer = new CsvHelperSerializer();

        _server
            .Given(Request.Create().WithPath("/").UsingGet())
            .RespondWith(Response.Create().WithBody(serializer.Serialize(expected)!).WithHeader(KnownHeaders.ContentType, ContentType.Csv));
    }

    [Fact]
    public async Task Use_CsvHelper_For_Response() {
        var expected = Fixture.Create<TestObject>();
        ConfigureResponse(expected);
        using var client = new RestClient(_server.Url!, configureSerialization: cfg => cfg.UseCsvHelper());

        var actual = await client.GetAsync<TestObject>(new RestRequest());
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Use_CsvHelper_For_Collection_Response() {
        var expected = Fixture.CreateMany<TestObject>();

        ConfigureResponse(expected);
        using var client = new RestClient(_server.Url!, configureSerialization: cfg => cfg.UseCsvHelper());

        var actual = await client.GetAsync<List<TestObject>>(new RestRequest());
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Invalid_csv_request_body_should_fail() {
        _server
            .Given(Request.Create().WithPath("/").UsingGet())
            .RespondWith(Response.Create().WithBody("invalid csv").WithHeader(KnownHeaders.ContentType, ContentType.Csv));

        using var client = new RestClient(_server.Url!, configureSerialization: cfg => cfg.UseCsvHelper());

        var response = await client.ExecuteAsync<TestObject>(new RestRequest());
        response.IsSuccessStatusCode.Should().BeTrue();
        response.IsSuccessful.Should().BeFalse();
    }

    [Fact]
    public async Task Valid_csv_response_should_succeed() {
        var item = Fixture.Create<TestObject>();
        ConfigureResponse(item);

        using var client = new RestClient(_server.Url!, configureSerialization: cfg => cfg.UseSystemTextJson());

        var response = await client.ExecuteAsync<TestObject>(new RestRequest());
        response.IsSuccessStatusCode.Should().BeTrue();
        response.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void SerializedObject_Should_Be() {
        var serializer = new CsvHelperSerializer(
            new CsvConfiguration(CultureInfo.InvariantCulture) {
                NewLine = ";"
            }
        );

        var item     = Fixture.Create<TestObject>();
        var actual   = serializer.Serialize(item);
        var expected = $"{TestObject.Titles};{item.ToString(CultureInfo.InvariantCulture)};";

        actual.Should().Be(expected);
    }

    [Fact]
    public void SerializedCollection_Should_Be() {
        var serializer = new CsvHelperSerializer(
            new CsvConfiguration(CultureInfo.InvariantCulture) {
                NewLine = ";"
            }
        );

        var items = new TestObject[] {
            new() {
                Int32Value    = 32,
                SingleValue   = 16.5f,
                StringValue   = "hello",
                TimeSpanValue = TimeSpan.FromMinutes(10),
                DateTimeValue = new DateTime(2024, 1, 20)
            },
            new() {
                Int32Value    = 65,
                DecimalValue  = 89.555m,
                TimeSpanValue = TimeSpan.FromSeconds(61),
                DateTimeValue = new DateTime(2022, 8, 19, 5, 15, 21)
            },
            new() {
                SingleValue = 80000,
                DoubleValue = 20.00001,
                StringValue = "String, with comma"
            }
        };
        string[] strings = [TestObject.Titles, .. items.Select(i => i.ToString(CultureInfo.InvariantCulture))];

        var expected = $"{string.Join(";", strings)};";
        var actual   = serializer.Serialize(items);

        actual.Should().Be(expected);
    }

    public void Dispose() => _server?.Dispose();
}

class TestObjectCustomization : ICustomization {
    public void Customize(IFixture fixture)
        => fixture.Customize<TestObject>(
            o => o
                .WithAutoProperties()
                .With(
                    p => p.DateTimeValue,
                    () => {
                        var dt = fixture.Create<DateTime>();
                        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
                    }
                )
        );
}