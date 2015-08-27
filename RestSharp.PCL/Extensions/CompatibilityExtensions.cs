using System;
using System.Globalization;

namespace RestSharp
{
    /// <summary>
    /// Compatibility extensions used to smooth over PCL incompatibilities
    /// </summary>
    internal static class CompatibilityExtensions
    {
        #if PCL
        internal static void ForEach(this string str, Action<char> action)
        {
            for (int i = 0; i < str.Length; i++)
            {
                action(str[i]);
            }
        }

        internal static bool Contains(this string str, char c)
        {
            return str.Contains(c.ToString());
        }

        internal static string ToLower(this string str, CultureInfo culture)
        {
            //TODO: Add culture support, if possible
            return str.ToLower();
        }

        internal static string ToUpper(this string str, CultureInfo culture)
        {
            //TODO: Add culture support, if possible
            return str.ToUpper();
        }
        
        internal static string ToString(this bool b, CultureInfo culture)
        {
            //TODO: Add culture support, if possible
            return b.ToString();
        }
        #endif
    }
}

