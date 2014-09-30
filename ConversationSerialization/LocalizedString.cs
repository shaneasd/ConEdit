using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace RuntimeConversation
{
    public class LocalizedString
    {
        public readonly ID<LocalizedText> Id;
        public LocalizedString(string value)
        {
            Id = ID<LocalizedText>.Parse(value);
        }

        public string Localized(Func<ID<LocalizedText>, string> localizer)
        {
            return localizer(Id);
        }
    }

    public class Audio
    {
        public readonly string Path;
        public Audio(string value)
        {
            Path = value;
        }
    }
}
