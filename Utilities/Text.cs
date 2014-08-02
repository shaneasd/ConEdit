using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Globalization;

namespace Utilities
{
    public static class Text
    {
        //public static IEnumerable<string> Tokens(this string text)
        //{
        //    StringBuilder b;
        //    for (int i = 0; i < text.Length; i++)
        //    {

        //    }
        //}

        public static string WordWrap(string text, double pixels, Typeface typeFace, float emSize)
        {
            StringBuilder wrappedLines = new StringBuilder();
            
            StringBuilder actualLine = new StringBuilder();
            StringBuilder testLine = new StringBuilder();

            int start = 0;
            for ( int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (char.IsWhiteSpace(c))
                {
                    testLine.Append(text, start, i - start);
                    testLine.Append(c);
                    FormattedText formatted = new FormattedText(testLine.ToString(), CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight, typeFace, emSize, Brushes.Black);
                    if (formatted.Width > pixels)
                    {
                        wrappedLines.Append(actualLine.ToString());
                        actualLine.Clear();
                        testLine.Clear();
                    }
                    else
                    {
                        actualLine.Append(c);
                    }
                    start = i + 1;
                }
            }

            return wrappedLines.ToString();
        }
    }
}
