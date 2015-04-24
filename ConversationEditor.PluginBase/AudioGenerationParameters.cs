using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ConversationEditor
{
    public class AudioGenerationParameters
    {
        private FileInfo m_file;
        private FileInfo m_project;

        public AudioGenerationParameters(FileInfo file, FileInfo project)
        {
            m_file = file;
            m_project = project;
        }
        /// <summary>
        /// The document containing the node containing the audio parameter this value is being generated for
        /// </summary>
        public FileInfo File { get { return m_file; } }
        /// <summary>
        /// The currently loaded project
        /// </summary>
        public FileInfo Project { get { return m_project; } }
    }
}
