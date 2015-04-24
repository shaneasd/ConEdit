using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using System.Windows.Forms;

namespace ConversationEditor
{
    public abstract class NodeEditorFactory
    {
        public abstract bool WillEdit(ID<NodeTypeTemp> typeId);
        public abstract ConfigureResult Edit(ColorScheme scheme, IEditable node, AudioGenerationParameters audioContext, Func<ParameterType, ParameterEditorSetupData, IParameterEditor<Control>> config, ILocalizationEngine localizer, IAudioProvider2 audioProvider);
        public abstract string DisplayName { get; }
    }
}
