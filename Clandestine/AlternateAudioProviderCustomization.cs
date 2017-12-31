using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConversationEditor;

namespace PluginPack
{
    public class AlternateAudioProviderCustomization : IAudioProviderCustomization
    {
        public Conversation.Audio Generate(AudioGenerationParameters parameters)
        {
            return new Conversation.Audio("AlternateAudioPath");
        }

        public string Name => "Alternate";
    }
}
