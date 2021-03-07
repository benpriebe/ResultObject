using System;
using System.Globalization;
using System.Threading;

namespace ResultObject.Core
{
    public static class EphemeralUiCulture 
    {
        /// <summary>
        /// Change the CurrentThread.CurrentUICulture and revert back to the previous CultureInfo on dispose.
        /// </summary>
        /// <param name="iso2Locale">The iso2Language-iso2Region locale identifier (e.g. en-au, en-gb)</param>
        public static IDisposable ChangeLocale(string iso2Locale)
        {
            return new ChangeCurrentUiCultureDisposable(iso2Locale);            
        }

        private class ChangeCurrentUiCultureDisposable : IDisposable
        {
            private readonly CultureInfo currentUiCulture;

            public ChangeCurrentUiCultureDisposable(string iso2Locale)
            {
                if (string.IsNullOrWhiteSpace(iso2Locale))
                {
                    return;
                }

                currentUiCulture = Thread.CurrentThread.CurrentUICulture;
                try
                {

                    var temporaryLocale = new CultureInfo(iso2Locale);
                    Thread.CurrentThread.CurrentUICulture = temporaryLocale;
                }
                catch 
                {
                    // swallow exception - we'll fallback to the standard CurrentUICulture when this happens
                }
            }

            public void Dispose()
            {
                Thread.CurrentThread.CurrentUICulture = currentUiCulture;
            }

        }
    }
}