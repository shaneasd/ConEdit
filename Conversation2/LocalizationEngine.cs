using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace Conversation
{
    //TODO: Move back into conversation editor
    public class LocalizationEngine
    {
        private Func<ILocalizer> m_l;
        public LocalizationEngine(Func<ILocalizer> l)
        {
            m_l = l;
        }

        public LocalizationEngine(ProjectElementList<LocalizationFile, global::ConversationEditor.MissingLocalizationFile, global::ConversationEditor.ILocalizationFile> m_localizers)
        {
            // TODO: Complete member initialization
            this.m_localizers = m_localizers;
        }

        public string this[Guid guid]
        {
            get
            {
                return m_l()[guid].Text;
            }
        }

        public string Localize(Guid guid)
        {
            return m_l()[guid].Text;
        }
    }
}
