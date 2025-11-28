using System.Globalization;

namespace RestSharp.Tests.Parameters;

public class InvariantCultureParameterTests {
    [Fact]
    public void AddParameter_Double_UsesInvariantCulture() {
        // Save original culture
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try {
            // Set a culture that uses comma as decimal separator
            Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");
            
            var request = new RestRequest();
            request.AddParameter("value", 1.234);
            
            var parameter = request.Parameters.First();
            parameter.Value.Should().Be("1.234");
        }
        finally {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void AddOrUpdateParameter_Double_UsesInvariantCulture() {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");
            
            var request = new RestRequest();
            request.AddOrUpdateParameter("value", 1.234);
            
            var parameter = request.Parameters.First();
            parameter.Value.Should().Be("1.234");
        }
        finally {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void AddQueryParameter_Double_UsesInvariantCulture() {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");
            
            var request = new RestRequest();
            request.AddQueryParameter("value", 1.234);
            
            var parameter = request.Parameters.First();
            parameter.Value.Should().Be("1.234");
        }
        finally {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void AddUrlSegment_Double_UsesInvariantCulture() {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");
            
            var request = new RestRequest("{value}");
            request.AddUrlSegment("value", 1.234);
            
            var parameter = request.Parameters.First();
            parameter.Value.Should().Be("1.234");
        }
        finally {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void CreateParameter_Double_UsesInvariantCulture() {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");
            
            var parameter = Parameter.CreateParameter("value", 1.234, ParameterType.GetOrPost);
            
            parameter.Value.Should().Be("1.234");
        }
        finally {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void AddParameter_Decimal_UsesInvariantCulture() {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
            
            var request = new RestRequest();
            request.AddParameter("value", 123.456m);
            
            var parameter = request.Parameters.First();
            parameter.Value.Should().Be("123.456");
        }
        finally {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void AddParameter_Float_UsesInvariantCulture() {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
            
            var request = new RestRequest();
            request.AddParameter("value", 2.5f);
            
            var parameter = request.Parameters.First();
            parameter.Value.Should().Be("2.5");
        }
        finally {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void AddParameter_DateTime_UsesInvariantCulture() {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");
            
            var dateTime = new DateTime(2024, 12, 25, 10, 30, 0, DateTimeKind.Unspecified);
            var request = new RestRequest();
            request.AddParameter("date", dateTime);
            
            var parameter = request.Parameters.First();
            // DateTime.ToString with InvariantCulture uses MM/dd/yyyy format
            parameter.Value.Should().Be("12/25/2024 10:30:00");
        }
        finally {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void AddParameter_Integer_WorksWithAnyCulture() {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");
            
            var request = new RestRequest();
            request.AddParameter("value", 12345);
            
            var parameter = request.Parameters.First();
            parameter.Value.Should().Be("12345");
        }
        finally {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void AddObject_WithDoubleProperty_UsesInvariantCulture() {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");
            
            var request = new RestRequest();
            request.AddObject(new { Amount = 1.234 });
            
            var parameter = request.Parameters.First();
            parameter.Value.Should().Be("1.234");
        }
        finally {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }
}
