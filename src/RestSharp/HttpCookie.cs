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

using System;

namespace RestSharp
{
    /// <summary>
    ///     Representation of an HTTP cookie
    /// </summary>
    public class HttpCookie
    {
        /// <summary>
        ///     Comment of the cookie
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        ///     Comment of the cookie
        /// </summary>
        public Uri CommentUri { get; set; }

        /// <summary>
        ///     Indicates whether the cookie should be discarded at the end of the session
        /// </summary>
        public bool Discard { get; set; }

        /// <summary>
        ///     Domain of the cookie
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        ///     Indicates whether the cookie is expired
        /// </summary>
        public bool Expired { get; set; }

        /// <summary>
        ///     Date and time that the cookie expires
        /// </summary>
        public DateTime Expires { get; set; }

        /// <summary>
        ///     Indicates that this cookie should only be accessed by the server
        /// </summary>
        public bool HttpOnly { get; set; }

        /// <summary>
        ///     Name of the cookie
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Path of the cookie
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        ///     Port of the cookie
        /// </summary>
        public string Port { get; set; }

        /// <summary>
        ///     Indicates that the cookie should only be sent over secure channels
        /// </summary>
        public bool Secure { get; set; }

        /// <summary>
        ///     Date and time the cookie was created
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        ///     Value of the cookie
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        ///     Version of the cookie
        /// </summary>
        public int Version { get; set; }
    }
}