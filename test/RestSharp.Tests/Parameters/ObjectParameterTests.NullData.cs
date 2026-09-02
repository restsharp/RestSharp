namespace RestSharp.Tests.Parameters;

public partial class ObjectParameterTests {
    [Fact]
    public void AddObjectStatic_skips_null_properties() {
        var data = new NullableData { Kind = "set" };

        var request = new RestRequest().AddObjectStatic(data);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(NullableData.Kind), "set"));
    }

    [Fact]
    public void AddObjectStatic_keeps_non_null_properties_and_skips_null_ones() {
        var data = new NullableData { Name = "Bob", Age = 30, Link = null, Values = null, Kind = "set" };

        var request = new RestRequest().AddObjectStatic(data);

        request
            .Parameters
            .Should()
            .BeEquivalentTo(new[] {
                new GetOrPostParameter(nameof(NullableData.Name), "Bob"),
                new GetOrPostParameter(nameof(NullableData.Age), "30"),
                new GetOrPostParameter(nameof(NullableData.Kind), "set")
            });
    }

    [Fact]
    public void AddObjectStatic_with_all_null_properties_yields_no_parameters() {
        var data = new NullableData { Kind = null };

        var request = new RestRequest().AddObjectStatic(data);

        request.Parameters.Should().BeEmpty();
    }

    [Fact]
    public void AddObjectStatic_null_property_handling_matches_AddObject() {
        var data = new NullableData { Name = null, Age = null, Link = null, Values = null, Kind = "set" };

        var objStatic = new RestRequest().AddObjectStatic(data);
        var reflection = new RestRequest().AddObject(data);

        objStatic.Parameters.Should().BeEquivalentTo(reflection.Parameters);
    }

    [Fact]
    public void AddObjectStatic_reads_each_property_once() {
        var data = new ChangingData();

        var request = new RestRequest().AddObjectStatic(data);

        request
            .Parameters
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .BeEquivalentTo(new GetOrPostParameter(nameof(ChangingData.Value), "set"));
        data.ReadCount.Should().Be(1);
    }

    class NullableData {
        public string Name { get; set; }
        public int? Age { get; set; }
        public Uri Link { get; set; }
        public List<int> Values { get; set; }
        public string Kind { get; set; } = "set";
    }

    class ChangingData {
        int _readCount;

        public string Value => ++_readCount == 1 ? "set" : null;
        internal int ReadCount => _readCount;
    }
}
