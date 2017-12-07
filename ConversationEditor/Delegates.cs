using Conversation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConversationEditor
{
    public delegate DocumentPath GetFilePath(Id<FileInProject> fileId);
}
