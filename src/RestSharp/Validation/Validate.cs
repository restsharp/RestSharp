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
    ///     Helper methods for validating values
    /// </summary>
    public class Validate
    {
        /// <summary>
        ///     Validate an integer value is between the specified values (exclusive of min/max)
        /// </summary>
        /// <param name="value">Value to validate</param>
        /// <param name="min">Exclusive minimum value</param>
        /// <param name="max">Exclusive maximum value</param>
        [Obsolete("This method will be removed soon. If you use it, please copy the code to your project.")]
        public static void IsBetween(int value, int min, int max)
        {
            if (value < min || value > max) throw new ArgumentException($"Value ({value}) is not between {min} and {max}.");
        }

        /// <summary>
        ///     Validate a string length
        /// </summary>
        /// <param name="value">String to be validated</param>
        /// <param name="maxSize">Maximum length of the string</param>
        [Obsolete("This method will be removed soon. If you use it, please copy the code to your project.")]
        public static void IsValidLength(string value, int maxSize)
        {
            if (value == null) return;

            if (value.Length > maxSize) throw new ArgumentException($"String is longer than max allowed size ({maxSize}).");
        }
    }
}