using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RuntimeConversation
{
    public class Audio
    {
        public string Path { get { return m_path; } }
        private readonly string m_path;
        public Audio(string value)
        {
            m_path = value;
        }
    }
}
