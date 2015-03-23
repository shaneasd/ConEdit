using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace ConversationEditor.Controllers
{
    class ProjectDomain : IDataSource
    {
        public static ProjectDomain Instance = new ProjectDomain();

        public IEnumerable<ParameterType> ParameterTypes
        {
            get { throw new NotImplementedException(); }
        }

        public INodeType Nodes
        {
            get { throw new NotImplementedException(); }
        }

        public EditableGenerator GetNode(ID<NodeTypeTemp> guid)
        {
            throw new NotImplementedException();
        }

        public bool IsInteger(ParameterType guid)
        {
            return false;
        }

        public bool IsDecimal(ParameterType guid)
        {
            return false;
        }

        public bool IsEnum(ParameterType guid)
        {
            throw new NotImplementedException();
        }

        public bool IsDynamicEnum(ParameterType guid)
        {
            throw new NotImplementedException();
        }

        public bool IsCategoryDefinition(ID<NodeTypeTemp> id)
        {
            throw new NotImplementedException();
        }

        public bool IsTypeDefinition(ID<NodeTypeTemp> id)
        {
            throw new NotImplementedException();
        }

        public bool IsConnectorDefinition(ID<NodeTypeTemp> id)
        {
            throw new NotImplementedException();
        }

        public bool IsNodeDefinition(ID<NodeTypeTemp> id)
        {
            throw new NotImplementedException();
        }


        public string GetTypeName(ParameterType type)
        {
            throw new NotImplementedException();
        }

        public Guid GetCategory(ID<NodeTypeTemp> type)
        {
            throw new NotImplementedException();
        }
    }
}
