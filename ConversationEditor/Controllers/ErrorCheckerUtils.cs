using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace ConversationEditor
{
    public class ErrorCheckerUtils<T> : IErrorCheckerUtilities<T> where T : class, IConversationNode
    {
        private IDataSource m_dataSource;
        private Func<IConversationNodeData, T> m_reverseLookup;

        public ErrorCheckerUtils(IDataSource datasource, Func<IConversationNodeData, T> reverseLookup)
        {
            m_dataSource = datasource;
            m_reverseLookup = reverseLookup;
        }

        public Guid GetCategory(Id<NodeTypeTemp> type)
        {
            return m_dataSource.GetCategory(type);
        }

        public T ReverseLookup(IConversationNodeData data)
        {
            return m_reverseLookup(data);
        }
    }
}
