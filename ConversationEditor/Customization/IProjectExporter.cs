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
        void Export(IProject project, ConfigParameterString exportPath, Func<ID<LocalizedText>, string> localize, IErrorCheckerUtilities util);
    }
}
