using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;

using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGui>;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace ConversationEditor
{
    public delegate Output OutputDefinition(IEditable parent);

    internal class DomainDomain : IDataSource
    {
        TypeSet m_typeSet;

        public static readonly ParameterType IntegerSetGuid = ParameterType.Parse("07ca7287-20c0-4ba5-ae28-e17ea97554d6");
        public static readonly ParameterType DecimalSetGuid = ParameterType.Parse("0c1e5fa8-97ff-450b-a01c-5d09ea6dbd78");
        public static readonly ParameterType EnumSetGuid = ParameterType.Parse("e7526632-95ca-4981-8b45-56cea272ddd0");
        public static readonly ParameterType DynamicEnumSetGuid = ParameterType.Parse("b3278dc3-6c2b-471a-a1c9-de39691af302");
        public static readonly ParameterType LocalDynamicEnumSetGuid = ParameterType.Parse("15fb7660-0c2e-45b3-979f-3bdf17f60fc9");

        Id<TConnector> parameterDefinitionConnector1 = Id<TConnector>.Parse("1fd8a64d-271e-42b8-bfd8-85e5174bbf9d");
        Id<TConnector> parameterConfigConnectorID = Id<TConnector>.Parse("3e914fe1-c59c-4494-b53a-da135426ff72");

        NodeType m_nodeHeirarchy;
        Dictionary<Id<NodeTypeTemp>, EditableGenerator> m_nodes = new Dictionary<Id<NodeTypeTemp>, EditableGenerator>();

        NodeType m_nodeMenu;
        NodeType m_autoCompleteMenu;
        NodeType m_connectorsMenu;

        static readonly List<NodeData.ConnectorData> NO_CONNECTORS = new List<NodeData.ConnectorData>();
        static readonly ReadOnlyCollection<NodeData.ConfigData> NO_CONFIG = new ReadOnlyCollection<NodeData.ConfigData>(new NodeData.ConfigData[0]);

        public DomainDomain(PluginsConfig pluginsConfig)
        {
            m_typeSet = BaseTypeSet.Make();
            m_pluginsConfig = pluginsConfig;

            EnumerationData categoryData = new EnumerationData("Categories", DomainIDs.CategoryType, new[] { new EnumerationData.Element("None", DomainIDs.CategoryNone) });
            m_typeSet.AddEnum(categoryData, true);

            var connectorOptions = new[] { new EnumerationData.Element(SpecialConnectors.Input.Name, SpecialConnectors.Input.Id.Guid),
                                           new EnumerationData.Element(SpecialConnectors.Output.Name, SpecialConnectors.Output.Id.Guid), };
            EnumerationData connectionData = new EnumerationData("Connections", DomainIDs.ConnectionType, connectorOptions);
            m_typeSet.AddEnum(connectionData, true);

            //m_types.Enumerations.Add(categoryData.Guid, categoryData.Name, categoryData);
            m_typeSet.AddEnum(ConnectorPosition.PositionConnectorDefinition, true);
            EnumerationData allEnums = new EnumerationData("Enumerations", EnumSetGuid, new List<EnumerationData.Element>());
            EnumerationData allDynamicEnums = new EnumerationData("Dynamic Enumerations", DynamicEnumSetGuid, new List<EnumerationData.Element>());
            EnumerationData allLocalDynamicEnums = new EnumerationData("Local Dynamic Enumerations", LocalDynamicEnumSetGuid, new List<EnumerationData.Element>());
            EnumerationData allIntegers = new EnumerationData("Integers", IntegerSetGuid, new List<EnumerationData.Element> { new EnumerationData.Element("Base", BaseTypeInteger.Data.TypeId.Guid) });
            EnumerationData allDecimals = new EnumerationData("Decimals", DecimalSetGuid, new List<EnumerationData.Element> { new EnumerationData.Element("Base", BaseTypeDecimal.Data.TypeId.Guid) });
            m_typeSet.AddEnum(allEnums, true);
            m_typeSet.AddEnum(allDynamicEnums, true);
            m_typeSet.AddEnum(allLocalDynamicEnums, true);
            m_typeSet.AddEnum(allIntegers, true);
            m_typeSet.AddEnum(allDecimals, true);
            //m_types.Enumerations.Add(ConnectorPosition.PositionConnectorDefinition.Guid, ConnectorPosition.PositionConnectorDefinition.Name, ConnectorPosition.PositionConnectorDefinition);


            m_typeSet.Modified += id =>
            {
                if (!(new[] { allEnums, allDynamicEnums, allLocalDynamicEnums, allIntegers, allDecimals }).Any(e => e.TypeID == id))
                {
                    allEnums.Elements = m_typeSet.VisibleEnums.Select(e => new EnumerationData.Element(e.Name, e.TypeID.Guid)).ToList();
                    m_typeSet.ModifyEnum(allEnums);

                    allDynamicEnums.Elements = m_typeSet.VisibleDynamicEnums.Select(e => new EnumerationData.Element(e.Name, e.TypeId.Guid)).ToList();
                    m_typeSet.ModifyEnum(allDynamicEnums);

                    allLocalDynamicEnums.Elements = m_typeSet.VisibleLocalDynamicEnums.Select(e => new EnumerationData.Element(e.Name, e.TypeId.Guid)).ToList();
                    m_typeSet.ModifyEnum(allLocalDynamicEnums);

                    allIntegers.Elements = m_typeSet.VisibleIntegers.Select(e => new EnumerationData.Element(e.Name, e.TypeId.Guid)).ToList();
                    m_typeSet.ModifyEnum(allIntegers);

                    allDecimals.Elements = m_typeSet.VisibleDecimals.Select(e => new EnumerationData.Element(e.Name, e.TypeId.Guid)).ToList();
                    m_typeSet.ModifyEnum(allDecimals);
                }
            };

            //m_categories = new MutableEnumeration(categoryNone.Only(), DomainIDs.CATEGORY_TYPE, categoryNone.Item1);
            m_nodeHeirarchy = new NodeType(null, Guid.Empty);

            #region Category Definition
            {
                List<NodeData.ParameterData> parameters = new List<NodeData.ParameterData>()
                {
                    new NodeData.ParameterData("Name", DomainIDs.CategoryName, BaseTypeString.ParameterType, NO_CONFIG),
                    new NodeData.ParameterData("Parent", DomainIDs.CategoryParent, DomainIDs.CategoryType, NO_CONFIG),
                };

                AddNode(DomainIDs.CategoryGuid, "Category", m_nodeHeirarchy, MakeConfig('x', "808080"), NO_CONNECTORS, parameters);
            }
            #endregion

            #region Custom Type Definition
            List<NodeData.ParameterData> integerParameters = new List<NodeData.ParameterData>()
            {
                new NodeData.ParameterData("Name", DomainIDs.IntegerName, BaseTypeString.ParameterType, NO_CONFIG),
                new NodeData.ParameterData("Max", DomainIDs.IntegerMax, BaseTypeInteger.PARAMETER_TYPE, NO_CONFIG),
                new NodeData.ParameterData("Min", DomainIDs.IntegerMin, BaseTypeInteger.PARAMETER_TYPE, NO_CONFIG),
            };
            AddNode(BaseType.Integer.NodeType, "Integer", m_nodeHeirarchy, MakeConfig('t', "808080"), NO_CONNECTORS, integerParameters);

            List<NodeData.ParameterData> decimalParameters = new List<NodeData.ParameterData>()
            {
                new NodeData.ParameterData("Name", DomainIDs.DecimalName, BaseTypeString.ParameterType, NO_CONFIG),
                new NodeData.ParameterData("Max", DomainIDs.DecimalMax, BaseTypeDecimal.PARAMETER_TYPE, NO_CONFIG),
                new NodeData.ParameterData("Min", DomainIDs.DecimalMin, BaseTypeDecimal.PARAMETER_TYPE, NO_CONFIG),
            };
            AddNode(BaseType.Decimal.NodeType, "Decimal", m_nodeHeirarchy, MakeConfig('d', "808080"), NO_CONNECTORS, decimalParameters);

            List<NodeData.ParameterData> dynamicEnumParameters = new List<NodeData.ParameterData>()
            {
                new NodeData.ParameterData("Name", DomainIDs.DynamicEnumName, BaseTypeString.ParameterType, NO_CONFIG),
            };
            AddNode(BaseType.DynamicEnumeration.NodeType, "Dynamic Enumeration", m_nodeHeirarchy, MakeConfig('y', "808080"), NO_CONNECTORS, dynamicEnumParameters);

            List<NodeData.ParameterData> localDynamicEnumParameters = new List<NodeData.ParameterData>()
            {
                new NodeData.ParameterData("Name", DomainIDs.LocalDynamicEnumName, BaseTypeString.ParameterType, NO_CONFIG),
            };
            AddNode(BaseType.LocalDynamicEnumeration.NodeType, "Local Dynamic Enumeration", m_nodeHeirarchy, MakeConfig('y', "808080"), NO_CONNECTORS, localDynamicEnumParameters);

            NodeType enumerationMenu = new NodeType("Enumeration", DomainIDs.EnumerationMenu);
            m_nodeHeirarchy.AddChildType(enumerationMenu);

            Id<TConnector> enumOutput1 = Id<TConnector>.Parse("8c8dc149-52c1-401e-ae1d-69b265a6841e");
            List<NodeData.ConnectorData> enumerationConnectors = new List<NodeData.ConnectorData>()
            {
                new NodeData.ConnectorData(enumOutput1, DomainIDs.EnumOutputDefinition.Id, new List<Parameter>()),
            };
            List<NodeData.ParameterData> enumerationParameters = new List<NodeData.ParameterData>()
            {
                new NodeData.ParameterData("Name", DomainIDs.EnumerationName, BaseTypeString.ParameterType, NO_CONFIG),
            };
            AddNode(BaseType.Enumeration.NodeType, "Enumeration", enumerationMenu, MakeConfig('e', "808080"), enumerationConnectors, enumerationParameters);

            Id<TConnector> enumerationValueOutput1 = Id<TConnector>.Parse("ef845d1a-11a2-45d1-bab2-de8104b46c51");
            List<NodeData.ConnectorData> enumerationValueConnectors = new List<NodeData.ConnectorData>()
            {
                new NodeData.ConnectorData(enumerationValueOutput1, DomainIDs.EnumValueOutputDefinition.Id, new List<Parameter>()),
            };
            List<NodeData.ParameterData> enumerationValuesParameters = new List<NodeData.ParameterData>()
            {
                new NodeData.ParameterData("Name", DomainIDs.EnumerationValueParameter, BaseTypeString.ParameterType, NO_CONFIG),
            };
            AddNode(DomainIDs.EnumerationValueDeclaration, "Enumeration Value", enumerationMenu, MakeConfig('v', "808080"), enumerationValueConnectors, enumerationValuesParameters);
            #endregion

            m_nodeMenu = new NodeType("Nodes", DomainIDs.NodeMenu);
            m_nodeHeirarchy.AddChildType(m_nodeMenu);

            NodeType parameterMenu = new NodeType("Parameters", DomainIDs.NodeMenu);
            m_nodeMenu.AddChildType(parameterMenu);
            List<NodeData.ConnectorData> connectorDefinitionConnectors = new List<NodeData.ConnectorData>()
            {
                new NodeData.ConnectorData(Id<TConnector>.Parse("9231f7d0-9831-4a99-b59a-3479b2716f7d"), DomainIDs.ConnectorDefiinitionOutputDefinition.Id, new List<Parameter>()),
                //new NodeData.ConnectorData(ID<TConnector>.Parse("5b52871e-183c-409f-a4fd-7be3f7fab82a"), DomainIDs.CONNECTOR_DEFINITION_CONNECTION_DEFINITION.Id, new List<Parameter>()),
            };
            List<NodeData.ParameterData> connectorDefinitionParameters = new List<NodeData.ParameterData>()
            {
                new NodeData.ParameterData("Name", DomainIDs.ConnectorDefinitionName, BaseTypeString.ParameterType, NO_CONFIG),
                new NodeData.ParameterData("Position", ConnectorPosition.ParameterId, ConnectorPosition.EnumId, NO_CONFIG, ConnectorPosition.Bottom.Element.Guid.ToString()),
            };
            AddNode(DomainIDs.ConnectorDefinitionGuid, "Connector", m_nodeHeirarchy, MakeConfig('o', "ffff00"), connectorDefinitionConnectors, connectorDefinitionParameters);

            List<NodeData.ParameterData> connectionDefinitionParameters = new List<NodeData.ParameterData>()
            {
                new NodeData.ParameterData("Connector1", DomainIDs.ConnectionDefinitionConnector1, DomainIDs.ConnectionType, NO_CONFIG),
                new NodeData.ParameterData("Connector2", DomainIDs.ConnectionDefinitionConnector2, DomainIDs.ConnectionType, NO_CONFIG),
            };
            AddNode(DomainIDs.ConnectionDefinitionGuid, "Connection", m_nodeHeirarchy, MakeConfig('o', "ffbb00"), NO_CONNECTORS, connectionDefinitionParameters);


            m_connectorsMenu = new NodeType("Connectors", DomainIDs.NodeMenu);
            m_nodeMenu.AddChildType(m_connectorsMenu);

            #region Parameter Definitions
            {
                NodeData.ConnectorData parameterOutput = new NodeData.ConnectorData(parameterDefinitionConnector1, DomainIDs.ParameterOutputDefinition.Id, new List<Parameter>());
                NodeData.ConnectorData parameterConfigConnector = new NodeData.ConnectorData(parameterConfigConnectorID, DomainIDs.ParameterConfigConnectorDefinition.Id, new List<Parameter>());

                NodeData.ParameterData nameParameter = new NodeData.ParameterData("Name", DomainIDs.ParameterName, BaseTypeString.ParameterType, NO_CONFIG);
                NodeData.ParameterData integerTypeParameter = new NodeData.ParameterData("Type", DomainIDs.PARAMETER_TYPE, IntegerSetGuid, NO_CONFIG, BaseTypeInteger.PARAMETER_TYPE.Guid.ToString());
                NodeData.ParameterData decimalTypeParameter = new NodeData.ParameterData("Type", DomainIDs.PARAMETER_TYPE, DecimalSetGuid, NO_CONFIG, BaseTypeDecimal.PARAMETER_TYPE.Guid.ToString());
                NodeData.ParameterData dynamicEnumTypeParameter = new NodeData.ParameterData("Type", DomainIDs.PARAMETER_TYPE, DynamicEnumSetGuid, NO_CONFIG);
                NodeData.ParameterData localDynamicEnumTypeParameter = new NodeData.ParameterData("Type", DomainIDs.PARAMETER_TYPE, LocalDynamicEnumSetGuid, NO_CONFIG);
                NodeData.ParameterData integerDefaultParameter = new NodeData.ParameterData("Default", DomainIDs.ParameterDefault, BaseTypeInteger.PARAMETER_TYPE, NO_CONFIG);
                NodeData.ParameterData decimalDefaultParameter = new NodeData.ParameterData("Default", DomainIDs.ParameterDefault, BaseTypeDecimal.PARAMETER_TYPE, NO_CONFIG);
                NodeData.ParameterData stringDefaultParameter = new NodeData.ParameterData("Default", DomainIDs.ParameterDefault, BaseTypeString.ParameterType, NO_CONFIG);
                NodeData.ParameterData booleanDefaultParameter = new NodeData.ParameterData("Default", DomainIDs.ParameterDefault, BaseTypeBoolean.PARAMETER_TYPE, NO_CONFIG);
                NodeData.ParameterData enumTypeParameter = new NodeData.ParameterData("Type", DomainIDs.PARAMETER_TYPE, EnumSetGuid, NO_CONFIG);

                List<NodeData.ConnectorData> parameterConnectors = new List<NodeData.ConnectorData> { parameterOutput, parameterConfigConnector };

                AddNode(BaseType.Integer.ParameterNodeType, "Integer", parameterMenu, MakeConfig('p', "00aaaa"), parameterConnectors, new List<NodeData.ParameterData> { nameParameter, integerTypeParameter, integerDefaultParameter });
                AddNode(BaseType.Decimal.ParameterNodeType, "Decimal", parameterMenu, MakeConfig('p', "00aaaa"), parameterConnectors, new List<NodeData.ParameterData> { nameParameter, decimalTypeParameter, decimalDefaultParameter });
                AddNode(BaseType.String.ParameterNodeType, "String", parameterMenu, MakeConfig('p', "00aaaa"), parameterConnectors, new List<NodeData.ParameterData> { nameParameter, stringDefaultParameter });
                AddNode(BaseType.LocalizedString.ParameterNodeType, "Localized String", parameterMenu, MakeConfig('p', "00aaaa"), parameterConnectors, new List<NodeData.ParameterData> { nameParameter });
                AddNode(BaseType.Boolean.ParameterNodeType, "Boolean", parameterMenu, MakeConfig('p', "00aaaa"), parameterConnectors, new List<NodeData.ParameterData> { nameParameter, booleanDefaultParameter });
                AddNode(BaseType.Audio.ParameterNodeType, "Audio", parameterMenu, MakeConfig('p', "00aaaa"), parameterConnectors, new List<NodeData.ParameterData> { nameParameter });
                AddNode(BaseType.DynamicEnumeration.ParameterNodeType, "Dynamic Enumeration", parameterMenu, MakeConfig('p', "00aaaa"), parameterConnectors, new List<NodeData.ParameterData> { nameParameter, dynamicEnumTypeParameter, stringDefaultParameter });
                AddNode(BaseType.LocalDynamicEnumeration.ParameterNodeType, "Local Dynamic Enumeration", parameterMenu, MakeConfig('p', "00aaaa"), parameterConnectors, new List<NodeData.ParameterData> { nameParameter, localDynamicEnumTypeParameter, stringDefaultParameter });
                AddNode(BaseType.Set.ParameterNodeType, "Set", parameterMenu, MakeConfig('p', "00aaaa"), parameterConnectors, new List<NodeData.ParameterData> { nameParameter, enumTypeParameter });
                AddEnumNode(parameterMenu);
            }
            #endregion

            #region Built In Connectors
            //ID<TConnector> connectorOutput1 = ID<TConnector>.Parse("d2dea7c3-4674-4ac7-a908-a9da30610092");
            //OutputDefinition inputOutputDefinition = (data) => DomainIDs.CONNECTOR_OUTPUT_DEFINITION.Make(connectorOutput1, data, DomainConnectionRules.Instance);
            //m_inputConnector = AddNode(ID<NodeTypeTemp>.ConvertFrom(ConnectorDefinitionData.Input.Id), "Input", m_connectorsMenu, config('i', ConnectorColor), inputOutputDefinition.Only(), (id, ng, c) => new ExternalFunction(ng, id, c));
            //m_outputConnector = AddNode(ID<NodeTypeTemp>.ConvertFrom(ConnectorDefinitionData.Output.Id), "Output", m_connectorsMenu, config('o', ConnectorColor), inputOutputDefinition.Only(), (id, ng, c) => new ExternalFunction(ng, id, c, new StringParameter("Name", DomainIDs.OUTPUT_NAME, BaseTypeString.PARAMETER_TYPE)));
            RefreshConnectorsMenu();
            #endregion

            NodeType configMenu = new NodeType("Config", DomainIDs.ConfigMenu);
            m_nodeMenu.AddChildType(configMenu);

            Id<TConnector> configOutput1 = Id<TConnector>.Parse("2d88af18-d66d-4f86-868e-5db9e020c99d");
            var configConnector = new NodeData.ConnectorData(configOutput1, DomainIDs.ConfigOutputDefinition.Id, new List<Parameter>());

            IEnumerable<IConfigNodeDefinition> configNodeDefitions = m_pluginsConfig.GetConfigDefinitions();
            foreach (var configNodeDefinition in configNodeDefitions)
            {
                var cnd = configNodeDefinition;
                AddNode(cnd.Id, cnd.Name, configMenu, MakeConfig('c', "aabb00"), new List<NodeData.ConnectorData> { configConnector }, new List<NodeData.ParameterData>(), () => cnd.MakeParameters().ToList());
            }

            var category = new NodeData.ParameterData("Category", DomainIDs.NodeCategory, DomainIDs.CategoryType, NO_CONFIG, DomainIDs.CategoryNone.ToString());
            var nodeName = new NodeData.ParameterData("Name", DomainIDs.NodeName, BaseTypeString.ParameterType, NO_CONFIG);
            AddNode(DomainIDs.NodeGuid, "Node", m_nodeMenu, MakeConfig('n', "808080"), NodeConnectors, new List<NodeData.ParameterData> { nodeName, category });

            AddAutoCompleteNodes();

            ConnectorDefinitions.Modified += () => RefreshConnectorsMenu();
        }

        internal IEnumerable<EnumerationData.Element> GetEnumOptions(Guid value)
        {
            return m_typeSet.VisibleEnums.Where(e => e.TypeID == ParameterType.Basic.FromGuid(value)).Single().Elements;
        }

        private void AddAutoCompleteNodes()
        {
            m_autoCompleteMenu = new NodeType("Autocomplete", DomainIDs.AutoComplete.Menu);
            m_nodeHeirarchy.AddChildType(m_autoCompleteMenu);

            NodeData.ParameterData enumTypeParameter = new NodeData.ParameterData("Type", DomainIDs.PARAMETER_TYPE, EnumSetGuid, NO_CONFIG);
            NodeData.ParameterData dynamicEnumTypeParameter = new NodeData.ParameterData("Type", DomainIDs.PARAMETER_TYPE, DynamicEnumSetGuid, NO_CONFIG);
            NodeData.ParameterData localDynamicEnumSetGuid = new NodeData.ParameterData("Type", DomainIDs.PARAMETER_TYPE, LocalDynamicEnumSetGuid, NO_CONFIG);

            AddNode(DomainIDs.AutoComplete.ZeroOrMore, "ZeroOrMore", m_autoCompleteMenu, MakeConfig('\0', "808080"), new List<NodeData.ConnectorData>() { DomainIDs.AutoComplete.child, DomainIDs.AutoComplete.parent, DomainIDs.AutoComplete.next, DomainIDs.AutoComplete.previous }, new List<NodeData.ParameterData>());
            AddNode(DomainIDs.AutoComplete.OneOrMore, "OneOrMore", m_autoCompleteMenu, MakeConfig('\0', "808080"), new List<NodeData.ConnectorData>() { DomainIDs.AutoComplete.child, DomainIDs.AutoComplete.parent, DomainIDs.AutoComplete.next, DomainIDs.AutoComplete.previous }, new List<NodeData.ParameterData>());
            AddNode(DomainIDs.AutoComplete.ZeroOrOne, "ZeroOrOne", m_autoCompleteMenu, MakeConfig('\0', "808080"), new List<NodeData.ConnectorData>() { DomainIDs.AutoComplete.child, DomainIDs.AutoComplete.parent, DomainIDs.AutoComplete.next, DomainIDs.AutoComplete.previous }, new List<NodeData.ParameterData>());
            AddNode(DomainIDs.AutoComplete.ExactlyOne, "ExactlyOne", m_autoCompleteMenu, MakeConfig('\0', "808080"), new List<NodeData.ConnectorData>() { DomainIDs.AutoComplete.child, DomainIDs.AutoComplete.parent, DomainIDs.AutoComplete.next, DomainIDs.AutoComplete.previous }, new List<NodeData.ParameterData>());
            AddNode(DomainIDs.AutoComplete.String, "String", m_autoCompleteMenu, MakeConfig('\0', "808080"), new List<NodeData.ConnectorData>() { DomainIDs.AutoComplete.parent }, new List<NodeData.ParameterData>() { new NodeData.ParameterData("value", Id<Parameter>.Parse("db341485-8177-4194-be8d-f2a7dafa1c6d"), BaseTypeString.ParameterType, NO_CONFIG) });
            AddNode(DomainIDs.AutoComplete.Character, "Character", m_autoCompleteMenu, MakeConfig('\0', "808080"), new List<NodeData.ConnectorData>() { DomainIDs.AutoComplete.parent }, new List<NodeData.ParameterData>() { new NodeData.ParameterData("value", Id<Parameter>.Parse("cb13c985-559f-44bd-8ef8-56ba6ea8f74a"), BaseTypeString.ParameterType, NO_CONFIG) });
            AddNode(DomainIDs.AutoComplete.EnumerationValue, "EnumerationValue", m_autoCompleteMenu, MakeConfig('\0', "808080"), new List<NodeData.ConnectorData>() { DomainIDs.AutoComplete.parent }, new List<NodeData.ParameterData>() { enumTypeParameter });
            AddNode(DomainIDs.AutoComplete.DynamicEnumerationValue, "DynamicEnumerationValue", m_autoCompleteMenu, MakeConfig('\0', "808080"), new List<NodeData.ConnectorData>() { DomainIDs.AutoComplete.parent }, new List<NodeData.ParameterData>() { dynamicEnumTypeParameter });
            AddNode(DomainIDs.AutoComplete.LocalDynamicEnumerationValue, "LocalDynamicEnumerationValue", m_autoCompleteMenu, MakeConfig('\0', "808080"), new List<NodeData.ConnectorData>() { DomainIDs.AutoComplete.parent }, new List<NodeData.ParameterData>() { localDynamicEnumSetGuid });
        }

        public static List<NodeData.ConnectorData> NodeConnectors
        {
            get
            {
                return new List<NodeData.ConnectorData>
                {
                    new NodeData.ConnectorData(Id<TConnector>.Parse("cdfa9a9e-e6e9-4b9a-b4ff-683ce6e4ad9d"),DomainIDs.NodeOutputConfigDefinition.Id, new List<Parameter>()),
                    new NodeData.ConnectorData(Id<TConnector>.Parse("28067d32-aa0e-4063-8985-fea92e64f2f5"),DomainIDs.NodeOutputParametersDefinition.Id, new List<Parameter>()),
                    new NodeData.ConnectorData(Id<TConnector>.Parse("34417028-db60-4483-8c31-e817f0d2548c"),DomainIDs.NodeOutputConnectorsDefinition.Id, new List<Parameter>()),
                };
            }
        }

        private static string ConnectorColor = "ffff00";

        public class EnumDefaultParameter : Parameter, IDynamicEnumParameter
        {
            public new static readonly ParameterType TypeId = ParameterType.Parse("82e83436-f1b0-4f71-8882-51c171d14ff3");

            Func<Dictionary<ParameterType, IEnumerable<EnumerationData.Element>>> m_enumOptions;

            private Dictionary<ParameterType, IEnumerable<EnumerationData.Element>> EnumOptions { get { return m_enumOptions(); } }

            Func<ParameterType> m_currentEnumType;

            Guid m_valueGuid;
            string m_value;

            public EnumDefaultParameter(Func<Dictionary<ParameterType, IEnumerable<EnumerationData.Element>>> enumOptions, Func<ParameterType> currentEnumType)
                : base("Default", DomainIDs.ParameterDefault, TypeId, null)
            {
                m_value = "";
                m_valueGuid = Guid.Empty;
                m_enumOptions = enumOptions;
                m_currentEnumType = currentEnumType;
            }

            private void UpdateValueGuid()
            {
                m_valueGuid = EnumOptions[m_currentEnumType()].FirstOrDefault(a => a.Name == m_value).Guid;
            }

            private void UpdateText()
            {
                string newText = null;
                if (m_valueGuid != Guid.Empty && EnumOptions.ContainsKey(m_currentEnumType()))
                {
                    var element = EnumOptions[m_currentEnumType()].FirstOrDefault(a => a.Guid == m_valueGuid);
                    if (element.Guid != Guid.Empty)
                        newText = element.Name;
                }
                m_value = newText ?? m_value;
            }

            public string Value
            {
                get
                {
                    UpdateText();
                    return m_value;
                }
            }

            public Either<Guid, string> BetterValue
            {
                get
                {
                    UpdateText();
                    if (m_valueGuid != Guid.Empty)
                        return m_valueGuid;
                    else
                        return m_value;
                }
            }

            public SimpleUndoPair? SetValueAction(string value)
            {
                var oldGuid = m_valueGuid;
                var oldValue = m_value;
                var oldCorrupted = Corrupted;

                if (value.Equals(oldValue) && !oldCorrupted)
                    return null;

                return new SimpleUndoPair
                {
                    Redo = () => { m_value = value; Corrupted = false; UpdateValueGuid(); },
                    Undo = () => { m_value = oldValue; Corrupted = oldCorrupted; m_valueGuid = oldGuid; }
                };
            }

            public override string DisplayValue(Func<Id<LocalizedText>, string> localize)
            {
                UpdateText();
                return m_value;
            }

            protected override string InnerValueAsString()
            {
                UpdateText();
                if (m_valueGuid != Guid.Empty)
                    return m_valueGuid.ToString();
                else
                    return m_value;
            }

            protected override bool DeserialiseValue(string value)
            {
                if (Guid.TryParse(value, out m_valueGuid))
                {
                    m_value = null;
                }
                else
                {
                    m_valueGuid = Guid.Empty;
                    m_value = value;
                }
                return true;
            }

            public IEnumerable<string> Options
            {
                get
                {
                    ParameterType guid = m_currentEnumType();
                    if (EnumOptions.ContainsKey(guid))
                        return EnumOptions[guid].Select(a => a.Name);
                    else
                        return new string[0];
                }
            }

            public bool Local
            {
                get
                {
                    return false;
                }
            }

            protected override void DecorruptFromNull()
            {
                //TODO: Do we need to do something here?
            }

            public void MergeInto(DynamicEnumParameter.Source newSource)
            {
                //TODO: Not really sure what to do about this
                //Contract.Assert(newSource == null, "source should be null because we're not merging into it");
                //Do nothing. This object maintains its own source and nothing needs to know about it
            }
        }

        private EditableGenerator AddEnumNode(NodeType parent)
        {
            Id<NodeTypeTemp> guid = BaseType.Enumeration.ParameterNodeType;
            string name = "Enumeration";
            NodeData.ConnectorData parameterOutput = new NodeData.ConnectorData(parameterDefinitionConnector1, DomainIDs.ParameterOutputDefinition.Id, new List<Parameter>());
            NodeData.ConnectorData parameterConfigConnector = new NodeData.ConnectorData(parameterConfigConnectorID, DomainIDs.ParameterConfigConnectorDefinition.Id, new List<Parameter>());
            List<NodeData.ConnectorData> parameterConnectors = new List<NodeData.ConnectorData> { parameterOutput, parameterConfigConnector };
            NodeData.ParameterData nameParameter = new NodeData.ParameterData("Name", DomainIDs.ParameterName, BaseTypeString.ParameterType, NO_CONFIG);
            NodeData.ParameterData enumTypeParameter = new NodeData.ParameterData("Type", DomainIDs.PARAMETER_TYPE, EnumSetGuid, NO_CONFIG);

            NodeData data;
            data.Config = MakeConfig('p', "00aaaa");
            data.Connectors = parameterConnectors;
            data.Guid = guid;
            data.Name = name;
            data.Parameters = new List<NodeData.ParameterData> { nameParameter, enumTypeParameter };
            data.Type = parent.Guid;

            Func<Dictionary<ParameterType, IEnumerable<EnumerationData.Element>>> options = () => m_typeSet.VisibleEnums.ToDictionary(e => e.TypeID, e => e.Elements.Select(a => a));

            var generator = new GenericEditableGenerator2(data, m_typeSet, ConnectorDefinitions, DomainConnectionRules.Instance,
                p => new List<Parameter> { new EnumDefaultParameter(options, () => ParameterType.Basic.FromGuid((p.Single(a => a.Id == enumTypeParameter.Id) as IEnumParameter).EditorSelected)) });
            parent.m_nodes.Add(generator);
            m_nodes[guid] = generator;
            return generator;
        }

        private EditableGenerator AddNode(Id<NodeTypeTemp> guid, string name, NodeType parent, List<NodeData.ConfigData> config, List<NodeData.ConnectorData> connectors, List<NodeData.ParameterData> parameters, Func<List<Parameter>> extraParameters)
        {
            NodeData data;
            data.Config = config;
            data.Connectors = connectors;
            data.Guid = guid;
            data.Name = name;
            data.Parameters = parameters;
            data.Type = parent.Guid;

            var generator = new GenericEditableGenerator2(data, m_typeSet, ConnectorDefinitions, DomainConnectionRules.Instance, x => extraParameters());
            parent.m_nodes.Add(generator);
            m_nodes[guid] = generator;
            return generator;
        }

        private EditableGenerator AddNode(Id<NodeTypeTemp> guid, string name, NodeType parent, List<NodeData.ConfigData> config, List<NodeData.ConnectorData> connectors, List<NodeData.ParameterData> parameters)
        {
            NodeData data;
            data.Config = config;
            data.Connectors = connectors;
            data.Guid = guid;
            data.Name = name;
            data.Parameters = parameters;
            data.Type = parent.Guid;

            var generator = new GenericEditableGenerator2(data, m_typeSet, ConnectorDefinitions, DomainConnectionRules.Instance);
            parent.m_nodes.Add(generator);
            m_nodes[guid] = generator;
            return generator;
        }

        public IEnumerable<ParameterType> ParameterTypes
        {
            get { throw new NotImplementedException(); }
        }

        public INodeType Nodes
        {
            get { return m_nodeHeirarchy; }
        }

        public EditableGenerator GetNode(Id<NodeTypeTemp> guid)
        {
            if (m_nodes.ContainsKey(guid))
                return m_nodes[guid];
            else
                return null;
        }

        public void RenameCategory(string name, Guid guid)
        {
            EnumerationData data = m_typeSet.GetEnumData(DomainIDs.CategoryType);
            var index = data.Elements.IndexOf(e => e.Guid == guid);
            var option = data.Elements[index];
            option.Name = name;
            data.Elements[index] = option;
            UpdateEnumeration(data);
            //m_categories.SetName(guid, name);
        }

        public void AddCategory(string name, Guid guid)
        {
            EnumerationData data = m_typeSet.GetEnumData(DomainIDs.CategoryType);
            data.Elements.Add(new EnumerationData.Element(name, guid));
            UpdateEnumeration(data);
            //m_categories.Add(guid, name);
        }

        public void RemoveCategory(Guid guid)
        {
            EnumerationData data = m_typeSet.GetEnumData(DomainIDs.CategoryType);
            data.Elements.RemoveAll(e => e.Guid == guid);
            UpdateEnumeration(data);
            //m_categories.Remove(guid);
        }

        public void RenameType(BaseType baseType, string name, ParameterType guid)
        {
            m_typeSet.RenameType(guid, name);
            //m_types.RenameType(baseType, name, guid);
        }

        public void AddDecimalType(DecimalData data)
        {
            m_typeSet.AddDecimal(data);
            //m_types.Decimals.Add(data.TypeID, data.Name, data);
        }

        public void AddIntegerType(IntegerData data)
        {
            m_typeSet.AddInteger(data);
            //m_types.Integers.Add(data.TypeID, data.Name, data);
        }

        public void AddEnumType(EnumerationData data)
        {
            m_typeSet.AddEnum(data, false);
            //m_types.Enumerations.Add(data.Guid, data.Name, data);
        }

        public void AddDynamicEnumType(DynamicEnumerationData data)
        {
            m_typeSet.AddDynamicEnum(data);
            //m_types.DynamicEnumerations.Add(data.TypeID, data.Name, data);
        }

        public void AddLocalDynamicEnumType(LocalDynamicEnumerationData data)
        {
            m_typeSet.AddLocalDynamicEnum(data);
            //m_types.LocalDynamicEnumerations.Add(data.TypeID, data.Name, data);
        }

        public void RemoveType(BaseType baseType, ParameterType guid)
        {
            m_typeSet.Remove(guid);
            //m_types.RemoveType(baseType, guid);
        }

        public static void ForEachNode(IEnumerable<ConversationNode> nodes, Action<NodeTypeData> categoryAction, Action<IntegerData> integerAction, Action<DecimalData> decimalAction, Action<DynamicEnumerationData> dynamicEnumAction, Action<LocalDynamicEnumerationData> localDynamicEnumAction, Action<EnumerationData> enumerationAction, Action<EnumerationData> enumerationValueAction, Action<NodeData> nodeAction, Action<ConnectorDefinitionData> connectorAction, Action<ConnectionDefinitionData> connectionAction)
        {
            ForEachNode(nodes.Select(n => n.m_data), categoryAction, integerAction, decimalAction, dynamicEnumAction, localDynamicEnumAction, enumerationAction, enumerationValueAction, nodeAction, connectorAction, connectionAction);
        }
        public static void ForEachNode(IEnumerable<IEditable> nodes, Action<NodeTypeData> categoryAction, Action<IntegerData> integerAction, Action<DecimalData> decimalAction, Action<DynamicEnumerationData> dynamicEnumAction, Action<LocalDynamicEnumerationData> localDynamicEnumAction, Action<EnumerationData> enumerationAction, Action<EnumerationData> enumerationValueAction, Action<NodeData> nodeAction, Action<ConnectorDefinitionData> connectorAction, Action<ConnectionDefinitionData> connectionAction)
        {
            foreach (var node in nodes.OrderBy(n => n.NodeTypeId == DomainIDs.NodeGuid ? 2 : 1))
            {
                if (node.NodeTypeId == DomainIDs.CategoryGuid)
                {
                    var nameParameter = node.Parameters.Single(p => p.Id == DomainIDs.CategoryName) as IStringParameter;
                    var name = nameParameter.Value;
                    var parentParameter = node.Parameters.Single(p => p.Id == DomainIDs.CategoryParent) as IEnumParameter;
                    var parent = parentParameter.Value;
                    categoryAction(new NodeTypeData(name, node.NodeId.Guid, parent));
                }
                else if (node.NodeTypeId == BaseType.Integer.NodeType)
                {
                    var nameParameter = node.Parameters.Single(p => p.Id == DomainIDs.IntegerName) as IStringParameter;
                    var name = nameParameter.Value;
                    var minParameter = node.Parameters.Single(p => p.Id == DomainIDs.IntegerMin) as IIntegerParameter;
                    var min = minParameter.Value;
                    var maxParameter = node.Parameters.Single(p => p.Id == DomainIDs.IntegerMax) as IIntegerParameter;
                    var max = maxParameter.Value;
                    //var defParameter = node.Parameters.Single(p => p.Guid == DomainIDs.INTEGER_DEFAULT) as IIntegerParameter;
                    //var def = defParameter.Value;
                    integerAction(new IntegerData(name, ParameterType.Basic.FromGuid(node.NodeId.Guid), max, min/*, def*/));
                }
                else if (node.NodeTypeId == BaseType.Decimal.NodeType)
                {
                    var nameParameter = node.Parameters.Single(p => p.Id == DomainIDs.DecimalName) as IStringParameter;
                    var name = nameParameter.Value;
                    var minParameter = node.Parameters.Single(p => p.Id == DomainIDs.DecimalMin) as IDecimalParameter;
                    var min = minParameter.Value;
                    var maxParameter = node.Parameters.Single(p => p.Id == DomainIDs.DecimalMax) as IDecimalParameter;
                    var max = maxParameter.Value;
                    //var defParameter = node.Parameters.Single(p => p.Guid == DomainIDs.DECIMAL_DEFAULT) as IDecimalParameter;
                    //var def = defParameter.Value;
                    decimalAction(new DecimalData(name, ParameterType.Basic.FromGuid(node.NodeId.Guid), max, min/*, def*/));
                }
                else if (node.NodeTypeId == BaseType.DynamicEnumeration.NodeType)
                {
                    var nameParameter = node.Parameters.Single(p => p.Id == DomainIDs.DynamicEnumName) as IStringParameter;
                    var name = nameParameter.Value;
                    dynamicEnumAction(new DynamicEnumerationData(name, ParameterType.Basic.FromGuid(node.NodeId.Guid)));
                }
                else if (node.NodeTypeId == BaseType.LocalDynamicEnumeration.NodeType)
                {
                    var nameParameter = node.Parameters.Single(p => p.Id == DomainIDs.LocalDynamicEnumName) as IStringParameter;
                    var name = nameParameter.Value;
                    localDynamicEnumAction(new LocalDynamicEnumerationData(name, ParameterType.Basic.FromGuid(node.NodeId.Guid)));
                }
                else if (node.NodeTypeId == BaseType.Enumeration.NodeType)
                {
                    ForEnumDeclaration(enumerationAction, node);
                }
                else if (node.NodeTypeId == DomainIDs.EnumerationValueDeclaration)
                {
                    foreach (var enumTypeNode in node.Connectors.SelectMany(t => t.Connections).Select(t => t.Parent).Where(n => n.NodeTypeId == BaseType.Enumeration.NodeType))
                    {
                        ForEnumDeclaration(enumerationValueAction, enumTypeNode);
                    }
                }
                else if (node.NodeTypeId == DomainIDs.NodeGuid)
                {
                    var nameParameter = node.Parameters.Single(p => p.Id == DomainIDs.NodeName) as IStringParameter;
                    var name = nameParameter.Value;
                    var categoryParameter = node.Parameters.Single(p => p.Id == DomainIDs.NodeCategory) as IEnumParameter;
                    var category = categoryParameter.Value;

                    List<NodeData.ConnectorData> connectors = new List<NodeData.ConnectorData>();
                    foreach (var connectorNode in node.Connectors.Single(c => c.m_definition.Id == DomainIDs.NodeOutputConnectorsDefinition.Id).Connections)
                    {
                        connectors.Add(new NodeData.ConnectorData(Id<TConnector>.ConvertFrom(connectorNode.Parent.NodeId), Id<TConnectorDefinition>.ConvertFrom(connectorNode.Parent.NodeTypeId), connectorNode.Parent.Parameters.ToList()));
                    }

                    List<NodeData.ParameterData> parameters = new List<NodeData.ParameterData>();
                    var parametersConnector = node.Connectors.Single(c => c.m_definition.Id == DomainIDs.NodeOutputParametersDefinition.Id);
                    IEnumerable<IEditable> parameterNodes = parametersConnector.Connections.Select(l => l.Parent);
                    foreach (var parameterNode in parameterNodes)//.Where(n => BaseType.TypeExists(n.NodeTypeID)))
                    {
                        parameters.Add(BaseType.GetType(parameterNode.NodeTypeId).ReadDomainNode(parameterNode));
                    }

                    List<NodeData.ConfigData> config = new List<NodeData.ConfigData>();
                    var configs = node.Connectors.Single(c => c.m_definition.Id == DomainIDs.NodeOutputConfigDefinition.Id).Connections.Select(l => l.Parent);
                    foreach (var configNode in configs)
                    {
                        config.Add(new NodeData.ConfigData(configNode.NodeTypeId, configNode.Parameters));
                    }

                    nodeAction(new NodeData(name, category, Id<NodeTypeTemp>.FromGuid(node.NodeId.Guid), connectors, parameters, config));
                }
                else if (node.NodeTypeId == DomainIDs.ConnectorDefinitionGuid)
                {
                    var nameParameter = node.Parameters.Single(p => p.Id == DomainIDs.ConnectorDefinitionName) as IStringParameter;
                    var positionParameter = node.Parameters.Single(p => p.Id == ConnectorPosition.ParameterId) as IEnumParameter;
                    var name = nameParameter.Value;

                    var linkedNodes = node.Connectors.SelectMany(n => n.Connections.Select(l => l.Parent));
                    List<NodeData.ParameterData> parameters = new List<NodeData.ParameterData>();
                    foreach (var parameterNode in linkedNodes.Where(n => BaseType.TypeExists(n.NodeTypeId)))
                    {
                        parameters.Add(BaseType.GetType(parameterNode.NodeTypeId).ReadDomainNode(parameterNode));
                    }

                    connectorAction(new ConnectorDefinitionData(name, Id<TConnectorDefinition>.ConvertFrom(node.NodeId), parameters, ConnectorPosition.Read(positionParameter)));
                }
                else if (node.NodeTypeId == DomainIDs.ConnectionDefinitionGuid)
                {
                    var connector1Parameter = node.Parameters.Single(p => p.Id == DomainIDs.ConnectionDefinitionConnector1) as IEnumParameter;
                    var connector2Parameter = node.Parameters.Single(p => p.Id == DomainIDs.ConnectionDefinitionConnector2) as IEnumParameter;
                    connectionAction(new ConnectionDefinitionData(UnorderedTuple.Make(Id<TConnectorDefinition>.FromGuid(connector1Parameter.Value), Id<TConnectorDefinition>.FromGuid(connector2Parameter.Value))));
                }
            }
        }

        private static void ForEnumDeclaration(Action<EnumerationData> enumerationAction, IEditable node)
        {
            var nameParameter = node.Parameters.Single(p => p.Id == DomainIDs.EnumerationName) as IStringParameter;
            var name = nameParameter.Value;
            //var defParameter = node.Parameters.Single(p => p.Guid == DomainIds.PARAMETER_DEFAULT) as IDynamicEnumParameter;
            //var def = defParameter.Value;
            //var links = node.TransitionsOut.Single().Connections;

            IEnumerable<Output> links = node.Connectors.SelectMany(n => n.Connections);
            List<EnumerationData.Element> elements = new List<EnumerationData.Element>();
            foreach (Output link in links.Where(l => l.Parent.NodeTypeId == DomainIDs.EnumerationValueDeclaration))
            {
                var valueParameter = link.Parent.Parameters.Single(p => p.Id == DomainIDs.EnumerationValueParameter) as IStringParameter;
                elements.Add(new EnumerationData.Element(valueParameter.Value, link.Parent.NodeId.Guid));
            }
            enumerationAction(new EnumerationData(name, ParameterType.Basic.ConvertFrom(node.NodeId), elements/*, def*/));
        }

        internal void UpdateEnumeration(EnumerationData data)
        {
            //m_types.UpdateEnumOptions(data.Guid, data.Elements);
            m_typeSet.ModifyEnum(data);
        }

        public bool IsInteger(ParameterType type)
        {
            return m_typeSet.IsInteger(type);
        }

        public bool IsDecimal(ParameterType type)
        {
            return m_typeSet.IsDecimal(type);
        }

        public bool IsEnum(ParameterType type)
        {
            return m_typeSet.IsEnum(type);
        }

        public bool IsDynamicEnum(ParameterType type)
        {
            return m_typeSet.IsDynamicEnum(type) || type == EnumDefaultParameter.TypeId;
        }

        public bool IsLocalDynamicEnum(ParameterType type)
        {
            return m_typeSet.IsLocalDynamicEnum(type) || type == EnumDefaultParameter.TypeId;
        }

        //CallbackDictionary<ID<TConnectorDefinition>, ConnectorDefinitionData> m_connectorDefinitions = new CallbackDictionary<ID<TConnectorDefinition>, ConnectorDefinitionData>()
        //{
        //    { SpecialConnectors.Input.Id, SpecialConnectors.Input },
        //    { SpecialConnectors.Output.Id, SpecialConnectors.Output },
        //};
        //private GenericEditableGenerator m_inputConnector;
        //private GenericEditableGenerator m_outputConnector;

        internal void ModifyConnector(ConnectorDefinitionData cdd)
        {
            ConnectorDefinitions[cdd.Id] = cdd;

            EnumerationData data = m_typeSet.GetEnumData(DomainIDs.ConnectionType);
            var index = data.Elements.IndexOf(e => e.Guid == cdd.Id.Guid);
            var option = data.Elements[index];
            option.Name = cdd.Name;
            data.Elements[index] = option;
            UpdateEnumeration(data);
        }

        internal void AddConnector(ConnectorDefinitionData cdd)
        {
            ConnectorDefinitions.Add(cdd.Id, cdd);

            EnumerationData data = m_typeSet.GetEnumData(DomainIDs.ConnectionType);
            data.Elements.Add(new EnumerationData.Element(cdd.Name, cdd.Id.Guid));
            UpdateEnumeration(data);
        }

        internal void RemoveConnector(ConnectorDefinitionData cdd)
        {
            ConnectorDefinitions.Remove(cdd.Id);

            EnumerationData data = m_typeSet.GetEnumData(DomainIDs.ConnectionType);
            data.Elements.RemoveAll(e => e.Guid == cdd.Id.Guid);
            UpdateEnumeration(data);
        }

        private void RefreshConnectorsMenu()
        {
            m_connectorsMenu.m_nodes.Clear();
            foreach (var data in ConnectorDefinitions.Values)
            {
                if (!data.Hidden)
                {
                    var d = data;
                    NodeData.ConnectorData connector = new NodeData.ConnectorData(Id<TConnector>.Parse("d7ac3a74-206d-48d5-b40f-30bc16dfdb67"), DomainIDs.ConnectorOutputDefinition.Id, new List<Parameter>());
                    AddNode(Id<NodeTypeTemp>.ConvertFrom(d.Id), d.Name, m_connectorsMenu, MakeConfig('\0', ConnectorColor), new List<NodeData.ConnectorData> { connector }, d.Parameters.ToList());
                }
            }
        }

        private static List<NodeData.ConfigData> MakeConfig(char shortcut, string color)
        {
            List<NodeData.ConfigData> result = new List<NodeData.ConfigData>();
            if (shortcut != '\0')
            {
                result.Add(GenericNodeConfigDefinition.Make("Shortcut", shortcut.ToString()));
            }
            result.Add(GenericNodeConfigDefinition.Make("Color", color));
            return result;
        }

        public bool IsCategoryDefinition(Id<NodeTypeTemp> id)
        {
            return id == DomainIDs.CategoryGuid;
        }

        public bool IsTypeDefinition(Id<NodeTypeTemp> id)
        {
            return TypeDefinitionNodeIds.All.Contains(id);
        }

        public bool IsConnectorDefinition(Id<NodeTypeTemp> id)
        {
            if (id == DomainIDs.ConnectorDefinitionGuid)
                return true;
            else if (BaseType.BaseTypes.Any(t => id == t.ParameterNodeType))
                return true;
            else
                return false;
        }

        public bool IsParameter(Id<NodeTypeTemp> id)
        {
            return BaseType.BaseTypes.Any(t => id == t.ParameterNodeType);
        }

        public bool IsConnector(Id<NodeTypeTemp> id)
        {
            return ConnectorDefinitions.Values.Any(d => id == Id<NodeTypeTemp>.ConvertFrom(d.Id));
        }

        public bool IsNodeDefinition(Id<NodeTypeTemp> id)
        {
            if (id == DomainIDs.NodeGuid)
                return true;
            else if (IsParameter(id))
                return true;
            //else if (id == m_inputConnector.Guid)
            //    return true;
            //else if (id == m_outputConnector.Guid)
            //    return true;
            else if (IsConnector(id))
                return true;
            else
                return false;
        }

        public bool IsAutoCompleteNode(Id<NodeTypeTemp> id)
        {
            return DomainIDs.AutoComplete.IsAutoCompleteNode(id);
        }

        public static bool IsConnector(IDataSource datasource, Id<NodeTypeTemp> id)
        {
            //TODO: This seems wrong
            if (!datasource.IsNodeDefinition(id))
                return false;
            if (id == DomainIDs.NodeGuid)
                return false;
            if (BaseType.BaseTypes.Any(t => id == t.ParameterNodeType))
                return false;
            return true;
        }

        internal bool IsConfig(Id<NodeTypeTemp> iD)
        {
            IEnumerable<IConfigNodeDefinition> configNodeDefitions = m_pluginsConfig.GetConfigDefinitions();
            foreach (var configNodeDefinition in configNodeDefitions)
            {
                if (configNodeDefinition.Id == iD)
                    return true;
            }
            return false;
        }

        public CallbackDictionary<Id<TConnectorDefinition>, ConnectorDefinitionData> ConnectorDefinitions = new CallbackDictionary<Id<TConnectorDefinition>, ConnectorDefinitionData>()
        {
            { SpecialConnectors.Input.Id, SpecialConnectors.Input },
            { SpecialConnectors.Output.Id, SpecialConnectors.Output },
            { DomainIDs.ParameterOutputDefinition                                .Id,       DomainIDs.ParameterOutputDefinition                     },
            { DomainIDs.ParameterConfigConnectorDefinition                      .Id,       DomainIDs.ParameterConfigConnectorDefinition           },
            { DomainIDs.ConnectorOutputDefinition                                .Id,       DomainIDs.ConnectorOutputDefinition                     },
            { DomainIDs.ConfigOutputDefinition                                   .Id,       DomainIDs.ConfigOutputDefinition                        },
            { DomainIDs.NodeOutputConfigDefinition                              .Id,       DomainIDs.NodeOutputConfigDefinition                   },
            { DomainIDs.NodeOutputParametersDefinition                          .Id,       DomainIDs.NodeOutputParametersDefinition               },
            { DomainIDs.NodeOutputConnectorsDefinition                          .Id,       DomainIDs.NodeOutputConnectorsDefinition               },
            { DomainIDs.ConnectorDefiinitionOutputDefinition                     .Id,       DomainIDs.ConnectorDefiinitionOutputDefinition          },
            { DomainIDs.EnumValueOutputDefinition                               .Id,       DomainIDs.EnumValueOutputDefinition                    },
            { DomainIDs.EnumOutputDefinition                                     .Id,       DomainIDs.EnumOutputDefinition                          },
            //{ DomainIDs.CONNECTOR_DEFINITION_CONNECTION_DEFINITION                 .Id,       DomainIDs.CONNECTOR_DEFINITION_CONNECTION_DEFINITION      },
            //{ DomainIDs.CONNECTION_DEFINITION_CONNECTOR                            .Id,       DomainIDs.CONNECTION_DEFINITION_CONNECTOR                 },

            { DomainIDs.AutoComplete.Child.Id, DomainIDs.AutoComplete.Child },
            { DomainIDs.AutoComplete.Parent.Id, DomainIDs.AutoComplete.Parent },
            { DomainIDs.AutoComplete.Next.Id, DomainIDs.AutoComplete.Next },
            { DomainIDs.AutoComplete.Previous.Id, DomainIDs.AutoComplete.Previous },
        };
        private PluginsConfig m_pluginsConfig;

        public string GetTypeName(ParameterType type)
        {
            throw new NotImplementedException(); // Don't need an implementation for this because we won't be modifying editors for types in the domain domain
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
