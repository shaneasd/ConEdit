using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace ConversationEditor
{
    internal interface IAudioProvider : IAudioProvider2
    {
        IProjectElementList<AudioFile, IAudioFile> AudioFiles { get; }

        /// <summary>
        /// Determine a complete list of audio entities which are referenced by nodes
        /// </summary>
        IEnumerable<Audio> UsedAudio();

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
}
