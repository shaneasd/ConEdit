using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Conversation;
using Utilities;
using System.Windows.Forms;

namespace ConversationEditor
{
    public abstract class NodeEditorFactory
    {
        public abstract bool WillEdit(ID<NodeTypeTemp> guid);
        public abstract ConfigureResult Edit(ColorScheme scheme, IEditable node, AudioGenerationParameters audioContext, Func<ParameterType, ParameterEditorSetupData, IParameterEditor<Control>> config, LocalizationEngine localizer, IAudioProvider audioProvider);
        public abstract string DisplayName { get; }
    }

    public class NodeEditorChoice : TypeChoice
    {
        public NodeEditorChoice(Type type)
            : base(type)
        {
        }

        public NodeEditorChoice(string assembly, string type)
            : base(assembly, type)
        {
        }

        public override string DisplayName
        {
            get { return GetEditorFactory().DisplayName; }
        }

        internal bool WillEdit(ID<NodeTypeTemp> nodeType)
        {
            return GetEditorFactory().WillEdit(nodeType);
        }

        public NodeEditorFactory GetEditorFactory()
        {
            var constructor = m_type.GetConstructor(new Type[0] );
            return constructor.Invoke(new object[0]) as NodeEditorFactory;
        }

        internal static NodeEditorChoice Default(ID<NodeTypeTemp> guid)
        {
            return new NodeEditorChoice(typeof(DefaultNodeEditorFactory));
        }
    }
}
