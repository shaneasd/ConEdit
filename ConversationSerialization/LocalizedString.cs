using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace RuntimeConversation
{
    public class LocalizedString
    {
        public Id<LocalizedText> Id { get { return m_id; } }
        private readonly Id<LocalizedText> m_id;
        public LocalizedString(string value)
        {
            m_id = Id<LocalizedText>.Parse(value);
        }

        public string Localized(Func<Id<LocalizedText>, string> localizer)
        {
            return localizer(Id);
        }
    }
}
