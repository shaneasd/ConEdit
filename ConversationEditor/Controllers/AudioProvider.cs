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
        public AudioGenerationParameters(ISaveableFileProvider file, IProject project, IEnumerable<Parameter> parameters, Func<ID<LocalizedText>, string> localize)
        {
            File = file;
            Project = project;
            Parameters = parameters.ToArray();
            Localize = localize;
        }
        public readonly ISaveableFileProvider File;
        public readonly IProject Project;
        public readonly Parameter[] Parameters;
        public readonly Func<ID<LocalizedText>, string> Localize;
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
            m_updateAll = true;
            UpdateQueued.TryExecute();
        }

        private void ReallyUpdateUsage()
        {
            if (m_updateAll)
            {
                var fileInfoComparer = new GenericEqualityComparer<FileInfo>((a, b) => a.FullName == b.FullName, a => a.FullName.GetHashCode());
                HashSet<FileInfo> usedAudioPaths = new HashSet<FileInfo>(fileInfoComparer);
                HashSet<FileInfo> existing = new HashSet<FileInfo>(AudioFiles.Select(a => a.File.File), fileInfoComparer);

                List<FileInfo> toLoad = new List<FileInfo>();
                foreach (var path in UsedAudio().Select(GetPath))
                {
                    if (!usedAudioPaths.Add(path))
                    {
                        MessageBox.Show("The path " + path + " is refered to by more than one audio parameter!", "Duplicate audio field", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                    }
                    else
                    {
                        if (!existing.Contains(path))
                            toLoad.Add(path);
                    }
                }

                if (toLoad.Any())
                {
                    m_project.AudioFiles.Load(toLoad);
                }
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
