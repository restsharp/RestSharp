//  Copyright © 2009-2021 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
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

using RestSharp.Authenticators.OAuth.Extensions;

namespace RestSharp; 

class RequestParameters {
    static readonly ParameterType[] MultiParameterTypes = { ParameterType.QueryString, ParameterType.GetOrPost };

    public ParametersCollection Parameters { get; } = new();

    public RequestParameters AddRequestParameters(RestRequest request) {
        Parameters.AddParameters(request.Parameters);
        return this;
    }

    // move RestClient.DefaultParameters into Request.Parameters
    public RequestParameters AddDefaultParameters(RestClient client) {
        foreach (var defaultParameter in client.DefaultParameters) {
            var parameterExists = Parameters.Exists(defaultParameter);

            if (client.Options.AllowMultipleDefaultParametersWithSameName) {
                var isMultiParameter = MultiParameterTypes.Any(pt => pt == defaultParameter.Type);
                parameterExists = !isMultiParameter && parameterExists;
            }

            if (!parameterExists) Parameters.AddParameter(defaultParameter);
        }

        return this;
    }

    // Add Accept header based on registered deserializers if none has been set by the caller.
    public RequestParameters AddAcceptHeader(RestClient client) {
        if (Parameters.TryFind(KnownHeaders.Accept) == null) {
            var accepts = string.Join(", ", client.AcceptedContentTypes);
            Parameters.AddParameter(new Parameter(KnownHeaders.Accept, accepts, ParameterType.HttpHeader));
        }

        return this;
    }
}