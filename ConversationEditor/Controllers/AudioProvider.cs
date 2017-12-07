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
    internal class NoAudio : IAudioLibrary
    {
        void IAudioParameterEditorCallbacks.Play(Audio guid) { }
        void IAudioLibrary.Play(IAudioFile file) { }
        Audio IAudioParameterEditorCallbacks.Generate(AudioGenerationParameters parameters) { throw new NotSupportedException(); }
        void IAudioLibrary.UpdateUsage(Audio audio) { }

        IProjectElementList<AudioFile, IAudioFile> IAudioLibrary.AudioFiles { get { throw new NotSupportedException(); } }
        IEnumerable<Audio> IAudioLibrary.UsedAudio() { return Enumerable.Empty<Audio>(); }
        void IAudioLibrary.UpdateUsage(ConversationNode<INodeGui> n) { }
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

        public void Rename(string from, string to)
        {
            throw new NotImplementedException();
        }
    }

    public class AudioProvider : IAudioLibrary
    {
        private class TDefaultCustomization : IAudioProviderCustomization
        {
            public Audio Generate(AudioGenerationParameters parameters)
            {
                var conversationPath = FileSystem.RelativePath(parameters.File, parameters.Project.Directory);
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

                if (conversationPath.StartsWith("Resources\\Conversations\\", StringComparison.OrdinalIgnoreCase))
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
        public AudioProvider(FileInfo projectPath, GetFilePath getFilePath, Func<string, bool> fileLocationOk, IProject project, IAudioProviderCustomization customization)
        {
            m_projectPath = projectPath.Directory;
            m_project = project;
            m_customization = customization;

            Func<IEnumerable<Tuple<Id<FileInProject>, DocumentPath>>, IEnumerable<AudioFile>> load = files => files.Select(file => new AudioFile(file.Item1, file.Item2, this));
            Func<DirectoryInfo, AudioFile> makeEmpty = path => { throw new NotSupportedException("Can't create new audio files within the editor"); };
            Func<Id<FileInProject>, MissingAudioFile> makeMissing = file => new MissingAudioFile(file, getFilePath(file), this);
            m_audioFiles = new ProjectElementList<AudioFile, MissingAudioFile, IAudioFile>(getFilePath, fileLocationOk, load, makeEmpty, makeMissing);
            UpdateQueued = new SuppressibleAction(() => { ReallyUpdateUsage(); }); //For now just update everything
        }

        HashSet<FileInfo> m_toUpdate = new HashSet<FileInfo>();
        bool m_updateAll = false; //TODO: LOC: it's used by commented out code which needs to be revived
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

        public void Play(IAudioFile audio)
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

        public IEnumerable<Audio> UsedAudio(ConversationNode<INodeGui> n)
        {
            var audioParameters = n.Data.Parameters.OfType<IAudioParameter>();
            var audioValues = audioParameters.Select(a => a.Value);
            return audioValues;
        }

        public void UpdateUsage(Audio audio)
        {
            if (audio.Value == null) //As far as I can tell, only a default constructed Audio can have this property
                return;

            m_toUpdate.Add(GetPath(audio));
            UpdateQueued.TryExecute();
        }

        public void UpdateUsage(ConversationNode<INodeGui> n)
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
            //TODO: LOC: uncomment this
            //if (m_updateAll)
            //{
            //    HashSet<FileInfo> usedAudioPaths = new HashSet<FileInfo>();
            //    HashSet<FileInfo> existing = new HashSet<FileInfo>(AudioFiles.Select(a => a.File.File));

            //    List<Id<FileInProject>> toLoad = new List<Id<FileInProject>>();
            //    foreach (FileInfo path in UsedAudio().Select(GetPath))
            //    {
            //        if (!usedAudioPaths.Add(path))
            //        {
            //            MessageBox.Show("The path " + path + " is refered to by more than one audio parameter!", "Duplicate audio field", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
            //        }
            //        else
            //        {
            //            if (!existing.Contains(path))
            //                toLoad.Add(path);
            //        }
            //    }

            //    if (toLoad.Any())
            //    {
            //        m_project.AudioFiles.Load(toLoad);
            //    }
            //}
            //else if (m_toUpdate.Any())
            //{
            //    m_project.AudioFiles.Load(m_toUpdate);
            //}

            //m_updateAll = false;
            //m_toUpdate.Clear();
        }

        public Audio Generate(AudioGenerationParameters parameters)
        {
            return m_customization.Generate(parameters);
        }
    }
}
