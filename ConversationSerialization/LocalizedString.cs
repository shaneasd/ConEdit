using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace RuntimeConversation
{
    public class LocalizedString
    {
        public Id<LocalizedText> Id { get; }
        public LocalizedString(string value)
        {
            Id = Id<LocalizedText>.Parse(value);
        }

        public string Localized(Func<Id<LocalizedText>, string> localizer)
        {
            return localizer(Id);
        }
    }
}
