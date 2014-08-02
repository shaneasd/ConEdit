using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Conversation;
using System.Windows;
using System.IO;
using Utilities;

namespace ConversationEditor
{
    public interface IAudioProvider
    {
        void Play(Audio guid); //A parameter value
        void Play(AudioFile file); //A file

        IProjectElementList<AudioFile, IAudioFile> AudioFiles { get; }
    }

    public class AudioProvider : IAudioProvider
    {
        private DirectoryInfo m_projectPath;
        public AudioProvider(FileInfo projectPath, Func<string, bool> fileLocationOk)
        {
            m_projectPath = projectPath.Directory;

            Func<IEnumerable<FileInfo>, IEnumerable<AudioFile>> load = files => files.Select(file => new AudioFile(file, this));
            Func<DirectoryInfo, AudioFile> makeEmpty = path => { throw new NotSupportedException("Can't create new audio files within the editor"); };
            Func<FileInfo, MissingAudioFile> makeMissing = file => new MissingAudioFile(file);
            m_audioFiles = new ProjectElementList<AudioFile, MissingAudioFile, IAudioFile>(fileLocationOk, load, makeEmpty, makeMissing);
        }

        public void Play(Audio audio)
        {
            string path = Path.Combine(m_projectPath.FullName, audio.Value);
            if (File.Exists(path))
                Process.Start(path);
            else
                MessageBox.Show("Audio file does not exist");
        }

        public void Play(AudioFile audio)
        {
            Process.Start(audio.File.File.FullName);
            //Process.Start(m_mediaPlayerPath(), "\"" +  audio.File.File.FullName + "\"");
        }


        ProjectElementList<AudioFile, MissingAudioFile, IAudioFile> m_audioFiles;

        public IProjectElementList<AudioFile, IAudioFile> AudioFiles
        {
            get { return m_audioFiles; }
        }
    }
}
