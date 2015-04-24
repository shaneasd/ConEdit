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
        public override bool WillEdit(ID<NodeTypeTemp> typeId)
        {
            return true;
        }

        public override Conversation.ConfigureResult Edit(ConversationEditor.ColorScheme scheme, Conversation.IEditable node, AudioGenerationParameters audioContext, Func<ParameterType, ParameterEditorSetupData, IParameterEditor<Control>> config, ILocalizationEngine localizer, IAudioProvider2 audioProvider)
        {
            return ConfigureResult.NotApplicable;
        }

        public override string DisplayName
        {
            get { return "Uneditable"; }
        }
    }
}
