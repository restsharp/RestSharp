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

namespace RestSharp.Extensions;

static class CookieContainerExtensions {
    public static void AddCookies(this CookieContainer cookieContainer, Uri uri, IEnumerable<string> cookiesHeader) {
        foreach (var header in cookiesHeader) {
            try {
                cookieContainer.SetCookies(uri, header);
            }
            catch (CookieException) {
                // Do not fail request if we cannot parse a cookie
            }
        }
    }
}
