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
        string Localize(Id<LocalizedText> id);
        DateTime LocalizationTime(Id<LocalizedText> id);
        SimpleUndoPair SetLocalizationAction(Id<LocalizedText> guid, string p);
        /// <summary>
        /// Will this localizer localize to real user specified data?
        /// </summary>
        bool IsValid { get; }

        IEnumerable<Id<LocalizedText>> ExistingLocalizations { get; }
    }
}
