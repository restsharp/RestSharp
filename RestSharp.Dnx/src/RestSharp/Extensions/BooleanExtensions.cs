#if DNXCORE50
using System;
using System.Globalization;

namespace RestSharp.Extensions
{
    public static class BooleanExtensions
    {
        public static string ToString(this Boolean source, CultureInfo culture)
        {
            return source.ToString();
        }
    }
}
#endif
