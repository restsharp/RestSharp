using System;
using System.Globalization;
using System.Threading;

namespace RestSharp.Tests
{
    public class CultureChange : IDisposable
    {
        public CultureInfo PreviousCulture { get; private set; }

        public CultureChange(string culture)
        {
            if (culture == null)
            {
                throw new ArgumentNullException("culture");
            }

            this.PreviousCulture = Thread.CurrentThread.CurrentCulture;

            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
        }

        public void Dispose()
        {
            if (this.PreviousCulture != null)
            {
                Thread.CurrentThread.CurrentCulture = this.PreviousCulture;

                this.PreviousCulture = null;
            }
        }
    }
}
