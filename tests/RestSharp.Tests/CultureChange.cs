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

            this.PreviousCulture = CultureInfo.CurrentCulture;

            CultureInfo.CurrentCulture = new CultureInfo(culture);
        }

        public void Dispose()
        {
            if (this.PreviousCulture != null)
            {
                CultureInfo.CurrentCulture = this.PreviousCulture;

                this.PreviousCulture = null;
            }
        }
    }
}
