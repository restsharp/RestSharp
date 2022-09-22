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

public class RestXmlRequest : RestRequest {
    /// <summary>
    /// Used by the default deserializers to explicitly set which date format string to use when parsing dates.
    /// </summary>
    public string? DateFormat { get; set; }

    /// <summary>
    /// Used by XmlDeserializer. If not specified, XmlDeserializer will flatten response by removing namespaces from
    /// element names.
    /// </summary>
    public string? XmlNamespace { get; set; }
}