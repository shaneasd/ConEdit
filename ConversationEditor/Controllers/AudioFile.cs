using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Utilities;
using Conversation;

namespace ConversationEditor
{
    public class AudioFile : Disposable, IAudioFile
    {
        ReadonlyFileUnmonitored m_file;
        private AudioProvider m_provider;
        private Audio m_audio;

        public Id<FileInProject> Id { get; }

        public AudioFile(Id<FileInProject> file, DocumentPath path, AudioProvider provider)
        {
            m_provider = provider;
            m_file = new ReadonlyFileUnmonitored(path.FileInfo);
            Id = file;
            m_audio = new Audio(path.RelativePath);
        }

        public ISaveableFile File
        {
            get { return m_file; }
        }

        public bool CanRemove(Func<bool> prompt)
        {
            if (m_provider.UsedAudio().Contains(m_audio))
                return prompt();
            else
                return true;
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_file.Dispose();
            }
        }
    }

    internal sealed class MissingAudioFile : IAudioFile
    {
        private MissingFile m_file;
        private AudioProvider m_provider;
        private Audio m_audio;
        public Id<FileInProject> Id { get; }

        public MissingAudioFile(Id<FileInProject> file, DocumentPath path, AudioProvider provider)
        {
            m_file = new MissingFile(file, path);
            Id = file;
            m_provider = provider;
            m_audio = new Audio(path.RelativePath);
        }

        bool IInProject.CanRemove(Func<bool> prompt)
        {
            if (m_provider.UsedAudio().Contains(m_audio))
                return prompt();
            else
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
