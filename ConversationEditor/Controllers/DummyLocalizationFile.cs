using Conversation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConversationEditor
{
    internal sealed class DummyLocalizationFile : ILocalizationFile
    {
        public static readonly DummyLocalizationFile Instance = new DummyLocalizationFile();
        private DummyLocalizationFile() { }

        public string Localize(Conversation.Id<Conversation.LocalizedText> id)
        {
            return LocalizationEngine.MISSING_LOCALIZATION;
        }

        public Utilities.SimpleUndoPair SetLocalizationAction(Conversation.Id<Conversation.LocalizedText> guid, string p)
        {
            throw new NotSupportedException("Attempting to modify localization values of a dummy localization file");
        }

         IEnumerable<Id<LocalizedText>> ILocalizationFile.ExistingLocalizations { get { throw new NotImplementedException("ExistingLocalizations"); } }

        public bool CanRemove(Func<bool> prompt)
        {
            throw new NotSupportedException(); //Can never be in a project
        }

        public void Removed()
        {
            throw new NotSupportedException(); // Can never be in a project
        }

        public Utilities.ISaveableFile File
        {
            get { return null; } //No corresponding file
        }

        public event Action FileModifiedExternally { add { } remove { } } //Doesn't exist on disk

        public event Action FileDeletedExternally { add { } remove { } } //Doesn't exist on disk

        public void Dispose() { } //No resources to clean up

        public DateTime LocalizationTime(Id<LocalizedText> id)
        {
            return Instance.LocalizationTime(id);
        }

        public bool IsValid
        {
            get { return false; }
        }
    }
}
