using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public struct NodeTypeData
    {
        public NodeTypeData(string name, Guid guid, Guid parent)
        {
            m_name = name;
            m_guid = guid;
            m_parent = parent;
        }

        private readonly string m_name;
        private readonly Guid m_guid;
        private readonly Guid m_parent;

        public string Name { get { return m_name; } }
        public Guid Guid { get { return m_guid; } }
        public Guid Parent { get { return m_parent; } }
    }

}
