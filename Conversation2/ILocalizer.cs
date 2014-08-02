using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public struct LocalizationElement
    {
        public readonly string Text;
        public readonly DateTime Localized;
        public LocalizationElement(DateTime localized, string text)
        {
            Localized = localized;
            Text = text;
        }
    }

    public sealed class LocalizedText { };

    public interface ILocalizer
    {
        bool Contains(ID<LocalizedText> guid);
        LocalizationElement this[ID<LocalizedText> guid] { get; set; }
    }
}
