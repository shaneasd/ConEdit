using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace ConversationEditor
{
    class ErrorCheckerUtils : IErrorCheckerUtilities
    {
        private IDataSource m_dataSource;
        public ErrorCheckerUtils(IDataSource datasource)
        {
            m_dataSource = datasource;
        }
        public Guid GetCategory(ID<NodeTypeTemp> type)
        {
            return m_dataSource.GetCategory(type);
        }
    }
}
