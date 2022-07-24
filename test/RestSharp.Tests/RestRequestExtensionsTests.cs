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

#nullable enable
using System.ComponentModel;

namespace RestSharp.Tests;

public sealed class RestRequestExtensionsTests {
    [Fact]
    public void RestRequest_AddParameters_AddsParameters() {
        var model = new User(Guid.Parse("27b7acdd-6184-4e21-9b64-cdaa2b2477bd"), "Joe", null, DateTime.Parse("2022-01-01T00:00:00Z"), 100, 100, 100);
        var request = new RestRequest().AddParameters(model, ParameterType.QueryString);

        string ConvertToInvariantString(object value) => TypeDescriptor.GetConverter(value.GetType()).ConvertToInvariantString(value);

        var expectedParameters = new ParametersCollection(
            new[] {
                new QueryParameter(nameof(User.Id), ConvertToInvariantString(model.Id)),
                new QueryParameter(nameof(User.FirstName), model.FirstName),
                new QueryParameter(nameof(User.LastName), model.LastName),
                new QueryParameter(nameof(User.LastLogin), ConvertToInvariantString(model.LastLogin)),
                new QueryParameter(nameof(User.NameSpan), model.NameSpan.ToString()),
                new QueryParameter(nameof(User.Score), ConvertToInvariantString(model.Score)),
                new QueryParameter(nameof(User.Age), ConvertToInvariantString(model.Age)),
                new QueryParameter(nameof(User.Special), ConvertToInvariantString(model.Special))
            }
        );

        request.Parameters.Should().BeEquivalentTo(expectedParameters);
    }

    sealed record User(Guid Id, string FirstName, string? LastName, DateTime? LastLogin, int Score, uint Age, nint Special) {
        public ReadOnlySpan<char> NameSpan => FirstName.AsSpan();
    }
}
