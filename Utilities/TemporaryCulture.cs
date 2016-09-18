using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class TemporaryCulture : Disposable
    {
        public static TemporaryCulture English()
        {
            return new TemporaryCulture("en-US");
        }

        public static TemporaryCulture European()
        {
            return new TemporaryCulture("da-DK");
        }

        private CultureInfo m_culture;

        public TemporaryCulture(string culture)
        {
            m_culture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = new CultureInfo(culture);
        }

        protected override void Dispose(bool disposing)
        {
            //If this was triggered from a finalizer then it's still tempting to reset our state to try to ensure we eventually revert to the current culture
            //however this will result in undefined timing as to when this occurs. Better to have a predictable permanent problem than an intermittent one.
            if (disposing)
            {
                CultureInfo.CurrentCulture = m_culture;
            }
        }
    }
}
