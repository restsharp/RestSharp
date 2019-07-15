using System;

namespace RestSharp.Validation
{
    public static class Ensure
    {
        public static void NotNull(object parameter, string name)
        {
            if (parameter == null)
                throw new ArgumentNullException(name);
        }
    }
}