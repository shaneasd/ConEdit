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

        public MissingLocalizationFile(FileInfo file)
        {
            m_file = new MissingFile(file);
        }

        ISaveableFile ISaveableFileProvider.File
        {
            get { return m_file; }
        }

        bool IInProject.CanRemove(Func<bool> prompt)
        {
            //Doesn't care
            return true;
        }

        void IInProject.Removed()
        {
            //Do nothing
        }

        string ILocalizationFile.Localize(ID<LocalizedText> guid)
        {
            throw new NotImplementedException();
        }

        Utilities.SimpleUndoPair ILocalizationFile.SetLocalizationAction(ID<LocalizedText> guid, string p)
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


        public bool IsValid
        {
            get { return false; }
        }
    }
}
