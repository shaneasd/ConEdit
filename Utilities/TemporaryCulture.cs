using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class TemporaryCulture : IDisposable
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

        public void Dispose()
        {
            CultureInfo.CurrentCulture = m_culture;
        }
    }
}
