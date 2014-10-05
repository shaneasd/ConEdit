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

        public override Conversation.ConfigureResult Edit(Conversation.IEditable node, AudioGenerationParameters audioContext, Func<ID<ParameterType>, ParameterEditorSetupData, IParameterEditor<Control>> config, LocalizationEngine localizer, IAudioProvider audioProvider)
        {
            MessageBox.Show("This node is configured to be uneditable");
            return ConfigureResult.NotApplicable;
        }

        public override string DisplayName
        {
            get { return "Uneditable"; }
        }
    }
}
