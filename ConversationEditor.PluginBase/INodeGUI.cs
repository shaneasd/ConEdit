using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace ConversationEditor
{
    public interface INodeGui : INodeUI<INodeGui>, IGui
    {
    }
}
