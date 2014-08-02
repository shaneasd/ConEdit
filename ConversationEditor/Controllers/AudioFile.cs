using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ConversationEditor
{
    public interface IAudioFile : IInProject, ISaveableFileProvider
    {
        void Play();
    }

    public class AudioFile : IAudioFile
    {
        ReadonlyFile m_file;
        private AudioProvider m_provider;

        public AudioFile(System.IO.FileInfo file, AudioProvider provider)
        {
            m_provider = provider;
            m_file = new ReadonlyFile(file);
        }

        public ISaveableFile File
        {
            get { return m_file; }
        }

        public void Removed()
        {
            //Doesn't care
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
        void IInProject.Removed()
        {
            //Doesn't care
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
