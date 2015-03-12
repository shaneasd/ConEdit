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
            return "Missing Localization";
        }

        public Utilities.SimpleUndoPair SetLocalizationAction(Conversation.ID<Conversation.LocalizedText> guid, string p)
        {
            throw new NotImplementedException();
        }

        public bool CanRemove(Func<bool> prompt)
        {
            throw new NotImplementedException();
        }

        public void Removed()
        {
            throw new NotImplementedException();
        }

        public Utilities.ISaveableFile File
        {
            get { throw new NotImplementedException(); }
        }

        public event Action FileModifiedExternally;

        public event Action FileDeletedExternally;

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
