using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace ConversationEditor
{
    internal class ProjectDomain : IDataSource
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

        public EditableGenerator GetNode(Id<NodeTypeTemp> guid)
        {
            throw new NotImplementedException();
        }

        public bool IsInteger(ParameterType type)
        {
            return false;
        }

        public bool IsDecimal(ParameterType type)
        {
            return false;
        }

        public bool IsEnum(ParameterType type)
        {
            throw new NotImplementedException();
        }

        public bool IsDynamicEnum(ParameterType type)
        {
            throw new NotImplementedException();
        }

        public bool IsLocalDynamicEnum(ParameterType type)
        {
            throw new NotImplementedException();
        }

        public bool IsCategoryDefinition(Id<NodeTypeTemp> id)
        {
            throw new NotImplementedException();
        }

        public bool IsTypeDefinition(Id<NodeTypeTemp> id)
        {
            throw new NotImplementedException();
        }

        public bool IsConnectorDefinition(Id<NodeTypeTemp> id)
        {
            throw new NotImplementedException();
        }

        public bool IsNodeDefinition(Id<NodeTypeTemp> id)
        {
            throw new NotImplementedException();
        }

        public bool IsAutoCompleteNode(Id<NodeTypeTemp> id)
        {
            throw new NotImplementedException();
        }

        public string GetTypeName(ParameterType type)
        {
            throw new NotImplementedException();
        }

        public Guid GetCategory(Id<NodeTypeTemp> type)
        {
            throw new NotImplementedException();
        }

        public DynamicEnumParameter.Source GetSource(ParameterType type, object newSourceId)
        {
            throw new NotImplementedException();
        }
    }
}
