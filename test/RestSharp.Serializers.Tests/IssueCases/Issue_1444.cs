//  Copyright © 2009-2020 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using RestSharp.Deserializers;
using RestSharp.Serialization.Json;

namespace RestSharp.Serializers.Tests.IssueCases
{
    // https://github.com/restsharp/RestSharp/issues/1444
    public class Issue_1444
    {
        [Test]
        public void Complex_type_deserialized_with_SimpleJson()
        {
            const string json = @"{""panes"":{""filter"":{""records"":[{""data"":{""customernumber"":""10002""}}]}}}";

            var actual = (new JsonDeserializer()).Deserialize<FilterBaseModel>(new RestResponse {Content = json});

            actual.Panes.Filter.Records.First().Data.Number.Should().Be("10002");
        }

        class FilterBaseModel
        {
            public Panes Panes { get; set; }
        }

        class Panes
        {
            public Filter Filter { get; set; }
        }

        class Filter
        {
            public List<Record> Records { get; set; }
        }

        public class Record
        {
            public Data Data { get; set; }
        }

        public class Data
        {
            [DeserializeAs(Name = "customernumber")]
            public string Number { get; set; }
        }
    }
}
