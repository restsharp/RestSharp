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
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Global

namespace RestSharp.Validation
{
    public static class Ensure
    {
        public static void NotNull(object parameter, string name)
        {
            if (parameter == null)
                throw new ArgumentNullException(name);
        }

        public static void NotEmpty(string parameter, string name)
        {
            if (string.IsNullOrWhiteSpace(parameter))
                throw new ArgumentNullException(name);
        }
        
        public static void NotEmptyString(object parameter, string name)
        {
            var s = parameter as string;
            if (string.IsNullOrWhiteSpace(s))
                throw new ArgumentNullException(name);
        }
    }
}