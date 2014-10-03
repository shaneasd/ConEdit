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

        public IEnumerable<ID<ParameterType>> ParameterTypes
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

        public bool IsInteger(ID<ParameterType> guid)
        {
            return false;
        }

        public bool IsDecimal(ID<ParameterType> guid)
        {
            return false;
        }

        public bool IsEnum(ID<ParameterType> guid)
        {
            throw new NotImplementedException();
        }

        public bool IsDynamicEnum(ID<ParameterType> guid)
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
    }
}
