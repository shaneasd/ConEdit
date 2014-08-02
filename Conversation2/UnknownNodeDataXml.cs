using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Conversation
{
    //TODO: Move to editor project
    public class UnknownNodeDataXml// : IConversationNode<T, TTransitionUI>
    {
        private Guid m_guid;
        private NodeID m_id;
        private XElement m_data;

        public UnknownNodeDataXml(XElement data, NodeID id, Guid guid)
        {
            m_id = id;
            m_guid = guid;
            m_data = data;
        }
        public NodeID Id
        {
            get { return m_id; }
        }

        public Guid Guid
        {
            get { return m_guid; }
        }
    }
}
