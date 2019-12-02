using System;
using System.Globalization;
using System.Threading;

namespace RestSharp.Tests
{
    public class CultureChange : IDisposable
    {
        public CultureChange(string culture)
        {
            if (culture == null) throw new ArgumentNullException("culture");

            PreviousCulture = Thread.CurrentThread.CurrentCulture;

            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
        }

        public CultureInfo PreviousCulture { get; private set; }

        public void Dispose()
        {
            if (PreviousCulture != null)
            {
                Thread.CurrentThread.CurrentCulture = PreviousCulture;

                PreviousCulture = null;
            }
        }
    }
}