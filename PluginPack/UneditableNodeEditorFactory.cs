using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConversationEditor;
using Conversation;
using System.Windows.Forms;
using Utilities;

namespace PluginPack
{
    public class UneditableNodeEditorFactory : NodeEditorFactory
    {
        public override bool WillEdit(Id<NodeTypeTemp> typeId)
        {
            return true;
        }

        public override Conversation.ConfigureResult2 Edit(IColorScheme scheme, Conversation.IConversationNodeData node, AudioGenerationParameters audioContext, Func<ParameterType, ParameterEditorSetupData, IParameterEditor> config, ILocalizationEngine localizer, IAudioParameterEditorCallbacks audioProvider, Func<IParameter, string, IEnumerable<string>> autoCompleteSuggestions)
        {
            return ConfigureResult2.NotApplicable;
        }

        public override string DisplayName
        {
            get { return "Uneditable"; }
        }
    }
}
