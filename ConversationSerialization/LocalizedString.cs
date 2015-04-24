using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace RuntimeConversation
{
    public class LocalizedString
    {
        public ID<LocalizedText> Id { get { return m_id; } }
        private readonly ID<LocalizedText> m_id;
        public LocalizedString(string value)
        {
            m_id = ID<LocalizedText>.Parse(value);
        }

        public string Localized(Func<ID<LocalizedText>, string> localizer)
        {
            return localizer(Id);
        }
    }
}
