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
        bool WillEdit(ID<ParameterType> type, WillEdit willEdit);
        string Name { get; }
        Guid Guid { get; }
        IParameterEditor<Control> Make();
    }
}
