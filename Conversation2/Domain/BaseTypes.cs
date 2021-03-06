﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace Conversation
{

    public class BaseTypeInteger : BaseType
    {
        public BaseTypeInteger()
            : base(parameterNodeType: ID<NodeTypeTemp>.Parse("eb8c2038-52ff-4f69-a36f-3a431d7c29cd"),
                   nodeType: TypeDefinitionNodeIDs.Integer)
        {
        }

        public static readonly ID<ParameterType> PARAMETER_TYPE = ID<ParameterType>.Parse("fa7245b2-7bbf-4f31-ad3d-41e78577131e");

        public override NodeData.ParameterData ReadDomainNode(IEditable parameterNode)
        {
            var parameterNameParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.PARAMETER_NAME) as IStringParameter;
            var parameterName = parameterNameParameter.Value;
            var parameterTypeParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.PARAMETER_TYPE) as IEnumParameter;
            var parameterType = parameterTypeParameter.Value;
            var parameterDefParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.PARAMETER_DEFAULT) as IIntegerParameter;
            var parameterDef = parameterDefParameter.Value;
            return new NodeData.ParameterData(parameterName, ID<Parameter>.ConvertFrom(parameterNode.NodeID), ID<ParameterType>.FromGuid(parameterType), parameterDef.ToString());
        }
    }

    public class BaseTypeDecimal : BaseType
    {
        public BaseTypeDecimal()
            : base(parameterNodeType: ID<NodeTypeTemp>.Parse("9c27b9b0-2105-4480-bcc1-5512d901afc1"),
                   nodeType: TypeDefinitionNodeIDs.Decimal)
        {
        }

        public static readonly ID<ParameterType> PARAMETER_TYPE = ID<ParameterType>.Parse("0222b56d-0e1b-40ac-bf86-5ab6399f6fc2");

        public override NodeData.ParameterData ReadDomainNode(IEditable parameterNode)
        {
            var parameterNameParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.PARAMETER_NAME) as IStringParameter;
            var parameterName = parameterNameParameter.Value;
            var parameterTypeParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.PARAMETER_TYPE) as IEnumParameter;
            var parameterType = parameterTypeParameter.Value;
            var parameterDefParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.PARAMETER_DEFAULT) as IDecimalParameter;
            var parameterDef = parameterDefParameter.Value;
            return new NodeData.ParameterData(parameterName, ID<Parameter>.ConvertFrom(parameterNode.NodeID), ID<ParameterType>.FromGuid(parameterType), parameterDef.ToString());
        }
    }

    public class BaseTypeString : BaseType
    {
        public BaseTypeString()
            : base(parameterNodeType: ID<NodeTypeTemp>.Parse("1559995a-c744-477d-a46e-fadd20f4bf0a"))
        {
        }

        public static readonly ID<ParameterType> PARAMETER_TYPE = ID<ParameterType>.Parse("7ca91556-5526-4c5c-b565-00aff5ae85ce");

        public override NodeData.ParameterData ReadDomainNode(IEditable parameterNode)
        {
            var parameterNameParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.PARAMETER_NAME) as IStringParameter;
            var parameterName = parameterNameParameter.Value;
            return new NodeData.ParameterData(parameterName, ID<Parameter>.ConvertFrom(parameterNode.NodeID), PARAMETER_TYPE, null);
        }
    }

    public class BaseTypeLocalizedString : BaseType
    {
        public BaseTypeLocalizedString()
            : base(parameterNodeType: ID<NodeTypeTemp>.Parse("3114c980-6729-4a17-bc2c-0e4781ee1e7c"))
        {
        }

        public static readonly ID<ParameterType> PARAMETER_TYPE = ID<ParameterType>.Parse("c72e8222-3e10-4995-b32b-5b3ebd8e0f20");

        public override NodeData.ParameterData ReadDomainNode(IEditable parameterNode)
        {
            var parameterNameParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.PARAMETER_NAME) as IStringParameter;
            var parameterName = parameterNameParameter.Value;
            return new NodeData.ParameterData(parameterName, ID<Parameter>.ConvertFrom(parameterNode.NodeID), PARAMETER_TYPE, null);
        }
    }

    public class BaseTypeBoolean : BaseType
    {
        public BaseTypeBoolean()
            : base(parameterNodeType: ID<NodeTypeTemp>.Parse("9a6d0cd1-649a-4b76-be74-f486435fac3f"))
        {
        }

        public static readonly ID<ParameterType> PARAMETER_TYPE = ID<ParameterType>.Parse("3a98d216-7427-45ef-a3ca-cd47431835a0");

        public override NodeData.ParameterData ReadDomainNode(IEditable parameterNode)
        {
            var parameterNameParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.PARAMETER_NAME) as IStringParameter;
            var parameterName = parameterNameParameter.Value;
            return new NodeData.ParameterData(parameterName, ID<Parameter>.ConvertFrom(parameterNode.NodeID), PARAMETER_TYPE, null);
        }
    }

    public class BaseTypeAudio : BaseType
    {
        public BaseTypeAudio()
            : base(parameterNodeType: ID<NodeTypeTemp>.Parse("0acba480-9e53-4945-91f2-61032eba4769"))
        {
        }

        public static readonly ID<ParameterType> PARAMETER_TYPE = ID<ParameterType>.Parse("05b29166-31c5-449f-bb91-d63a603183db");

        public override NodeData.ParameterData ReadDomainNode(IEditable parameterNode)
        {
            var parameterNameParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.PARAMETER_NAME) as IStringParameter;
            var parameterName = parameterNameParameter.Value;
            return new NodeData.ParameterData(parameterName, ID<Parameter>.ConvertFrom(parameterNode.NodeID), PARAMETER_TYPE, null);
        }
    }

    public class BaseTypeEnumeration : BaseType
    {
        public BaseTypeEnumeration()
            : base(parameterNodeType: ID<NodeTypeTemp>.Parse("a3bbcba8-036b-4fdb-938f-a4c51053946b"),
                   nodeType: TypeDefinitionNodeIDs.Enumeration)
        {
        }

        public override NodeData.ParameterData ReadDomainNode(IEditable parameterNode)
        {
            var parameterNameParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.PARAMETER_NAME) as IStringParameter;
            var parameterName = parameterNameParameter.Value;
            var parameterTypeParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.PARAMETER_TYPE) as IEnumParameter;
            var parameterType = ID<ParameterType>.FromGuid(parameterTypeParameter.Value);
            var parameterDefParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.PARAMETER_DEFAULT) as DomainDomain.EnumDefaultParameter;
            var parameterDef = parameterDefParameter.BetterValue;
            var data = parameterDef.Transformed(d => new NodeData.ParameterData(parameterName, ID<Parameter>.ConvertFrom(parameterNode.NodeID), parameterType, d.ToString()),
                                                d => new NodeData.ParameterData(parameterName, ID<Parameter>.ConvertFrom(parameterNode.NodeID), parameterType, d));
            return data;
        }
    }

    public class BaseTypeDynamicEnumeration : BaseType
    {
        public BaseTypeDynamicEnumeration()
            : base(parameterNodeType: ID<NodeTypeTemp>.Parse("16dcbe34-3e14-40d6-b907-227514a40847"),
                   nodeType: TypeDefinitionNodeIDs.DynamicEnumeration)
        { }

        public override NodeData.ParameterData ReadDomainNode(IEditable parameterNode)
        {
            var parameterNameParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.PARAMETER_NAME) as IStringParameter;
            var parameterName = parameterNameParameter.Value;
            var parameterTypeParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.PARAMETER_TYPE) as IEnumParameter;
            var parameterType = parameterTypeParameter.Value;
            var parameterDefParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.PARAMETER_DEFAULT) as IStringParameter;
            var parameterDef = parameterDefParameter.Value;
            return new NodeData.ParameterData(parameterName, ID<Parameter>.ConvertFrom(parameterNode.NodeID), ID<ParameterType>.FromGuid(parameterType), parameterDef);
        }
    }

    public class BaseTypeConnector : BaseType
    {
        public BaseTypeConnector()
            : base(parameterNodeType: ID<NodeTypeTemp>.Parse("6148491b-eca8-46d5-bee2-02b3b637c2ad"))
        {
        }

        public static readonly ID<ParameterType> PARAMETER_TYPE = ID<ParameterType>.Parse("7509bdd0-0295-4685-87a7-db65cf0357d9");

        public override NodeData.ParameterData ReadDomainNode(IEditable parameterNode)
        {
            var parameterNameParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.PARAMETER_NAME) as IStringParameter;
            var parameterName = parameterNameParameter.Value;
            return new NodeData.ParameterData(parameterName, ID<Parameter>.ConvertFrom(parameterNode.NodeID), PARAMETER_TYPE, null);
        }
    }

    public abstract class BaseType
    {
        public readonly ID<NodeTypeTemp> NodeType;           //Type ID of the node which defines a new subtype of this type
        public readonly ID<NodeTypeTemp> ParameterNodeType;  //Type ID of the node which defines a new parameter of this type

        public BaseType(ID<NodeTypeTemp> parameterNodeType, ID<NodeTypeTemp> nodeType = null)
        {
            ParameterNodeType = parameterNodeType;
            NodeType = nodeType;
        }

        public static BaseTypeInteger Integer = new BaseTypeInteger();
        public static BaseTypeDecimal Decimal = new BaseTypeDecimal();
        public static BaseTypeString String = new BaseTypeString();
        public static BaseTypeLocalizedString LocalizedString = new BaseTypeLocalizedString();
        public static BaseTypeBoolean Boolean = new BaseTypeBoolean();
        public static BaseTypeAudio Audio = new BaseTypeAudio();
        public static BaseTypeEnumeration Enumeration = new BaseTypeEnumeration();
        public static BaseTypeDynamicEnumeration DynamicEnumeration = new BaseTypeDynamicEnumeration();

        static BaseType()
        {
            m_parameterNodeTypeToBaseTypeMapping = new Dictionary<ID<NodeTypeTemp>, BaseType>();
            Action<BaseType> AddMapping = b => { m_parameterNodeTypeToBaseTypeMapping.Add(b.ParameterNodeType, b); };

            AddMapping(Integer);
            AddMapping(Decimal);
            AddMapping(String);
            AddMapping(LocalizedString);
            AddMapping(Boolean);
            AddMapping(Audio);
            AddMapping(Enumeration);
            AddMapping(DynamicEnumeration);
        }

        private static Dictionary<ID<NodeTypeTemp>, BaseType> m_parameterNodeTypeToBaseTypeMapping;

        public static IEnumerable<BaseType> BaseTypes { get { return m_parameterNodeTypeToBaseTypeMapping.Values; } }

        public static BaseType GetType(ID<NodeTypeTemp> parameterNodeType)
        {
            return m_parameterNodeTypeToBaseTypeMapping[parameterNodeType];
        }

        public static bool TypeExists(ID<NodeTypeTemp> parameterNodeType)
        {
            return m_parameterNodeTypeToBaseTypeMapping.ContainsKey(parameterNodeType);
        }

        public abstract NodeData.ParameterData ReadDomainNode(IEditable parameterNode);
    }

}
