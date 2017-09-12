#region License

//   Copyright 2010 John Sheehan
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

#endregion

using System;
using System.Globalization;

namespace RestSharp.Extensions
{
    /// <summary>
    /// Parsing that is compatbile with Compact Framework
    /// </summary>
    public static class TryParse
    {
        public static bool UInt32(
            string s,
            NumberStyles style,
            IFormatProvider provider,
            out UInt32 result)
        {
#if PocketPC
            try {
                result = System.UInt32.Parse(s, style, provider);
                return true;
            } catch {
                result = 0;
            }
            return false;
#else
            return System.UInt32.TryParse(s, style, provider, out result);
#endif
        }

        public static bool Long(
            string s,
            out long result)
        {
#if PocketPC
            try {
                result = long.Parse(s);
                return true;
            } catch {
                result = 0;
            }
            return false;
#else
            return long.TryParse(s, out result);
#endif
        }

        public static bool Long(
            string s,
            NumberStyles style,
            IFormatProvider provider,
            out long result)
        {
#if PocketPC
            try {
                result = long.Parse(s, style, provider);
                return true;
            } catch {
                result = 0;
            }
            return false;
#else
            return long.TryParse(s, style, provider, out result);
#endif
        }

        public static bool Double(
            string s, 
            NumberStyles style, 
            IFormatProvider provider, 
            out double result)
        {
#if PocketPC
            try {
                result = double.Parse(s, style, provider);
                return true;
            } catch {
                result = 0;
            }
            return false;
#else
            return double.TryParse(s, style, provider, out result);
#endif
        }

        public static bool DateTime(
            string s, 
            IFormatProvider provider, 
            DateTimeStyles styles, 
            out DateTime result)
        {
#if PocketPC
            try {
                result = System.DateTime.Parse(s, provider, styles);
                return true;
            } catch {
                result = new DateTime();
            }
            return false;
#else
            return System.DateTime.TryParse(s, provider, styles, out result);
#endif
        }

        public static bool DateTimeExact(
            string s, 
            string[] formats, 
            IFormatProvider provider, 
            DateTimeStyles style, 
            out DateTime result)
        {
#if PocketPC
            try {
                result = System.DateTime.ParseExact(s, formats, provider, style);
                return true;
            } catch {
                result = new DateTime();
            }
            return false;
#else
            return System.DateTime.TryParseExact(s, formats, provider, style, out result);
#endif
        }

        public static bool TimeSpan(
            string s,
            out TimeSpan result)
        {
#if PocketPC
            try {
                result = System.TimeSpan.Parse(s);
                return true;
            } catch {
                result = new TimeSpan();
            }
            return false;
#else
            return System.TimeSpan.TryParse(s, out result);
#endif
        }
    }
}
