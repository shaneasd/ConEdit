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
    internal class NodeEditorChoice : TypeChoice
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

        internal bool WillEdit(Id<NodeTypeTemp> nodeType)
        {
            return GetEditorFactory().WillEdit(nodeType);
        }

        public NodeEditorFactory GetEditorFactory()
        {
            var constructor = m_type.GetConstructor(new Type[0] );
            return constructor.Invoke(new object[0]) as NodeEditorFactory;
        }

        internal static NodeEditorChoice Default(Id<NodeTypeTemp> guid)
        {
            return new NodeEditorChoice(typeof(DefaultNodeEditorFactory));
        }
    }
}
