﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Conversation;
using Utilities;

namespace ConversationEditor
{
    internal sealed class MissingLocalizationFile : ILocalizationFile
    {
        private MissingFile m_file;

        public Id<FileInProject> Id { get ; }

        public MissingLocalizationFile(Id<FileInProject> file, DocumentPath path)
        {
            m_file = new MissingFile(file, path);
            Id = file;
        }

        ISaveableFile ISaveableFileProvider.File => m_file;

        bool IInProject.CanRemove(Func<bool> prompt)
        {
            //Doesn't care
            return true;
        }

        void IInProject.Removed()
        {
            //Do nothing
        }

        string ILocalizationFile.Localize(Id<LocalizedText> guid)
        {
            throw new NotImplementedException();
        }

        void ILocalizationFile.ImportInto(string[] fileNames, DirectoryInfo origin)
        {
            throw new NotImplementedException();
        }

        public DateTime LocalizationTime(Id<LocalizedText> id)
        {
            throw new NotImplementedException();
        }
        
        Utilities.SimpleUndoPair ILocalizationFile.SetLocalizationAction(Id<LocalizedText> guid, string p)
        {
            throw new NotImplementedException();
        }

        Utilities.SimpleUndoPair ILocalizationFile.ClearLocalizationAction(Id<LocalizedText> guid)
        {
            throw new NotImplementedException();
        }

        public event Action FileModifiedExternally
        {
            add { (this as ISaveableFileProvider).File.FileModifiedExternally += value; }
            remove { (this as ISaveableFileProvider).File.FileModifiedExternally -= value; }
        }

        public event Action FileDeletedExternally
        {
            add { (this as ISaveableFileProvider).File.FileDeletedExternally += value; }
            remove { (this as ISaveableFileProvider).File.FileDeletedExternally -= value; }
        }

        public void Dispose()
        {
            m_file.Dispose();
        }

        public bool IsValid => false;

        public IEnumerable<Id<LocalizedText>> ExistingLocalizations => Enumerable.Empty<Id<LocalizedText>>();
    }
}
