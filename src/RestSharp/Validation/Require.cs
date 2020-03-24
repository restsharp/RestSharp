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

namespace RestSharp.Validation
{
    /// <summary>
    ///     Helper methods for validating required values
    /// </summary>
    public class Require
    {
        /// <summary>
        ///     Require a parameter to not be null
        /// </summary>
        /// <param name="argumentName">Name of the parameter</param>
        /// <param name="argumentValue">Value of the parameter</param>
        [Obsolete("This method will be removed soon. If you use it, please copy the code to your project.")]
        public static void Argument(string argumentName, object argumentValue)
        {
            if (argumentValue == null) throw new ArgumentException("Argument cannot be null.", argumentName);
        }
    }
}