using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace ConversationEditor
{
    public interface IProjectExporter
    {
        string Name { get; }
        void Export(IProject2 project, ConfigParameterString exportPath, Func<Id<LocalizedText>, Tuple<string, DateTime>> localize, IErrorCheckerUtilities<IConversationNode> util);
    }
}
