using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using System.Collections.ObjectModel;
using Utilities;
using System.Globalization;

namespace ConversationEditor
{
    public class BaseTypeInteger : BaseType
    {
        public BaseTypeInteger()
            : base(parameterNodeType: Id<NodeTypeTemp>.Parse("eb8c2038-52ff-4f69-a36f-3a431d7c29cd"),
                   nodeType: TypeDefinitionNodeIds.Integer)
        {
        }

        public static ParameterType PARAMETER_TYPE { get; } = ParameterType.Parse("fa7245b2-7bbf-4f31-ad3d-41e78577131e");
        private const string NAME = "Integer";

        public static IntegerData Data { get { return new IntegerData(NAME, PARAMETER_TYPE, null, null); } }

        public override NodeData.ParameterData ReadDomainNode(IConversationNodeData parameterNode)
        {
            var parameterNameParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.ParameterName) as IStringParameter;
            var parameterName = parameterNameParameter.Value;
            var parameterTypeParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.PARAMETER_TYPE) as IEnumParameter;
            var parameterType = parameterTypeParameter.Value;
            var parameterDefParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.ParameterDefault) as IIntegerParameter;
            var parameterDef = parameterDefParameter.Value;
            return new NodeData.ParameterData(parameterName, Id<Parameter>.ConvertFrom(parameterNode.NodeId), ParameterType.Basic.FromGuid(parameterType), ReadConfig(parameterNode), parameterDef.ToString(CultureInfo.InvariantCulture));
        }

        public override string Name
        {
            get { return NAME; }
        }
    }

    public class BaseTypeDecimal : BaseType
    {
        public BaseTypeDecimal()
            : base(parameterNodeType: Id<NodeTypeTemp>.Parse("9c27b9b0-2105-4480-bcc1-5512d901afc1"),
                   nodeType: TypeDefinitionNodeIds.Decimal)
        {
        }

        public static ParameterType PARAMETER_TYPE { get; } = ParameterType.Parse("0222b56d-0e1b-40ac-bf86-5ab6399f6fc2");
        private const string NAME = "Decimal";

        public static DecimalData Data { get { return new DecimalData(NAME, PARAMETER_TYPE, null, null); } }

        public override NodeData.ParameterData ReadDomainNode(IConversationNodeData parameterNode)
        {
            var parameterNameParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.ParameterName) as IStringParameter;
            var parameterName = parameterNameParameter.Value;
            var parameterTypeParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.PARAMETER_TYPE) as IEnumParameter;
            var parameterType = parameterTypeParameter.Value;
            var parameterDefParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.ParameterDefault) as IDecimalParameter;
            var parameterDef = parameterDefParameter.Value;
            return new NodeData.ParameterData(parameterName, Id<Parameter>.ConvertFrom(parameterNode.NodeId), ParameterType.Basic.FromGuid(parameterType), ReadConfig(parameterNode), parameterDef.ToString(CultureInfo.InvariantCulture));
        }

        public override string Name
        {
            get { return NAME; }
        }
    }

    public class BaseTypeString : BaseType
    {
        public BaseTypeString()
            : base(parameterNodeType: Id<NodeTypeTemp>.Parse("1559995a-c744-477d-a46e-fadd20f4bf0a"))
        {
        }

        public static readonly ParameterType ParameterType = StringParameter.ParameterType;

        public override NodeData.ParameterData ReadDomainNode(IConversationNodeData parameterNode)
        {
            var parameterNameParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.ParameterName) as IStringParameter;
            var parameterName = parameterNameParameter.Value;
            var parameterDefaultParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.ParameterDefault) as IStringParameter;
            var parameterDefault = parameterDefaultParameter.Value;
            return new NodeData.ParameterData(parameterName, Id<Parameter>.ConvertFrom(parameterNode.NodeId), ParameterType, ReadConfig(parameterNode), parameterDefault);
        }

        public override string Name
        {
            get { return "String"; }
        }
    }

    public class BaseTypeLocalizedString : BaseType
    {
        public BaseTypeLocalizedString()
            : base(parameterNodeType: Id<NodeTypeTemp>.Parse("3114c980-6729-4a17-bc2c-0e4781ee1e7c"))
        {
        }

        public static readonly ParameterType ParameterType = LocalizedStringParameter.ParameterType;

        public override NodeData.ParameterData ReadDomainNode(IConversationNodeData parameterNode)
        {
            var parameterNameParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.ParameterName) as IStringParameter;
            var parameterName = parameterNameParameter.Value;
            return new NodeData.ParameterData(parameterName, Id<Parameter>.ConvertFrom(parameterNode.NodeId), ParameterType, ReadConfig(parameterNode));
        }

        public override string Name
        {
            get { return "Localized String"; }
        }
    }

    public class BaseTypeBoolean : BaseType
    {
        public BaseTypeBoolean()
            : base(parameterNodeType: Id<NodeTypeTemp>.Parse("9a6d0cd1-649a-4b76-be74-f486435fac3f"))
        {
        }

        public static ParameterType PARAMETER_TYPE { get; } = BooleanParameter.ParameterType;

        public override NodeData.ParameterData ReadDomainNode(IConversationNodeData parameterNode)
        {
            var parameterNameParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.ParameterName) as IStringParameter;
            var parameterName = parameterNameParameter.Value;
            var parameterDefParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.ParameterDefault) as IBooleanParameter;
            var parameterDef = parameterDefParameter.Value;
            return new NodeData.ParameterData(parameterName, Id<Parameter>.ConvertFrom(parameterNode.NodeId), PARAMETER_TYPE, ReadConfig(parameterNode), parameterDef.ToString());
        }

        public override string Name
        {
            get { return "Boolean"; }
        }
    }

    public class BaseTypeAudio : BaseType
    {
        public BaseTypeAudio()
            : base(parameterNodeType: Id<NodeTypeTemp>.Parse("0acba480-9e53-4945-91f2-61032eba4769"))
        {
        }

        public static ParameterType PARAMETER_TYPE { get; } = AudioParameter.ParameterType;

        public override NodeData.ParameterData ReadDomainNode(IConversationNodeData parameterNode)
        {
            var parameterNameParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.ParameterName) as IStringParameter;
            var parameterName = parameterNameParameter.Value;
            return new NodeData.ParameterData(parameterName, Id<Parameter>.ConvertFrom(parameterNode.NodeId), PARAMETER_TYPE, ReadConfig(parameterNode));
        }

        public override string Name
        {
            get { return "Audio"; }
        }
    }

    public class BaseTypeEnumeration : BaseType
    {
        public BaseTypeEnumeration()
            : base(parameterNodeType: Id<NodeTypeTemp>.Parse("a3bbcba8-036b-4fdb-938f-a4c51053946b"),
                   nodeType: TypeDefinitionNodeIds.Enumeration)
        {
        }

        public override NodeData.ParameterData ReadDomainNode(IConversationNodeData parameterNode)
        {
            var parameterNameParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.ParameterName) as IStringParameter;
            var parameterName = parameterNameParameter.Value;
            var parameterTypeParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.PARAMETER_TYPE) as IEnumParameter;
            var parameterType = ParameterType.Basic.FromGuid(parameterTypeParameter.Value);
            var parameterDefParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.ParameterDefault) as EnumDefaultParameter;
            var parameterDef = parameterDefParameter.BetterValue;
            var data = parameterDef.Transformed(d => new NodeData.ParameterData(parameterName, Id<Parameter>.ConvertFrom(parameterNode.NodeId), parameterType, ReadConfig(parameterNode), d.ToString()),
                                                d => new NodeData.ParameterData(parameterName, Id<Parameter>.ConvertFrom(parameterNode.NodeId), parameterType, ReadConfig(parameterNode), d));
            return data;
        }

        public override string Name
        {
            get { return "Enumeration"; }
        }
    }

    public class BaseTypeFlags : BaseType
    {
        public BaseTypeFlags()
            : base(parameterNodeType: Id<NodeTypeTemp>.Parse("d60469d8-2c7a-4cef-8dd3-c372a3481c8a"),
                   nodeType: TypeDefinitionNodeIds.Set)
        {
        }

        public override NodeData.ParameterData ReadDomainNode(IConversationNodeData parameterNode)
        {
            var parameterNameParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.ParameterName) as IStringParameter;
            var parameterName = parameterNameParameter.Value;
            var parameterTypeParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.PARAMETER_TYPE) as IEnumParameter;
            var parameterType = ParameterType.ValueSetType.FromGuid(parameterTypeParameter.Value);
            var parameterDefParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.ParameterDefault) as SetDefaultParameter;

            var data = new NodeData.ParameterData(parameterName, Id<Parameter>.ConvertFrom(parameterNode.NodeId), parameterType, ReadConfig(parameterNode), parameterDefParameter.SerializedValue);
            return data;
        }

        public override string Name
        {
            get { return "Set"; }
        }
    }

    public class BaseTypeDynamicEnumeration : BaseType
    {
        public BaseTypeDynamicEnumeration()
            : base(parameterNodeType: Id<NodeTypeTemp>.Parse("16dcbe34-3e14-40d6-b907-227514a40847"),
                   nodeType: TypeDefinitionNodeIds.DynamicEnumeration)
        { }

        public override NodeData.ParameterData ReadDomainNode(IConversationNodeData parameterNode)
        {
            var parameterNameParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.ParameterName) as IStringParameter;
            var parameterName = parameterNameParameter.Value;
            var parameterTypeParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.PARAMETER_TYPE) as IEnumParameter;
            var parameterType = parameterTypeParameter.Value;
            var parameterDefParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.ParameterDefault) as IStringParameter;
            var parameterDef = parameterDefParameter.Value;
            return new NodeData.ParameterData(parameterName, Id<Parameter>.ConvertFrom(parameterNode.NodeId), ParameterType.Basic.FromGuid(parameterType), ReadConfig(parameterNode), parameterDef);
        }

        public override string Name
        {
            get { return "Dynamic Enumeration"; }
        }
    }

    public class BaseTypeLocalDynamicEnumeration : BaseType
    {
        public BaseTypeLocalDynamicEnumeration()
            : base(parameterNodeType: Id<NodeTypeTemp>.Parse("9a778089-974e-4153-9b07-b18e4b3e39bc"),
                   nodeType: TypeDefinitionNodeIds.LocalDynamicEnumeration)
        { }

        public override NodeData.ParameterData ReadDomainNode(IConversationNodeData parameterNode)
        {
            var parameterNameParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.ParameterName) as IStringParameter;
            var parameterName = parameterNameParameter.Value;
            var parameterTypeParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.PARAMETER_TYPE) as IEnumParameter;
            var parameterType = parameterTypeParameter.Value;
            var parameterDefParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.ParameterDefault) as IStringParameter;
            var parameterDef = parameterDefParameter.Value;
            return new NodeData.ParameterData(parameterName, Id<Parameter>.ConvertFrom(parameterNode.NodeId), ParameterType.Basic.FromGuid(parameterType), ReadConfig(parameterNode), parameterDef);
        }

        public override string Name
        {
            get { return "Local Dynamic Enumeration"; }
        }
    }


    //internal class BaseTypeConnector : BaseType
    //{
    //    public BaseTypeConnector()
    //        : base(parameterNodeType: ID<NodeTypeTemp>.Parse("6148491b-eca8-46d5-bee2-02b3b637c2ad"))
    //    {
    //    }

    //    public static readonly ParameterType PARAMETER_TYPE = ParameterType.Parse("7509bdd0-0295-4685-87a7-db65cf0357d9");

    //    public override NodeData.ParameterData ReadDomainNode(IEditable parameterNode)
    //    {
    //        var parameterNameParameter = parameterNode.Parameters.Single(p => p.Id == DomainIDs.PARAMETER_NAME) as IStringParameter;
    //        var parameterName = parameterNameParameter.Value;
    //        return new NodeData.ParameterData(parameterName, ID<Parameter>.ConvertFrom(parameterNode.NodeID), PARAMETER_TYPE, null);
    //    }

    //    public override string Name
    //    {
    //        get { return "Connector"; }
    //    }
    //}

    public abstract class BaseType
    {
        public readonly Id<NodeTypeTemp> NodeType;           //Type ID of the node which defines a new subtype of this type
        public readonly Id<NodeTypeTemp> ParameterNodeType;  //Type ID of the node which defines a new parameter of this type

        protected BaseType(Id<NodeTypeTemp> parameterNodeType, Id<NodeTypeTemp> nodeType = null)
        {
            ParameterNodeType = parameterNodeType;
            NodeType = nodeType;
        }

        public static BaseTypeInteger Integer { get; } = new BaseTypeInteger();
        public static BaseTypeDecimal Decimal { get; } = new BaseTypeDecimal();
        public static BaseTypeString String { get; } = new BaseTypeString();
        public static BaseTypeLocalizedString LocalizedString { get; } = new BaseTypeLocalizedString();
        public static BaseTypeBoolean Boolean { get; } = new BaseTypeBoolean();
        public static BaseTypeAudio Audio { get; } = new BaseTypeAudio();
        public static BaseTypeEnumeration Enumeration { get; } = new BaseTypeEnumeration();
        public static BaseTypeDynamicEnumeration DynamicEnumeration { get; } = new BaseTypeDynamicEnumeration();
        public static BaseTypeLocalDynamicEnumeration LocalDynamicEnumeration { get; } = new BaseTypeLocalDynamicEnumeration();
        public static BaseTypeFlags Set { get; } = new BaseTypeFlags();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static BaseType()
        {
            Action<BaseType> AddMapping = b => { s_parameterNodeTypeToBaseTypeMapping.Add(b.ParameterNodeType, b); };

            AddMapping(Integer);
            AddMapping(Decimal);
            AddMapping(String);
            AddMapping(LocalizedString);
            AddMapping(Boolean);
            AddMapping(Audio);
            AddMapping(Enumeration);
            AddMapping(DynamicEnumeration);
            AddMapping(LocalDynamicEnumeration);
            AddMapping(Set);
        }

        private static Dictionary<Id<NodeTypeTemp>, BaseType> s_parameterNodeTypeToBaseTypeMapping = new Dictionary<Id<NodeTypeTemp>, BaseType>();

        public static IEnumerable<BaseType> BaseTypes { get { return s_parameterNodeTypeToBaseTypeMapping.Values; } }

        public static BaseType GetType(Id<NodeTypeTemp> parameterNodeType)
        {
            return s_parameterNodeTypeToBaseTypeMapping[parameterNodeType];
        }

        public static bool TypeExists(Id<NodeTypeTemp> parameterNodeType)
        {
            return s_parameterNodeTypeToBaseTypeMapping.ContainsKey(parameterNodeType);
        }

        public abstract NodeData.ParameterData ReadDomainNode(IConversationNodeData parameterNode);

        public abstract string Name { get; }

        protected static ReadOnlyCollection<NodeData.ConfigData> ReadConfig(IConversationNodeData parameterNode)
        {
            List<NodeData.ConfigData> config = new List<NodeData.ConfigData>();
            var configs = parameterNode.Connectors.Single(c => c.Definition.Id == DomainIDs.ParameterConfigConnectorDefinition.Id).Connections.Select(l => l.Parent);
            foreach (var configNode in configs)
            {
                config.Add(new NodeData.ConfigData(configNode.NodeTypeId, configNode.Parameters));
            }
            return config.AsReadOnly();
        }
    }

}
