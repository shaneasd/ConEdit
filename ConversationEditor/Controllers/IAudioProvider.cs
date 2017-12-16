using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;

namespace ConversationEditor
{
    public interface IAudioLibrary : IAudioParameterEditorCallbacks
    {
        /// <summary>
        /// Play the audio associated with the specified audio file
        /// </summary>
        void Play(IAudioFile file);

        IProjectElementList<IAudioFile> AudioFiles { get; }

        /// <summary>
        /// Determine a complete list of audio entities which are referenced by nodes
        /// </summary>
        IEnumerable<Audio> UsedAudio();

        /// <summary>
        /// Add all audio that the input node refers to to the project
        /// Update can be delayed using SuppressUpdates()
        /// </summary>
        void UpdateUsage(ConversationNode<INodeGui> n);

        /// <summary>
        /// Add all required audio files to the project
        /// Update can be delayed using SuppressUpdates()
        /// </summary>
        void UpdateUsage();

        /// <summary>
        /// Add this audio to the project if it is referenced by a node
        /// Update can be delayed using SuppressUpdates()
        /// </summary>
        void UpdateUsage(Audio audio);

        IDisposable SuppressUpdates();
    }

    internal class DummyAudioLibrary : IAudioLibrary
    {
        public static readonly IAudioLibrary Instance = new DummyAudioLibrary();

        public void Play(IAudioFile file)
        {
            throw new NotImplementedException();
        }

        public void Play(Audio value)
        {
            throw new NotImplementedException();
        }

        public IProjectElementList<IAudioFile> AudioFiles
        {
            get { return DummyProjectElementList<AudioFile, IAudioFile>.Instance; }
        }

        public IEnumerable<Audio> UsedAudio()
        {
            return Enumerable.Empty<Audio>();
        }

        public void UpdateUsage(ConversationNode<INodeGui> n)
        {
        }

        public void UpdateUsage()
        {
        }

        public void UpdateUsage(Audio audio)
        {
        }

        private class IrrelevantDisposable : Disposable
        {
            protected override void Dispose(bool disposing)
            {
            }
        }

        public IDisposable SuppressUpdates()
        {
            return new IrrelevantDisposable();
        }

        public Audio Generate(AudioGenerationParameters parameters)
        {
            throw new NotImplementedException();
        }

        public void Rename(string from, string to)
        {
            throw new NotImplementedException();
        }
    }
}
