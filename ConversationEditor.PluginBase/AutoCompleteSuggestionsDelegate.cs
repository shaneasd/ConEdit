using Conversation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversationEditor
{
    public delegate IEnumerable<string> AutoCompleteSuggestionsDelegate(IParameter parameter, string start);
}
