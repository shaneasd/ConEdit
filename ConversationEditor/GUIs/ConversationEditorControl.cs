using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using ConversationNode = Conversation.ConversationNode<Conversation.INodeGUI>;

namespace ConversationEditor
{
    public class ConversationEditorControl : GraphEditorControl<ConversationNode>
    {
        protected override bool IsDomainEditor { get { return false; } }
    }
}
