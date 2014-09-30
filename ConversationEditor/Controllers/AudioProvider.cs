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
    public class AudioGenerationParameters
    {
        public AudioGenerationParameters(ISaveableFileProvider file, IProject project)
        {
            File = file;
            Project = project;
        }
        public readonly ISaveableFileProvider File;
        public readonly IProject Project;
    }

    public interface IAudioProvider
    {
        void Play(Audio guid); //A parameter value
        void Play(AudioFile file); //A file

        IProjectElementList<AudioFile, IAudioFile> AudioFiles { get; }

        Audio Generate(AudioGenerationParameters parameters);
        IEnumerable<Audio> UsedAudio();
        void UpdateUsage(Audio audio);
    }

    public class AudioProvider : IAudioProvider
    {
        private class TDefaultCustomization : IAudioProviderCustomization
        {
            public Audio Generate(AudioGenerationParameters parameters)
            {
                var conversationPath = FileSystem.RelativePath(parameters.File.File.File, parameters.Project.File.File.Directory);
                conversationPath = Path.ChangeExtension(conversationPath, null);

                //GetRandomFileName generates 11 random (cryptographically strong) 5 bit characters (abcdefghijklmnopqrstuvwxyz01235)
                var filename = Path.GetRandomFileName();
                //We get rid of the superfluous '.' but might as well keep the extension digits resulting in 8+3=11 characters
                //or 32^11 possible strings.
                filename = Path.GetFileNameWithoutExtension(filename) + Path.GetExtension(filename).Substring(1);
                //Supposing a context were to generate N audio files, the probability of no collision can be calculated as
                // ((32^11-1)/32^11)^(N*(N-1)/2)
                //See:
                //http://math.stackexchange.com/questions/33610/probability-of-duplicate-guid
                //http://en.wikipedia.org/wiki/Birthday_problem
                //For N = 1000 this gives a probability of no collision of 0.9999995 (http://www.wolframalpha.com/input/?i=%28%2832^8-+1%29%2F%2832^8%29%29^%28999000%2F2%29)
                //for roughly a one in a million chance of collision

                return new Audio("Audio\\" + conversationPath + "\\" + filename + ".ogg");
            }


            public string Name
            {
                get { return "Default"; }
            }
        }
        public static IAudioProviderCustomization DefaultCustomization = new TDefaultCustomization();

        ProjectElementList<AudioFile, MissingAudioFile, IAudioFile> m_audioFiles;
        private IProject m_project;
        private DirectoryInfo m_projectPath;
        private IAudioProviderCustomization m_customization;
        public AudioProvider(FileInfo projectPath, Func<string, bool> fileLocationOk, IProject project, IAudioProviderCustomization customization)
        {
            m_projectPath = projectPath.Directory;
            m_project = project;
            m_customization = customization;

            Func<IEnumerable<FileInfo>, IEnumerable<AudioFile>> load = files => files.Select(file => new AudioFile(file, this));
            Func<DirectoryInfo, AudioFile> makeEmpty = path => { throw new NotSupportedException("Can't create new audio files within the editor"); };
            Func<FileInfo, MissingAudioFile> makeMissing = file => new MissingAudioFile(file, this);
            m_audioFiles = new ProjectElementList<AudioFile, MissingAudioFile, IAudioFile>(fileLocationOk, load, makeEmpty, makeMissing);
        }

        FileInfo GetPath(Audio audio)
        {
            return new FileInfo(Path.Combine(m_projectPath.FullName, audio.Value));
        }

        public void Play(Audio audio)
        {
            FileInfo file = GetPath(audio);
            if (file.Exists)
                Process.Start(file.FullName);
            else
                MessageBox.Show("Audio file does not exist");
        }

        public void Play(AudioFile audio)
        {
            Process.Start(audio.File.File.FullName);
            //Process.Start(m_mediaPlayerPath(), "\"" +  audio.File.File.FullName + "\"");
        }

        public IProjectElementList<AudioFile, IAudioFile> AudioFiles
        {
            get { return m_audioFiles; }
        }

        //Will have to cache this one day
        public IEnumerable<Audio> UsedAudio()
        {
            var conversations = m_project.Conversations;
            var nodes = conversations.SelectMany(c => c.Nodes);
            var parameters = nodes.SelectMany(n => n.Parameters);
            var audioParameters = parameters.OfType<IAudioParameter>();
            var audioValues = audioParameters.Select(a => a.Value);
            return audioValues;
        }

        private bool Matches(Audio audio, IAudioFile file)
        {
            return GetPath(audio).FullName == file.File.File.FullName;
        }

        public void UpdateUsage(Audio audio)
        {
            bool used = UsedAudio().Contains(audio);
            IAudioFile match = m_project.AudioFiles.FirstOrDefault(f => Matches(audio, f));

            if (used && match == null)
                m_project.AudioFiles.Load(GetPath(audio).Only());
            else if (!used && match != null)
                m_project.AudioFiles.Remove(match, true);
        }

        public Audio Generate(AudioGenerationParameters parameters)
        {
            return m_customization.Generate(parameters);
        }
    }
}
