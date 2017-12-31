using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ConversationEditor
{
    public class AudioGenerationParameters
    {
        public AudioGenerationParameters(FileInfo file, FileInfo project)
        {
            File = file;
            Project = project;
        }
        /// <summary>
        /// The document containing the node containing the audio parameter this value is being generated for
        /// </summary>
        public FileInfo File { get; }
        /// <summary>
        /// The currently loaded project
        /// </summary>
        public FileInfo Project { get; }
    }
}
