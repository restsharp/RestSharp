using System.Globalization;

namespace RestSharp.Tests.Parameters;

public class InvariantCultureParameterTests {
    [Fact]
    public void AddParameter_Double_UsesInvariantCulture_WhenOptIn() {
        // Save original culture
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try {
            // Set a culture that uses comma as decimal separator
            Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");
            
            var request = new RestRequest();
            request.AddParameter("value", 1.234, useInvariantCulture: true);
            
            var parameter = request.Parameters.First();
            parameter.Value.Should().Be("1.234");
        }
        finally {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void AddParameter_Double_UsesCurrentCulture_ByDefault() {
        // Save original culture
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try {
            // Set a culture that uses comma as decimal separator
            Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");
            
            var request = new RestRequest();
            request.AddParameter("value", 1.234);
            
            var parameter = request.Parameters.First();
            // Default behavior uses current culture (comma as decimal separator)
            parameter.Value.Should().Be("1,234");
        }
        finally {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void AddOrUpdateParameter_Double_UsesInvariantCulture_WhenOptIn() {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");
            
            var request = new RestRequest();
            request.AddOrUpdateParameter("value", 1.234, useInvariantCulture: true);
            
            var parameter = request.Parameters.First();
            parameter.Value.Should().Be("1.234");
        }
        finally {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void AddQueryParameter_Double_UsesInvariantCulture_WhenOptIn() {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");
            
            var request = new RestRequest();
            request.AddQueryParameter("value", 1.234, useInvariantCulture: true);
            
            var parameter = request.Parameters.First();
            parameter.Value.Should().Be("1.234");
        }
        finally {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void AddUrlSegment_Double_UsesInvariantCulture_WhenOptIn() {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");
            
            var request = new RestRequest("{value}");
            request.AddUrlSegment("value", 1.234, useInvariantCulture: true);
            
            var parameter = request.Parameters.First();
            parameter.Value.Should().Be("1.234");
        }
        finally {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void AddParameter_Decimal_UsesInvariantCulture_WhenOptIn() {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
            
            var request = new RestRequest();
            request.AddParameter("value", 123.456m, useInvariantCulture: true);
            
            var parameter = request.Parameters.First();
            parameter.Value.Should().Be("123.456");
        }
        finally {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void AddParameter_Float_UsesInvariantCulture_WhenOptIn() {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
            
            var request = new RestRequest();
            request.AddParameter("value", 2.5f, useInvariantCulture: true);
            
            var parameter = request.Parameters.First();
            parameter.Value.Should().Be("2.5");
        }
        finally {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void AddParameter_DateTime_UsesInvariantCulture_WhenOptIn() {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");
            
            var dateTime = new DateTime(2024, 12, 25, 10, 30, 0, DateTimeKind.Unspecified);
            var request = new RestRequest();
            request.AddParameter("date", dateTime, useInvariantCulture: true);
            
            var parameter = request.Parameters.First();
            // DateTime.ToString with InvariantCulture uses MM/dd/yyyy format
            parameter.Value.Should().Be("12/25/2024 10:30:00");
        }
        finally {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void AddParameter_Integer_SameValueWithOrWithoutInvariantCulture() {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");
            
            var requestWithInvariant = new RestRequest();
            requestWithInvariant.AddParameter("value", 12345, useInvariantCulture: true);
            
            var requestWithoutInvariant = new RestRequest();
            requestWithoutInvariant.AddParameter("value", 12345);
            
            var parameterWithInvariant = requestWithInvariant.Parameters.First();
            var parameterWithoutInvariant = requestWithoutInvariant.Parameters.First();
            
            parameterWithInvariant.Value.Should().Be("12345");
            parameterWithoutInvariant.Value.Should().Be("12345");
        }
        finally {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }
}
