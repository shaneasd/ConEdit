using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace ConversationEditor
{
    /// <summary>
    /// Provide an interface for an audio parameter editor to extract information from the system
    /// </summary>
    public interface IAudioParameterEditorCallbacks
    {
        /// <summary>
        /// Play the audio associated with the specified audio value if it exists
        /// </summary>
        void Play(Audio audio);

        /// <summary>
        /// Generate an audio value given the specified contextual information for the value
        /// </summary>
        Audio Generate(AudioGenerationParameters parameters);
    }
}
