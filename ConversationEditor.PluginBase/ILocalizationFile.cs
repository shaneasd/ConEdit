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
        SimpleUndoPair ClearLocalizationAction(Id<LocalizedText> guid);
        /// <summary>
        /// Will this localizer localize to real user specified data?
        /// </summary>
        bool IsValid { get; }

        IEnumerable<Id<LocalizedText>> ExistingLocalizations { get; }

        /// <summary>
        /// Deserialize localizations from the specified files and import all data into this localization file
        /// Fails if any value in the input files already exists in this localization
        /// </summary>
        /// <param name="fileNames">Paths to the files to load</param>
        void ImportInto(string[] fileNames);
    }
}
