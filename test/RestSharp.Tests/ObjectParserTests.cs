using System.Globalization;

namespace RestSharp.Tests;

public class ObjectParserTests {
    [Fact]
    public void ShouldUseRequestProperty() {
        var now   = DateTime.Now;
        var dates = new[] { now, now.AddDays(1), now.AddDays(2) };
        var request = new TestObject {
            SomeData   = "test",
            SomeDate   = now,
            Plain      = 123,
            PlainArray = dates,
            DatesArray = dates
        };

        var parsed = request.GetProperties().ToDictionary(x => x.Name, x => x.Value);
        parsed["some_data"].Should().Be(request.SomeData);
        parsed["SomeDate"].Should().Be(request.SomeDate.ToString("d"));
        parsed["Plain"].Should().Be(request.Plain.ToString());
        // ReSharper disable once SpecifyACultureInStringConversionExplicitly
        parsed["PlainArray"].Should().Be(string.Join(",", dates.Select(x => x.ToString())));
        parsed["dates"].Should().Be(string.Join(",", dates.Select(x => x.ToString("d"))));
    }

    class TestObject {
        [RequestProperty(Name = "some_data")]
        public string SomeData { get; set; }

        [RequestProperty(Format = "d")]
        public DateTime SomeDate { get; set; }
        
        [RequestProperty(Name = "dates", Format = "d")]
        public DateTime[] DatesArray { get; set; }

        public int      Plain      { get; set; }
        public DateTime[] PlainArray { get; set; }
    }
}
