using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Conversation;

namespace ConversationEditor
{
    public interface IParameterEditorFactory
    {
        bool WillEdit(ParameterType type, WillEdit queries);
        string Name { get; }
        Guid Guid { get; }
        IParameterEditor<Control> Make(ColorScheme scheme);
    }
}
