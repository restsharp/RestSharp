using System;

namespace RestSharp.Extensions
{
    public static class UriHelper
    {
        /// <summary>
        /// Determines whether a specified character is a valid hexadecimal digit.
        /// </summary>
        /// 
        /// <returns>
        /// A <see cref="T:System.Boolean"/> value that is true if the character is a valid hexadecimal digit; otherwise false.
        /// </returns>
        /// <param name="character">The character to validate. </param>
        public static bool IsHexDigit(char character)
        {
#if WINDOWS_UWP || DNXCORE50
            if (character >= 48 && character <= 57 || character >= 65 && character <= 70)
                return true;

            if (character >= 97)
                return character <= 102;

            return false;
#else
            return Uri.IsHexDigit(character);
#endif
        }

        public static int FromHex(char digit)
        {
#if WINDOWS_UWP || DNXCORE50
            if (((digit >= '0') && (digit <= '9'))
                || ((digit >= 'A') && (digit <= 'F')) 
                || ((digit >= 'a') && (digit <= 'f')))
            {
                return  (digit <= '9')
                    ? ((int)digit - (int)'0')
                    : (((digit <= 'F') 
                    ? ((int)digit - (int)'A')
                    : ((int)digit - (int)'a')) 
                    + 10); 
            }

            throw new ArgumentException("digit");
#else
            return Uri.FromHex(digit);
#endif
        }
    }
}