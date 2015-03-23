using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConversationEditor
{
    public class DummyLocalizationFile : ILocalizationFile
    {
        public static readonly DummyLocalizationFile Instance = new DummyLocalizationFile();
        private DummyLocalizationFile() { }

        public string Localize(Conversation.ID<Conversation.LocalizedText> id)
        {
            return LocalizationEngine.MISSING_LOCALIZATION;
        }

        public Utilities.SimpleUndoPair SetLocalizationAction(Conversation.ID<Conversation.LocalizedText> guid, string p)
        {
            throw new NotSupportedException("Attempting to modify localization values of a dummy localization file");
        }

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


        public bool IsValid
        {
            get { return false; }
        }
    }
}
