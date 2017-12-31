using Conversation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ConversationEditor
{
    internal sealed class DummyLocalizationFile : ILocalizationFile
    {
        public static readonly DummyLocalizationFile Instance = new DummyLocalizationFile();
        private DummyLocalizationFile() { }

        public string Localize(Id<LocalizedText> id)
        {
            return LocalizationEngine.MissingLocalizationString;
        }

        public Utilities.SimpleUndoPair SetLocalizationAction(Id<LocalizedText> guid, string p)
        {
            throw new NotSupportedException("Attempting to modify localization values of a dummy localization file");
        }

        Utilities.SimpleUndoPair ILocalizationFile.ClearLocalizationAction(Id<LocalizedText> guid)
        {
            throw new NotSupportedException("Attempting to modify localization values of a dummy localization file");
        }

        IEnumerable<Id<LocalizedText>> ILocalizationFile.ExistingLocalizations => throw new NotSupportedException("ExistingLocalizations");

        void ILocalizationFile.ImportInto(string[] fileNames, DirectoryInfo origin)
        {
            throw new NotImplementedException("ExistingLocalizations");
        }

        public bool CanRemove(Func<bool> prompt)
        {
            throw new NotSupportedException(); //Can never be in a project
        }

        public void Removed()
        {
            throw new NotSupportedException(); // Can never be in a project
        }

        public Utilities.ISaveableFile File => null; //No corresponding file

        public event Action FileModifiedExternally { add { } remove { } } //Doesn't exist on disk

        public event Action FileDeletedExternally { add { } remove { } } //Doesn't exist on disk

        public void Dispose() { } //No resources to clean up

        public DateTime LocalizationTime(Id<LocalizedText> id)
        {
            throw new NotImplementedException();
        }

        public bool IsValid => false;

        Id<FileInProject> IInProject.Id => throw new NotImplementedException();
    }
}
