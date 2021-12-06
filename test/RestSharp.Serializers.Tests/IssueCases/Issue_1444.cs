using RestSharp.Deserializers;
using RestSharp.Serialization.Json;

namespace RestSharp.Serializers.Tests.IssueCases; 

// https://github.com/restsharp/RestSharp/issues/1444
public class Issue_1444 {
    [Fact]
    public void Complex_type_deserialized_with_SimpleJson() {
        const string json = @"{""panes"":{""filter"":{""records"":[{""data"":{""customernumber"":""10002""}}]}}}";

        var actual = new JsonDeserializer().Deserialize<FilterBaseModel>(new RestResponse { Content = json });

        actual.Panes.Filter.Records.First().Data.Number.Should().Be("10002");
    }

    class FilterBaseModel {
        public Panes Panes { get; set; }
    }

    class Panes {
        public Filter Filter { get; set; }
    }

    class Filter {
        public List<Record> Records { get; set; }
    }

    class Record {
        public Data Data { get; set; }
    }

    class Data {
        [DeserializeAs(Name = "customernumber")]
        public string Number { get; set; }
    }
}