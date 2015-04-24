using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;

namespace ConversationEditor
{
    public interface IAudioFile : IInProject, ISaveableFileProvider
    {
        void Play();
    }
}
