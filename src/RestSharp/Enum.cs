//   Copyright © 2009-2020 John Sheehan, Andrew Young, Alexey Zimarev and RestSharp community
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

namespace RestSharp
{
    /// <summary>
    ///     Types of parameters that can be added to requests
    /// </summary>
    public enum ParameterType
    {
        /// <summary>
        /// Cookie parameter
        /// </summary>
        Cookie, 
        GetOrPost, UrlSegment, HttpHeader, RequestBody, QueryString,
        QueryStringWithoutEncode
    }

    /// <summary>
    ///     Data formats
    /// </summary>
    public enum DataFormat { Json, Xml, None }

    /// <summary>
    ///     HTTP method to use when making requests
    /// </summary>
    public enum Method
    {
        GET, POST, PUT, DELETE, HEAD, OPTIONS,
        PATCH, MERGE, COPY
    }

    /// <summary>
    ///     Format strings for commonly-used date formats
    /// </summary>
    public struct DateFormat
    {
        /// <summary>
        ///     .NET format string for ISO 8601 date format
        /// </summary>
        public const string ISO_8601 = "s";

        /// <summary>
        ///     .NET format string for roundtrip date format
        /// </summary>
        public const string ROUND_TRIP = "u";
    }

    /// <summary>
    ///     Status for responses (surprised?)
    /// </summary>
    public enum ResponseStatus { None, Completed, Error, TimedOut, Aborted }
}