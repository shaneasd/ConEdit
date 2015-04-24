using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace ConversationEditor
{
    //TODO: rename
    public interface IAudioProvider2
    {
        void Play(Audio guid); //A parameter value
        void Play(IAudioFile file); //A file
        Audio Generate(AudioGenerationParameters parameters);

        /// <summary>
        /// Add this audio to the project if it is referenced by a node
        /// Update can be delayed using SuppressUpdates()
        /// </summary>
        void UpdateUsage(Audio audio);
    }
}
