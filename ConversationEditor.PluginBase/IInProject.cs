using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConversationEditor
{
    public interface IInProject
    {
        bool CanRemove(Func<bool> prompt);
        void Removed();
    }
}
