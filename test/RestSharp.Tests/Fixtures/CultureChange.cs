using System;
using System.Globalization;
using System.Threading;
using RestSharp.Validation;

namespace RestSharp.Tests.Fixtures
{
    public class CultureChange : IDisposable
    {
        public CultureChange(string culture)
        {
            Ensure.NotEmpty(culture, nameof(culture));

            PreviousCulture = Thread.CurrentThread.CurrentCulture;

            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
        }

        public CultureInfo PreviousCulture { get; private set; }

        public void Dispose()
        {
            if (PreviousCulture == null) return;

            Thread.CurrentThread.CurrentCulture = PreviousCulture;

            PreviousCulture = null;
        }
    }
}