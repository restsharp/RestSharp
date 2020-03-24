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
using System.Net;
using static System.Net.WebExceptionStatus;

namespace RestSharp.Extensions
{
    public static class ResponseStatusExtensions
    {
        /// <summary>
        ///     Convert a <see cref="ResponseStatus" /> to a <see cref="WebException" /> instance.
        /// </summary>
        /// <param name="responseStatus">The response status.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">responseStatus</exception>
        public static WebException ToWebException(this ResponseStatus responseStatus)
            => responseStatus switch
            {
                ResponseStatus.None     => new WebException("The request could not be processed.", ServerProtocolViolation),
                ResponseStatus.Error    => new WebException("An error occurred while processing the request.", ServerProtocolViolation),
                ResponseStatus.TimedOut => new WebException("The request timed-out.", Timeout),
                ResponseStatus.Aborted  => new WebException("The request was aborted.", Timeout),
                _                       => throw new ArgumentOutOfRangeException(nameof(responseStatus))
            };
    }
}