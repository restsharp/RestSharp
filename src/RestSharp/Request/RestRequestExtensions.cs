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

namespace RestSharp;

[PublicAPI]
public static partial class RestRequestExtensions {
    /// <summary>
    /// Adds a HTTP parameter to the request (QueryString for GET, DELETE, OPTIONS and HEAD; Encoded form for POST and PUT)
    /// </summary>
    /// <param name="request"></param>
    /// <param name="name">Name of the parameter</param>
    /// <param name="value">Value of the parameter</param>
    /// <param name="encode">Encode the value or not, default true</param>
    /// <returns>This request</returns>
    public static RestRequest AddParameter(this RestRequest request, string name, string? value, bool encode = true)
        => request.AddParameter(new GetOrPostParameter(name, value, encode));

    /// <summary>
    /// Adds a parameter of a given type to the request. It will create a typed parameter instance based on the type argument.
    /// It is not recommended to use this overload unless you must, as it doesn't provide any restrictions, and if the name-value-type
    /// combination doesn't match, it will throw.
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="name">Name of the parameter, must be matching a placeholder in the resource URL as {name}</param>
    /// <param name="value">Value of the parameter</param>
    /// <param name="type">Enum value specifying what kind of parameter is being added</param>
    /// <param name="encode">Encode the value or not, default true</param>
    /// <returns></returns>
    public static RestRequest AddParameter(this RestRequest request, string? name, object value, ParameterType type, bool encode = true)
        => type == ParameterType.RequestBody
            ? request.AddBodyParameter(name, value)
            : request.AddParameter(Parameter.CreateParameter(name, value, type, encode));

    /// <summary>
    /// Adds a HTTP parameter to the request (QueryString for GET, DELETE, OPTIONS and HEAD; Encoded form for POST and PUT).
    /// The value will be converted to string.
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="name">Name of the parameter</param>
    /// <param name="value">Value of the parameter</param>
    /// <param name="encode">Encode the value or not, default true</param>
    /// <returns>This request</returns>
    public static RestRequest AddParameter<T>(this RestRequest request, string name, T value, bool encode = true) where T : struct
        => request.AddParameter(name, value.ToString(), encode);

    /// <summary>
    /// Adds or updates a HTTP parameter to the request (QueryString for GET, DELETE, OPTIONS and HEAD; Encoded form for POST and PUT)
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="name">Name of the parameter</param>
    /// <param name="value">Value of the parameter</param>
    /// <param name="encode">Encode the value or not, default true</param>
    /// <returns>This request</returns>
    public static RestRequest AddOrUpdateParameter(this RestRequest request, string name, string? value, bool encode = true)
        => request.AddOrUpdateParameter(new GetOrPostParameter(name, value, encode));

    /// <summary>
    /// Adds or updates a HTTP parameter to the request (QueryString for GET, DELETE, OPTIONS and HEAD; Encoded form for POST and PUT)
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="name">Name of the parameter</param>
    /// <param name="value">Value of the parameter</param>
    /// <param name="encode">Encode the value or not, default true</param>
    /// <returns>This request</returns>
    public static RestRequest AddOrUpdateParameter<T>(this RestRequest request, string name, T value, bool encode = true) where T : struct
        => request.AddOrUpdateParameter(name, value.ToString(), encode);

    static RestRequest AddParameters(this RestRequest request, IEnumerable<Parameter> parameters) {
        request.Parameters.AddParameters(parameters);
        return request;
    }

    /// <summary>
    /// Adds or updates request parameter of a given type. It will create a typed parameter instance based on the type argument.
    /// Parameter will be added or updated based on its name. If the request has a parameter with the same name, it will be updated.
    /// It is not recommended to use this overload unless you must, as it doesn't provide any restrictions, and if the name-value-type
    /// combination doesn't match, it will throw.
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="name">Name of the parameter, must be matching a placeholder in the resource URL as {name}</param>
    /// <param name="value">Value of the parameter</param>
    /// <param name="type">Enum value specifying what kind of parameter is being added</param>
    /// <param name="encode">Encode the value or not, default true</param>
    /// <returns></returns>
    public static RestRequest AddOrUpdateParameter(this RestRequest request, string name, object value, ParameterType type, bool encode = true) {
        request.RemoveParameter(name, type);

        return type == ParameterType.RequestBody
            ? request.AddBodyParameter(name, value)
            : request.AddOrUpdateParameter(Parameter.CreateParameter(name, value, type, encode));
    }

    /// <summary>
    /// Adds or updates request parameter, given the parameter instance, for example <see cref="QueryParameter"/> or <see cref="UrlSegmentParameter"/>.
    /// It will replace an existing parameter with the same name.
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="parameter">Parameter instance</param>
    /// <returns></returns>
    public static RestRequest AddOrUpdateParameter(this RestRequest request, Parameter parameter)
        => request.RemoveParameter(parameter.Name, parameter.Type).AddParameter(parameter);

    /// <summary>
    /// Adds or updates multiple request parameters, given the parameter instance, for example
    /// <see cref="QueryParameter"/> or <see cref="UrlSegmentParameter"/>. Parameters with the same name will be replaced.
    /// </summary>
    /// <param name="request">Request instance</param>
    /// <param name="parameters">Collection of parameter instances</param>
    /// <returns></returns>
    public static RestRequest AddOrUpdateParameters(this RestRequest request, IEnumerable<Parameter> parameters) {
        foreach (var parameter in parameters) request.AddOrUpdateParameter(parameter);

        return request;
    }

    static RestRequest RemoveParameter(this RestRequest request, string? name, ParameterType type) {
        var p = request.Parameters.FirstOrDefault(x => x.Name == name && x.Type == type);
        return p != null ? request.RemoveParameter(p) : request;
    }

    /// <summary>
    /// Adds cookie to the <seealso cref="HttpClient"/> cookie container.
    /// </summary>
    /// <param name="request">RestRequest to add the cookies to</param>
    /// <param name="name">Cookie name</param>
    /// <param name="value">Cookie value</param>
    /// <param name="path">Cookie path</param>
    /// <param name="domain">Cookie domain, must not be an empty string</param>
    /// <returns></returns>
    public static RestRequest AddCookie(this RestRequest request, string name, string value, string path, string domain) {
        request.CookieContainer ??= new CookieContainer();
        request.CookieContainer.Add(new Cookie(name, value, path, domain));
        return request;
    }
}