using System.Globalization;

namespace RestSharp.Tests;

public class InvariantCultureTests {
    [Fact]
    public void AddParameter_uses_invariant_culture_for_double() {
        var originalCulture = CultureInfo.CurrentCulture;

        try {
            CultureInfo.CurrentCulture = new CultureInfo("da-DK");
            var request = new RestRequest().AddParameter("value", 1.234);

            var parameter = request.Parameters.FirstOrDefault(p => p.Name == "value");
            parameter.Should().NotBeNull();
            parameter!.Value.Should().Be("1.234");
        }
        finally {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void AddParameter_can_use_specific_culture() {
        var request = new RestRequest().AddParameter("value", 1.234, culture: new CultureInfo("da-DK"));

        var parameter = request.Parameters.FirstOrDefault(p => p.Name == "value");
        parameter.Should().NotBeNull();
        parameter!.Value.Should().Be("1,234");
    }

    [Fact]
    public void AddOrUpdateParameter_uses_invariant_culture_for_double() {
        var originalCulture = CultureInfo.CurrentCulture;

        try {
            CultureInfo.CurrentCulture = new CultureInfo("da-DK");
            var request = new RestRequest().AddOrUpdateParameter("value", 1.234);

            var parameter = request.Parameters.FirstOrDefault(p => p.Name == "value");
            parameter.Should().NotBeNull();
            parameter!.Value.Should().Be("1.234");
        }
        finally {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void AddQueryParameter_uses_invariant_culture_for_decimal() {
        var originalCulture = CultureInfo.CurrentCulture;

        try {
            CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
            var request = new RestRequest().AddQueryParameter("price", 99.95m);

            var parameter = request.Parameters.FirstOrDefault(p => p.Name == "price");
            parameter.Should().NotBeNull();
            parameter!.Value.Should().Be("99.95");
        }
        finally {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void AddUrlSegment_uses_invariant_culture_for_float() {
        var originalCulture = CultureInfo.CurrentCulture;

        try {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            var request = new RestRequest("{id}").AddUrlSegment("id", 3.14f);

            var parameter = request.Parameters.FirstOrDefault(p => p.Name == "id");
            parameter.Should().NotBeNull();
            parameter!.Value.Should().Be("3.14");
        }
        finally {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void CreateParameter_uses_invariant_culture_for_object_value() {
        var originalCulture = CultureInfo.CurrentCulture;

        try {
            CultureInfo.CurrentCulture = new CultureInfo("da-DK");
            object value = 1.234;
            var parameter = Parameter.CreateParameter("value", value, ParameterType.QueryString);

            parameter.Value.Should().Be("1.234");
        }
        finally {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}
