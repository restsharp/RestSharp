using System;
using System.Globalization;

namespace RestSharp.Tests
{
    public class CultureChange : IDisposable
    {
        public CultureInfo PreviousCulture { get; private set; }

        public CultureChange(string culture)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            PreviousCulture = CultureInfo.CurrentCulture;

            SetCurrentCulture(culture);
        }

        public static void SetCurrentCulture(CultureInfo culture)
        {
#if NETCOREAPP1_1
            CultureInfo.CurrentCulture = culture;
#else
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
#endif
        }

        public static void SetCurrentCulture(string culture)
        {
            SetCurrentCulture(new CultureInfo(culture));
        }

        public void Dispose()
        {
            if (PreviousCulture == null) return;
            SetCurrentCulture(PreviousCulture);
            PreviousCulture = null;
        }
    }
}
