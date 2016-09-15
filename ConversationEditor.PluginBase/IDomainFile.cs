using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGui>;
using Conversation;

namespace ConversationEditor
{
    public interface IDomainFile : IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo>, IInProject
    {
        IDomainData Data { get; }
        event Action ConversationDomainModified;
        IEnumerable<string> AutoCompleteSuggestions(IParameter p, string s, Func<ParameterType,DynamicEnumParameter.Source> enumSource);
    }
}
