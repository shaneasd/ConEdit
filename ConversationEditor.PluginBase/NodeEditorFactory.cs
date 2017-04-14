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
        public abstract bool WillEdit(Id<NodeTypeTemp> typeId);
        public abstract ConfigureResult2 Edit(IColorScheme scheme, IConversationNodeData node, AudioGenerationParameters audioContext, Func<ParameterType, ParameterEditorSetupData, IParameterEditor> config, ILocalizationEngine localizer, IAudioParameterEditorCallbacks audioProvider, Func<IParameter, string, IEnumerable<string>> autoCompleteSuggestions);
        public abstract string DisplayName { get; }
    }
}
