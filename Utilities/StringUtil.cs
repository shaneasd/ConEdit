using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace Utilities
{
    public static class StringUtil
    {
        public static int WordCount(string text)
        {
            int count = 0;
            bool whitespace = true;
            for (int i = 0; i < text.Length; i++)
            {
                bool next = char.IsWhiteSpace(text[i]);
                if (!next && whitespace)
                    count++;
                whitespace = next;
            }
            return count;
        }

        /// <summary>
        /// Get the ordered list of "words" in the input text. Each word is either 
        /// A) A contiguous sequence of non-whitespace which in the input is bound by whitespace (or start/end of text)
        /// B) A whitespace character
        /// </summary>
        public static IEnumerable<string> WordsAndWhiteSpace(string text)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (char.IsWhiteSpace(c))
                {
                    if (sb.Length > 0)
                    {
                        yield return sb.ToString();
                        sb.Clear();
                    }
                    yield return c.ToString();
                }
                else
                {
                    sb.Append(c);
                }
            }
            if (sb.Length > 0)
                yield return sb.ToString();
        }

        public static IEnumerable<string> GetLines(string text, Font font, float width, TextFormatFlags format)
        {
            List<string> words = StringUtil.WordsAndWhiteSpace(text).ToList();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < words.Count; i++)
            {
                var word = words[i];
                if (word == "\n") //If it's a newline then return what we have even if we could fit more
                {
                    sb.Append(word);
                    yield return sb.ToString();
                    sb.Clear();
                }
                else if (TextRenderer.MeasureText(word, font, new Size(int.MaxValue, int.MaxValue), format).Width <= width) //It will fit in a line
                {
                    int start = sb.Length;
                    sb.Append(word);
                    if (TextRenderer.MeasureText(sb.ToString(), font, new Size(int.MaxValue, int.MaxValue), format).Width > width) //It won't fit on the last line
                    {
                        sb.Remove(start, word.Length);
                        yield return sb.ToString();
                        sb.Clear();
                        sb.Append(word); //put it on the next line instead
                    }
                }
                else //It won't fit on a whole line so put what we can on this line
                {
                    sb.Append(word);
                    int removed = 0;
                    while (TextRenderer.MeasureText(sb.ToString(), font, new Size(int.MaxValue, int.MaxValue), format).Width > width)
                    {
                        if (sb.Length == 1)
                            break; //We have to put at least one character on the line. This avoids issues with characters that are individually wider that the supported width.
                        sb.Remove(sb.Length - 1, 1);
                        removed++;
                    }
                    yield return sb.ToString();
                    sb.Clear();
                    words[i] = word.Substring(word.Length - removed, removed);
                    i--; //So that we look at what's left of the word
                }
            }
            yield return sb.ToString();
        }

        public static bool IsAcceptablePathChar(char c)
        {
            return !Path.GetInvalidPathChars().Contains(c);
        }
    }
}
