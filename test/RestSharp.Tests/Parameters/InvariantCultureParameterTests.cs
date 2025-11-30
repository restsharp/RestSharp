using System.Globalization;

namespace RestSharp.Tests.Parameters;

public class InvariantCultureParameterTests {
    [Fact]
    public void AddParameter_Double_UsesInvariantCulture_WhenConfigured() {
        // Save original culture
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try {
            // Set a culture that uses comma as decimal separator
            Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");
            
            var options = new RestClientOptions { CultureForParameters = CultureInfo.InvariantCulture };
            using var client = new RestClient(options);
            var request = new RestRequest();
            request.AddParameter(client, "value", 1.234);
            
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
    public void AddOrUpdateParameter_Double_UsesInvariantCulture_WhenConfigured() {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");
            
            var options = new RestClientOptions { CultureForParameters = CultureInfo.InvariantCulture };
            using var client = new RestClient(options);
            var request = new RestRequest();
            request.AddOrUpdateParameter(client, "value", 1.234);
            
            var parameter = request.Parameters.First();
            parameter.Value.Should().Be("1.234");
        }
        finally {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void AddQueryParameter_Double_UsesInvariantCulture_WhenConfigured() {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");
            
            var options = new RestClientOptions { CultureForParameters = CultureInfo.InvariantCulture };
            using var client = new RestClient(options);
            var request = new RestRequest();
            request.AddQueryParameter(client, "value", 1.234);
            
            var parameter = request.Parameters.First();
            parameter.Value.Should().Be("1.234");
        }
        finally {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void AddUrlSegment_Double_UsesInvariantCulture_WhenConfigured() {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");
            
            var options = new RestClientOptions { CultureForParameters = CultureInfo.InvariantCulture };
            using var client = new RestClient(options);
            var request = new RestRequest("{value}");
            request.AddUrlSegment(client, "value", 1.234);
            
            var parameter = request.Parameters.First();
            parameter.Value.Should().Be("1.234");
        }
        finally {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void AddDefaultParameter_Double_UsesInvariantCulture_WhenConfigured() {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");
            
            var options = new RestClientOptions { CultureForParameters = CultureInfo.InvariantCulture };
            using var client = new RestClient(options);
            client.AddDefaultParameter("value", 1.234);
            
            var parameter = client.DefaultParameters.First(p => p.Name == "value");
            parameter.Value.Should().Be("1.234");
        }
        finally {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void AddDefaultQueryParameter_Double_UsesInvariantCulture_WhenConfigured() {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");
            
            var options = new RestClientOptions { CultureForParameters = CultureInfo.InvariantCulture };
            using var client = new RestClient(options);
            client.AddDefaultQueryParameter("value", 1.234);
            
            var parameter = client.DefaultParameters.First(p => p.Name == "value");
            parameter.Value.Should().Be("1.234");
        }
        finally {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void AddDefaultUrlSegment_Double_UsesInvariantCulture_WhenConfigured() {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");
            
            var options = new RestClientOptions { CultureForParameters = CultureInfo.InvariantCulture };
            using var client = new RestClient(options);
            client.AddDefaultUrlSegment("value", 1.234);
            
            var parameter = client.DefaultParameters.First(p => p.Name == "value");
            parameter.Value.Should().Be("1.234");
        }
        finally {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void FormatValue_Double_UsesInvariantCulture_WhenConfigured() {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");
            
            var options = new RestClientOptions { CultureForParameters = CultureInfo.InvariantCulture };
            using var client = new RestClient(options);
            
            var formattedValue = client.FormatValue(1.234);
            
            formattedValue.Should().Be("1.234");
        }
        finally {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void AddParameter_Decimal_UsesInvariantCulture_WhenConfigured() {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
            
            var options = new RestClientOptions { CultureForParameters = CultureInfo.InvariantCulture };
            using var client = new RestClient(options);
            var request = new RestRequest();
            request.AddParameter(client, "value", 123.456m);
            
            var parameter = request.Parameters.First();
            parameter.Value.Should().Be("123.456");
        }
        finally {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void AddParameter_Float_UsesInvariantCulture_WhenConfigured() {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
            
            var options = new RestClientOptions { CultureForParameters = CultureInfo.InvariantCulture };
            using var client = new RestClient(options);
            var request = new RestRequest();
            request.AddParameter(client, "value", 2.5f);
            
            var parameter = request.Parameters.First();
            parameter.Value.Should().Be("2.5");
        }
        finally {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void AddParameter_DateTime_UsesInvariantCulture_WhenConfigured() {
        var originalCulture = Thread.CurrentThread.CurrentCulture;
        try {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");
            
            var dateTime = new DateTime(2024, 12, 25, 10, 30, 0, DateTimeKind.Unspecified);
            var options = new RestClientOptions { CultureForParameters = CultureInfo.InvariantCulture };
            using var client = new RestClient(options);
            var request = new RestRequest();
            request.AddParameter(client, "date", dateTime);
            
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
            
            var options = new RestClientOptions { CultureForParameters = CultureInfo.InvariantCulture };
            using var client = new RestClient(options);
            var requestWithInvariant = new RestRequest();
            requestWithInvariant.AddParameter(client, "value", 12345);
            
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

    [Fact]
    public void CultureForParameters_DefaultValue_IsNull() {
        var options = new RestClientOptions();
        options.CultureForParameters.Should().BeNull();
    }
}
