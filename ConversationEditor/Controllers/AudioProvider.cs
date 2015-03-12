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

        /// <summary>
        /// Determine a complete list of audio entities which are referenced by nodes
        /// </summary>
        IEnumerable<Audio> UsedAudio();

        /// <summary>
        /// Add this audio to the project if it is referenced by a node
        /// Update can be delayed using SuppressUpdates()
        /// </summary>
        void UpdateUsage(Audio audio);

        /// <summary>
        /// Add all audio that the input node refers to to the project
        /// Update can be delayed using SuppressUpdates()
        /// </summary>
        void UpdateUsage(ConversationNode<INodeGUI> n);

        /// <summary>
        /// Add all required audio files to the project
        /// Update can be delayed using SuppressUpdates()
        /// </summary>
        void UpdateUsage();

        IDisposable SuppressUpdates();
    }

    public class NoAudio : IAudioProvider
    {
        void IAudioProvider.Play(Audio guid) { }
        void IAudioProvider.Play(AudioFile file) { }
        IProjectElementList<AudioFile, IAudioFile> IAudioProvider.AudioFiles { get { throw new NotSupportedException(); } }
        Audio IAudioProvider.Generate(AudioGenerationParameters parameters) { throw new NotSupportedException(); }
        IEnumerable<Audio> IAudioProvider.UsedAudio() { return Enumerable.Empty<Audio>(); }
        void IAudioProvider.UpdateUsage(Audio audio) { }
        void IAudioProvider.UpdateUsage(ConversationNode<INodeGUI> n) { }
        public static readonly NoAudio Instance = new NoAudio();

        public void UpdateUsage() { }

        private class Disposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
        public IDisposable SuppressUpdates()
        {
            return new Disposable();
        }
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

                if (conversationPath.StartsWith("Resources\\Conversations\\"))
                    conversationPath = conversationPath.Substring("Resources\\Conversations\\".Length, conversationPath.Length - "Resources\\Conversations\\".Length);
                return new Audio("Resources\\Audio\\" + conversationPath + "\\" + filename + ".ogg");
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
            UpdateQueued = new SuppressibleAction(() => { ReallyUpdateUsage(); }); //For now just update everything
        }

        HashSet<FileInfo> m_toUpdate = new HashSet<FileInfo>();
        bool m_updateAll = false;
        public IDisposable SuppressUpdates()
        {
            return UpdateQueued.SuppressCallback();
        }
        SuppressibleAction UpdateQueued;

        FileInfo GetPath(Audio audio)
        {
            //Must be an absolute path or the FileInfo constructor will treat it as relative to current working dir as opposed to project dir
            //return new FileInfo(audio.Value);
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
            return nodes.SelectMany(UsedAudio);
        }

        public IEnumerable<Audio> UsedAudio(ConversationNode<INodeGUI> n)
        {
            var audioParameters = n.Parameters.OfType<IAudioParameter>();
            var audioValues = audioParameters.Select(a => a.Value);
            return audioValues;
        }

        private bool Matches(Audio audio, IAudioFile file)
        {
            return GetPath(audio).FullName == file.File.File.FullName;
        }

        public void UpdateUsage(Audio audio)
        {
            if (audio.Value == null) //As far as I can tell, only a default constructed Audio can have this property
                return;

            m_toUpdate.Add(GetPath(audio));
            UpdateQueued.TryExecute();
        }

        public void UpdateUsage(ConversationNode<INodeGUI> n)
        {
            using (SuppressUpdates())
            {
                foreach (var audio in UsedAudio(n))
                    UpdateUsage(audio);
            }
        }

        public void UpdateUsage()
        {
            //Func<IEnumerable<FileInfo>, int> CalculateAudioFilesHash = files =>
            //{
            //    int hash = 0;
            //    foreach (var file in files)
            //    {
            //        unchecked
            //        {
            //            //Accumulate all the hashes using addition. I didn't want to use xor because I intuite that it might be quite sensitive to
            //            //false matches (an unintentional birthday attack). However I did need something that was commutative so that the order of
            //            //the lists doesn't matter. Unchecked signed integer addition seems to use modulo arithmetic and so is commutative over addition
            //            hash += file.FullName.GetHashCode();
            //        }
            //    }
            //    return hash;
            //};

            //int oldHash = CalculateAudioFilesHash(AudioFiles.Select(a => a.File.File));
            //int newHash = CalculateAudioFilesHash(UsedAudio().Select(GetPath));

            //We make the (potentially incorrect) assumption that if the hash of all the files hasn't changed
            //Then the list of files hasn't changed. This will probably always hold but due to the primitive
            //hashing function there could be some peculiar cases.
            //if (oldHash != newHash)
            //{
            m_updateAll = true;
            UpdateQueued.TryExecute();
            //return true;
            //}
            //else
            //{
            //    return false;
            //}
        }

        private void ReallyUpdateUsage()
        {
            if (m_updateAll)
            {
                m_project.AudioFiles.Load(UsedAudio().Select(GetPath));
            }
            else if (m_toUpdate.Any())
            {
                m_project.AudioFiles.Load(m_toUpdate);
            }

            m_updateAll = false;
            m_toUpdate.Clear();
        }

        public Audio Generate(AudioGenerationParameters parameters)
        {
            return m_customization.Generate(parameters);
        }
    }
}
