using CsvHelper.Configuration;
using RestSharp.Serializers.CsvHelper;
using RestSharp.Serializers.Json;
using RestSharp.Tests.Shared.Extensions;
using RestSharp.Tests.Shared.Fixtures;
using System.Globalization;
using System.Net;
using System.Text;

namespace RestSharp.Tests.Serializers.Csv;

public sealed class CsvHelperTests : IDisposable {
    static readonly Fixture Fixture = new();

    readonly WireMockServer _server = WireMockServer.Start();

    void ConfigureResponse(object expected) {
        var serializer = new CsvHelperSerializer();

        _server
            .Given(Request.Create().WithPath("/").UsingGet())
            .RespondWith(Response.Create().WithBody(serializer.Serialize(expected)!).WithHeader(KnownHeaders.ContentType, ContentType.Csv));
    }

    TestObject CreateTestObject() {
        var expected = Fixture.Create<TestObject>();

        expected.DateTimeValue = new DateTime(
            expected.DateTimeValue.Year,
            expected.DateTimeValue.Month,
            expected.DateTimeValue.Day,
            expected.DateTimeValue.Hour,
            expected.DateTimeValue.Minute,
            expected.DateTimeValue.Second
        );

        return expected;
    }

    [Fact]
    public async Task Use_CsvHelper_For_Response() {
        var expected = CreateTestObject();

        ConfigureResponse(expected);

        var client = new RestClient(_server.Url!, configureSerialization: cfg => cfg.UseCsvHelper());
        var actual = await client.GetAsync<TestObject>(new RestRequest());

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Use_CsvHelper_For_Collection_Response() {
        var count    = Fixture.Create<int>();
        var expected = new List<TestObject>(count);

        for (var i = 0; i < count; i++) {
            var item = CreateTestObject();
            expected.Add(item);
        }

        ConfigureResponse(expected);

        var client = new RestClient(_server.Url!, configureSerialization: cfg => cfg.UseCsvHelper());
        var actual = await client.GetAsync<List<TestObject>>(new RestRequest());

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task DeserilizationFails_IsSuccessful_Should_BeFalse() {
        _server
            .Given(Request.Create().WithPath("/").UsingGet())
            .RespondWith(Response.Create().WithBody("invalid csv").WithHeader(KnownHeaders.ContentType, ContentType.Csv));

        var client = new RestClient(_server.Url!, configureSerialization: cfg => cfg.UseCsvHelper());

        var response = await client.ExecuteAsync<TestObject>(new RestRequest());

        response.IsSuccessStatusCode.Should().BeTrue();
        response.IsSuccessful.Should().BeFalse();
    }

    [Fact]
    public async Task DeserilizationSucceeds_IsSuccessful_Should_BeTrue() {
        var item = CreateTestObject();
        ConfigureResponse(item);

        var client   = new RestClient(_server.Url!, configureSerialization: cfg => cfg.UseSystemTextJson());
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

        var item = new TestObject {
            Int32Value    = 32,
            SingleValue   = 16.5f,
            StringValue   = "hello",
            TimeSpanValue = TimeSpan.FromMinutes(10),
            DateTimeValue = new DateTime(2024, 1, 20)
        };

        var actual = serializer.Serialize(item);

        const string expected =
            "StringValue,Int32Value,DecimalValue,DoubleValue,SingleValue,DateTimeValue,TimeSpanValue;hello,32,0,0,16.5,01/20/2024 00:00:00,00:10:00;";

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

        serializer.Serialize(items)
            .Should()
            .Be(
                "StringValue,Int32Value,DecimalValue,DoubleValue,SingleValue,DateTimeValue,TimeSpanValue;hello,32,0,0,16.5,01/20/2024 00:00:00,00:10:00;,65,89.555,0,0,08/19/2022 05:15:21,00:01:01;\"String, with comma\",0,0,20.00001,80000,01/01/0001 00:00:00,00:00:00;"
            );
    }

    public void Dispose() => _server?.Dispose();
}