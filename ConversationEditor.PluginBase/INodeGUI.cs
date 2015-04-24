using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace ConversationEditor
{
    public interface INodeGUI : INodeUI<INodeGUI>, IGUI
    {
        //string DisplayName { get; } //TODO: Get rid of all implementations of this
    }
}
