using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public interface INodeFactory<TNode, in TNodeUI> where TNode : IGraphNode, IConfigurable
    {
        TNode MakeNode(IEditable e, TNodeUI uiData);
    }
}
