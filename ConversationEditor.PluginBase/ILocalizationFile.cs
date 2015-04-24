using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using Conversation;

namespace ConversationEditor
{
    public interface ILocalizationFile : IInProject, ISaveableFileProvider
    {
        string Localize(ID<LocalizedText> id);
        SimpleUndoPair SetLocalizationAction(ID<LocalizedText> guid, string p);
        /// <summary>
        /// Will this localizer localize to real user specified data?
        /// </summary>
        bool IsValid { get; }
    }
}
