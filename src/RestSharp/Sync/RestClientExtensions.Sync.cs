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

namespace RestSharp; 

[PublicAPI]
public static class RestClientSyncExtensions {
    /// <summary>
    /// Executes a GET-style request synchronously, authenticating if needed.
    /// The response content then gets deserialized to T.
    /// </summary>
    /// <typeparam name="T">Target deserialization type</typeparam>
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    /// <returns>Deserialized response content</returns>
    public static RestResponse<T> ExecuteGet<T>(this RestClient client, RestRequest request)
        => AsyncHelpers.RunSync(() => client.ExecuteAsync<T>(request, Method.Get));

    /// <summary>
    /// Executes a GET-style synchronously, authenticating if needed
    /// </summary>
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    public static RestResponse ExecuteGet(this RestClient client, RestRequest request)
        => AsyncHelpers.RunSync(() => client.ExecuteAsync(request, Method.Get));
    
    /// <summary>
    /// Executes a POST-style request synchronously, authenticating if needed.
    /// The response content then gets deserialized to T.
    /// </summary>
    /// <typeparam name="T">Target deserialization type</typeparam>
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    /// <returns>Deserialized response content</returns>
    public static RestResponse<T> ExecutePost<T>(
        this RestClient   client,
        RestRequest       request
    )
        => AsyncHelpers.RunSync(() => client.ExecuteAsync<T>(request, Method.Post));

    /// <summary>
    /// Executes a POST-style synchronously, authenticating if needed
    /// </summary>
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    public static RestResponse ExecutePost(this RestClient client, RestRequest request)
        => AsyncHelpers.RunSync(() => client.ExecuteAsync(request, Method.Post));

    /// <summary>
    /// Executes a PUT-style request synchronously, authenticating if needed.
    /// The response content then gets deserialized to T.
    /// </summary>
    /// <typeparam name="T">Target deserialization type</typeparam>
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    /// <returns>Deserialized response content</returns>
    public static RestResponse<T> ExecutePut<T>(
        this RestClient   client,
        RestRequest       request
    )
        => AsyncHelpers.RunSync(() => client.ExecuteAsync<T>(request, Method.Put));

    /// <summary>
    /// Executes a PUP-style synchronously, authenticating if needed
    /// </summary>
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    public static RestResponse ExecutePut(this RestClient client, RestRequest request)
        => AsyncHelpers.RunSync(() => client.ExecuteAsync(request, Method.Put));

    /// <summary>
    /// Executes the request synchronously, authenticating if needed
    /// </summary>
    /// <typeparam name="T">Target deserialization type</typeparam>
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    public static RestResponse<T> Execute<T>(
        this RestClient client,
        RestRequest     request
    )
        => AsyncHelpers.RunSync(() => client.ExecuteAsync<T>(request));

    /// <summary>
    /// Executes the request synchronously, authenticating if needed
    /// </summary>
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    /// <param name="httpMethod">Override the request method</param>
    public static RestResponse Execute(
        this RestClient   client,
        RestRequest       request,
        Method            httpMethod
    )
        => AsyncHelpers.RunSync(() => client.ExecuteAsync(request, httpMethod));

    /// <summary>
    /// Executes the request synchronously, authenticating if needed
    /// </summary>
    /// <typeparam name="T">Target deserialization type</typeparam>
    /// <param name="client"></param>
    /// <param name="request">Request to be executed</param>
    /// <param name="httpMethod">Override the request method</param>
    public static RestResponse<T> Execute<T>(
        this RestClient   client,
        RestRequest       request,
        Method            httpMethod
    )
        => AsyncHelpers.RunSync(() => client.ExecuteAsync<T>(request, httpMethod));

    /// <summary>
    /// Execute the request using GET HTTP method. Exception will be thrown if the request does not succeed.
    /// The response data is deserialized to the Data property of the returned response object.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="request">The request</param>
    /// <typeparam name="T">Expected result type</typeparam>
    /// <returns></returns>
    public static T? Get<T>(this RestClient client, RestRequest request)
        => AsyncHelpers.RunSync(() => client.GetAsync<T>(request));
    
    public static RestResponse Get(this RestClient client, RestRequest request)
        => AsyncHelpers.RunSync(() => client.GetAsync(request));

    /// <summary>
    /// Execute the request using POST HTTP method. Exception will be thrown if the request does not succeed.
    /// The response data is deserialized to the Data property of the returned response object.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="request">The request</param>
    /// <typeparam name="T">Expected result type</typeparam>
    /// <returns></returns>
    public static T? Post<T>(this RestClient client, RestRequest request)
        => AsyncHelpers.RunSync(() => client.PostAsync<T>(request));

    /// <summary>
    /// Execute the request using PUT HTTP method. Exception will be thrown if the request does not succeed.
    /// The response data is deserialized to the Data property of the returned response object.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="request">The request</param>
    /// <typeparam name="T">Expected result type</typeparam>
    /// <returns></returns>
    public static T? Put<T>(this RestClient client, RestRequest request)
        => AsyncHelpers.RunSync(() => client.PutAsync<T>(request));

    public static RestResponse Put(this RestClient client, RestRequest request)
        => AsyncHelpers.RunSync(() => client.PutAsync(request));

    /// <summary>
    /// Execute the request using HEAD HTTP method. Exception will be thrown if the request does not succeed.
    /// The response data is deserialized to the Data property of the returned response object.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="request">The request</param>
    /// <typeparam name="T">Expected result type</typeparam>
    /// <returns></returns>
    public static T? Head<T>(this RestClient client, RestRequest request)
        => AsyncHelpers.RunSync(() => client.HeadAsync<T>(request));

    public static RestResponse Head(this RestClient client, RestRequest request)
        => AsyncHelpers.RunSync(() => client.HeadAsync(request));

    /// <summary>
    /// Execute the request using OPTIONS HTTP method. Exception will be thrown if the request does not succeed.
    /// The response data is deserialized to the Data property of the returned response object.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="request">The request</param>
    /// <typeparam name="T">Expected result type</typeparam>
    /// <returns></returns>
    public static T? Options<T>(this RestClient client, RestRequest request)
        => AsyncHelpers.RunSync(() => client.OptionsAsync<T>(request));

    public static RestResponse Options(this RestClient client, RestRequest request)
        => AsyncHelpers.RunSync(() => client.OptionsAsync(request));

    /// <summary>
    /// Execute the request using PATCH HTTP method. Exception will be thrown if the request does not succeed.
    /// The response data is deserialized to the Data property of the returned response object.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="request">The request</param>
    /// <typeparam name="T">Expected result type</typeparam>
    /// <returns></returns>
    [PublicAPI]
    public static T? Patch<T>(this RestClient client, RestRequest request)
        => AsyncHelpers.RunSync(() => client.PatchAsync<T>(request));

    [PublicAPI]
    public static RestResponse Patch(this RestClient client, RestRequest request)
        => AsyncHelpers.RunSync(() => client.PatchAsync(request));

    /// <summary>
    /// Execute the request using DELETE HTTP method. Exception will be thrown if the request does not succeed.
    /// The response data is deserialized to the Data property of the returned response object.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="request">The request</param>
    /// <typeparam name="T">Expected result type</typeparam>
    /// <returns></returns>
    public static T? Delete<T>(this RestClient client, RestRequest request)
        => AsyncHelpers.RunSync(() => client.DeleteAsync<T>(request));

    public static RestResponse Delete(this RestClient client, RestRequest request)
        => AsyncHelpers.RunSync(() => client.DeleteAsync(request));

    /// <summary>
    /// A specialized method to download files.
    /// </summary>
    /// <param name="client">RestClient instance</param>
    /// <param name="request">Pre-configured request instance.</param>
    /// <returns>The downloaded file.</returns>
    public static byte[]? DownloadData(this RestClient client, RestRequest request)
        => AsyncHelpers.RunSync(() => client.DownloadDataAsync(request));
}
