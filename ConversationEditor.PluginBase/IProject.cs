using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.Collections.ObjectModel;

namespace ConversationEditor
{
    //TODO: Rename
    public interface IProject2 : ISaveableFileProvider
    {
        IEnumerable<IDomainFile> DomainFilesCollection { get; }

        IEnumerable<IConversationFile> ConversationFilesCollection { get; }
    }
}
