using Conversation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Conversation
{
    public interface ILocalizationEngine
    {
        Tuple<Id<LocalizedText>, SimpleUndoPair> DuplicateActions(Id<LocalizedText> id);

        /// <summary>
        /// Does the localization engine provide the facility store a localization for this string
        /// for the current localization set regardless of whether a localization currently exists.
        /// </summary>
        bool CanLocalize(Id<LocalizedStringType> localizedStringType);

        string Localize(Id<LocalizedStringType> type, Id<LocalizedText> id);

        SimpleUndoPair SetLocalizationAction(Id<LocalizedStringType> type, Id<LocalizedText> id, string value);

        SimpleUndoPair ClearLocalizationAction(Id<LocalizedStringType> type, Id<LocalizedText> id);

        IEnumerable<Tuple<Id<LocalizedStringType>, Id<LocalizedText>>> ExistingLocalizations { get; }
    }
}
