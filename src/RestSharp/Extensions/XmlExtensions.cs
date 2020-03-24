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

using System.Xml.Linq;

namespace RestSharp.Extensions
{
    /// <summary>
    ///     XML Extension Methods
    /// </summary>
    public static class XmlExtensions
    {
        /// <summary>
        ///     Returns the name of an element with the namespace if specified
        /// </summary>
        /// <param name="name">Element name</param>
        /// <param name="namespace">XML Namespace</param>
        /// <returns></returns>
        public static XName AsNamespaced(this string name, string @namespace)
        {
            XName xName = name;

            if (@namespace.HasValue()) xName = XName.Get(name, @namespace);

            return xName;
        }
    }
}