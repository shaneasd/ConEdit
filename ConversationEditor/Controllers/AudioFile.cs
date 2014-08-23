using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Utilities;

namespace ConversationEditor
{
    public interface IAudioFile : IInProject, ISaveableFileProvider
    {
        void Play();
    }

    public class AudioFile : IAudioFile
    {
        ReadonlyFileUnmonitored m_file;
        private AudioProvider m_provider;

        public AudioFile(System.IO.FileInfo file, AudioProvider provider)
        {
            m_provider = provider;
            m_file = new ReadonlyFileUnmonitored(file);
        }

        public ISaveableFile File
        {
            get { return m_file; }
        }

        public bool CanRemove(Func<bool> prompt)
        {
            return true; //TODO: prevent removing an audio file that is being used by a conversation
        }

        public void Removed()
        {
            //Do nothing
        }

        public void Play()
        {
            m_provider.Play(this);
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
            File.Dispose();
        }
    }

    public class MissingAudioFile : IAudioFile
    {
        private MissingFile m_file;

        public MissingAudioFile(FileInfo file)
        {
            m_file = new MissingFile(file);
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

        ISaveableFile ISaveableFileProvider.File
        {
            get { return m_file; }
        }

        public void Play()
        {
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
    }
}
