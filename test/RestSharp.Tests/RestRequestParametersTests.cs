//  Copyright (c) .NET Foundation and Contributors
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

namespace RestSharp.Tests;

public class RestRequestValidateParametersTests {
    [Fact]
    public void RestRequest_AlwaysMultipartFormData_IsAllowed() {
        var request = new RestRequest {
            AlwaysMultipartFormData = true
        };

        request.ValidateParameters();
    }

    [Fact]
    public void RestRequest_AlwaysSingleFileAsContent_IsAllowed() {
        var request = new RestRequest {
            AlwaysSingleFileAsContent = true
        };

        request.ValidateParameters();
    }

    [Fact]
    public void RestRequest_AlwaysSingleFileAsContent_And_AlwaysMultipartFormData_IsNotAllowed() {
        var request = new RestRequest {
            AlwaysSingleFileAsContent = true,
            AlwaysMultipartFormData   = true
        };

        Assert.Throws<ArgumentException>(
            () => { request.ValidateParameters(); }
        );
    }
    
    [Fact]
    public void RestRequest_AlwaysSingleFileAsContent_And_PostParameters_IsNotAllowed() {
        var request = new RestRequest {
            Method = Method.Post,
            AlwaysSingleFileAsContent = true,
        };

        request.AddParameter("name", "value", ParameterType.GetOrPost);

        Assert.Throws<ArgumentException>(
            () => { request.ValidateParameters(); }
        );
    }
    
    [Fact]
    public void RestRequest_AlwaysSingleFileAsContent_And_BodyParameters_IsNotAllowed() {
        var request = new RestRequest {
            AlwaysSingleFileAsContent = true,
        };

        request.AddParameter("name", "value", ParameterType.RequestBody);

        Assert.Throws<ArgumentException>(
            () => { request.ValidateParameters(); }
        );
    }
}