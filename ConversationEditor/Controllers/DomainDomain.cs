using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;

using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGUI>;

namespace ConversationEditor
{
    public delegate Output OutputDefinition(IEditable parent);

    public class DomainDomain : IDataSource
    {
        TypeSet m_typeSet = BaseTypeSet.Make();

        public static readonly ParameterType INTEGER_SET_GUID = ParameterType.Parse("07ca7287-20c0-4ba5-ae28-e17ea97554d6");
        public static readonly ParameterType DECIMAL_SET_GUID = ParameterType.Parse("0c1e5fa8-97ff-450b-a01c-5d09ea6dbd78");
        public static readonly ParameterType ENUM_SET_GUID = ParameterType.Parse("e7526632-95ca-4981-8b45-56cea272ddd0");
        public static readonly ParameterType DYNAMIC_ENUM_SET_GUID = ParameterType.Parse("b3278dc3-6c2b-471a-a1c9-de39691af302");

        ID<TConnector> parameterDefinitionConnector1 = ID<TConnector>.Parse("1fd8a64d-271e-42b8-bfd8-85e5174bbf9d");
        ID<TConnector> parameterConfigConnectorID = ID<TConnector>.Parse("3e914fe1-c59c-4494-b53a-da135426ff72");

        NodeType m_nodeHeirarchy;
        Dictionary<ID<NodeTypeTemp>, EditableGenerator> m_nodes = new Dictionary<ID<NodeTypeTemp>, EditableGenerator>();

        NodeType m_nodeMenu;
        NodeType m_connectorsMenu;

        static readonly List<NodeData.ConnectorData> NO_CONNECTORS = new List<NodeData.ConnectorData>();
        static readonly List<NodeData.ConfigData> NO_CONFIG = new List<NodeData.ConfigData>();
        IEnumerable<OutputDefinition> NO_OUTPUT_DEFINITIONS = Enumerable.Empty<OutputDefinition>();

        public DomainDomain(PluginsConfig pluginsConfig)
        {
            m_pluginsConfig = pluginsConfig;

            EnumerationData categoryData = new EnumerationData("Categories", DomainIDs.CATEGORY_TYPE, new[] { new EnumerationData.Element("None", DomainIDs.CATEGORY_NONE) });
            m_typeSet.AddEnum(categoryData, true);

            var connectorOptions = new[] { new EnumerationData.Element(SpecialConnectors.Input.Name, SpecialConnectors.Input.Id.Guid),
                                           new EnumerationData.Element(SpecialConnectors.Output.Name, SpecialConnectors.Output.Id.Guid), };
            EnumerationData connectionData = new EnumerationData("Connections", DomainIDs.CONNECTION_TYPE, connectorOptions);
            m_typeSet.AddEnum(connectionData, true);

            //m_types.Enumerations.Add(categoryData.Guid, categoryData.Name, categoryData);
            m_typeSet.AddEnum(ConnectorPosition.PositionConnectorDefinition, true);
            EnumerationData allEnums = new EnumerationData("Enumerations", ENUM_SET_GUID, new List<EnumerationData.Element>());
            EnumerationData allDynamicEnums = new EnumerationData("Dynamic Enumerations", DYNAMIC_ENUM_SET_GUID, new List<EnumerationData.Element>());
            EnumerationData allIntegers = new EnumerationData("Integers", INTEGER_SET_GUID, new List<EnumerationData.Element> { new EnumerationData.Element("Base", BaseTypeInteger.Data.TypeID.Guid) });
            EnumerationData allDecimals = new EnumerationData("Decimals", DECIMAL_SET_GUID, new List<EnumerationData.Element> { new EnumerationData.Element("Base", BaseTypeDecimal.Data.TypeID.Guid) });
            m_typeSet.AddEnum(allEnums, true);
            m_typeSet.AddEnum(allDynamicEnums, true);
            m_typeSet.AddEnum(allIntegers, true);
            m_typeSet.AddEnum(allDecimals, true);
            //m_types.Enumerations.Add(ConnectorPosition.PositionConnectorDefinition.Guid, ConnectorPosition.PositionConnectorDefinition.Name, ConnectorPosition.PositionConnectorDefinition);


            m_typeSet.Modified += id =>
            {
                if (!(new[] { allEnums, allDynamicEnums, allIntegers, allDecimals }).Any(e => e.TypeID == id))
                {
                    allEnums.Elements = m_typeSet.VisibleEnums.Select(e => new EnumerationData.Element(e.Name, e.TypeID.Guid)).ToList();
                    m_typeSet.ModifyEnum(allEnums);

                    allDynamicEnums.Elements = m_typeSet.VisibleDynamicEnums.Select(e => new EnumerationData.Element(e.Name, e.TypeID.Guid)).ToList();
                    m_typeSet.ModifyEnum(allDynamicEnums);

                    allIntegers.Elements = m_typeSet.VisiblelIntegers.Select(e => new EnumerationData.Element(e.Name, e.TypeID.Guid)).ToList();
                    m_typeSet.ModifyEnum(allIntegers);

                    allDecimals.Elements = m_typeSet.VisibleDecimals.Select(e => new EnumerationData.Element(e.Name, e.TypeID.Guid)).ToList();
                    m_typeSet.ModifyEnum(allDecimals);
                }
            };

            //m_categories = new MutableEnumeration(categoryNone.Only(), DomainIDs.CATEGORY_TYPE, categoryNone.Item1);
            m_nodeHeirarchy = new NodeType(null, Guid.Empty);

            #region Category Definition
            {
                List<NodeData.ParameterData> parameters = new List<NodeData.ParameterData>()
                {
                    new NodeData.ParameterData("Name", DomainIDs.CATEGORY_NAME, BaseTypeString.PARAMETER_TYPE, NO_CONFIG),
                    new NodeData.ParameterData("Parent", DomainIDs.CATEGORY_PARENT, DomainIDs.CATEGORY_TYPE, NO_CONFIG),
                };

                AddNode(DomainIDs.CATEGORY_GUID, "Category", m_nodeHeirarchy, config('x', "808080"), NO_CONNECTORS, parameters);
            }
            #endregion

            #region Custom Type Definition
            List<NodeData.ParameterData> integerParameters = new List<NodeData.ParameterData>()
            {
                new NodeData.ParameterData("Name", DomainIDs.INTEGER_NAME, BaseTypeString.PARAMETER_TYPE, NO_CONFIG),
                new NodeData.ParameterData("Max", DomainIDs.INTEGER_MAX, BaseTypeInteger.PARAMETER_TYPE, NO_CONFIG),
                new NodeData.ParameterData("Min", DomainIDs.INTEGER_MIN, BaseTypeInteger.PARAMETER_TYPE, NO_CONFIG),
            };
            AddNode(BaseType.Integer.NodeType, "Integer", m_nodeHeirarchy, config('t', "808080"), NO_CONNECTORS, integerParameters);

            List<NodeData.ParameterData> decimalParameters = new List<NodeData.ParameterData>()
            {
                new NodeData.ParameterData("Name", DomainIDs.DECIMAL_NAME, BaseTypeString.PARAMETER_TYPE, NO_CONFIG),
                new NodeData.ParameterData("Max", DomainIDs.DECIMAL_MAX, BaseTypeDecimal.PARAMETER_TYPE, NO_CONFIG),
                new NodeData.ParameterData("Min", DomainIDs.DECIMAL_MIN, BaseTypeDecimal.PARAMETER_TYPE, NO_CONFIG),
            };
            AddNode(BaseType.Decimal.NodeType, "Decimal", m_nodeHeirarchy, config('d', "808080"), NO_CONNECTORS, decimalParameters);

            List<NodeData.ParameterData> dynamicEnumParameters = new List<NodeData.ParameterData>()
            {
                new NodeData.ParameterData("Name", DomainIDs.DYNAMIC_ENUM_NAME, BaseTypeString.PARAMETER_TYPE, NO_CONFIG),
            };
            AddNode(BaseType.DynamicEnumeration.NodeType, "Dynamic Enumeration", m_nodeHeirarchy, config('y', "808080"), NO_CONNECTORS, dynamicEnumParameters);

            NodeType enumerationMenu = new NodeType("Enumeration", DomainIDs.ENUMERATION_MENU);
            m_nodeHeirarchy.m_childTypes.Add(enumerationMenu);

            ID<TConnector> enumOutput1 = ID<TConnector>.Parse("8c8dc149-52c1-401e-ae1d-69b265a6841e");
            List<NodeData.ConnectorData> enumerationConnectors = new List<NodeData.ConnectorData>()
            {
                new NodeData.ConnectorData(enumOutput1, DomainIDs.ENUM_OUTPUT_DEFINITION.Id, new List<Parameter>()),
            };
            List<NodeData.ParameterData> enumerationParameters = new List<NodeData.ParameterData>()
            {
                new NodeData.ParameterData("Name", DomainIDs.ENUMERATION_NAME, BaseTypeString.PARAMETER_TYPE, NO_CONFIG),
            };
            AddNode(BaseType.Enumeration.NodeType, "Enumeration", enumerationMenu, config('e', "808080"), enumerationConnectors, enumerationParameters);

            ID<TConnector> enumerationValueOutput1 = ID<TConnector>.Parse("ef845d1a-11a2-45d1-bab2-de8104b46c51");
            List<NodeData.ConnectorData> enumerationValueConnectors = new List<NodeData.ConnectorData>()
            {
                new NodeData.ConnectorData(enumerationValueOutput1, DomainIDs.ENUM_VALUE_OUTPUT_DEFINITION.Id, new List<Parameter>()),
            };
            List<NodeData.ParameterData> enumerationValuesParameters = new List<NodeData.ParameterData>()
            {
                new NodeData.ParameterData("Name", DomainIDs.ENUMERATION_VALUE_PARAMETER, BaseTypeString.PARAMETER_TYPE, NO_CONFIG),
            };
            AddNode(DomainIDs.ENUMERATION_VALUE_DECLARATION, "Enumeration Value", enumerationMenu, config('v', "808080"), enumerationValueConnectors, enumerationValuesParameters);
            #endregion

            m_nodeMenu = new NodeType("Nodes", DomainIDs.NODE_MENU);
            m_nodeHeirarchy.m_childTypes.Add(m_nodeMenu);
            NodeType parameterMenu = new NodeType("Parameters", DomainIDs.NODE_MENU);
            m_nodeMenu.m_childTypes.Add(parameterMenu);
            List<NodeData.ConnectorData> connectorDefinitionConnectors = new List<NodeData.ConnectorData>()
            {
                new NodeData.ConnectorData(ID<TConnector>.Parse("9231f7d0-9831-4a99-b59a-3479b2716f7d"), DomainIDs.CONNECTOR_DEFINITION_OUTPUT_DEFINITION.Id, new List<Parameter>()),
                //new NodeData.ConnectorData(ID<TConnector>.Parse("5b52871e-183c-409f-a4fd-7be3f7fab82a"), DomainIDs.CONNECTOR_DEFINITION_CONNECTION_DEFINITION.Id, new List<Parameter>()),
            };
            List<NodeData.ParameterData> connectorDefinitionParameters = new List<NodeData.ParameterData>()
            {
                new NodeData.ParameterData("Name", DomainIDs.CONNECTOR_DEFINITION_NAME, BaseTypeString.PARAMETER_TYPE, NO_CONFIG),
                new NodeData.ParameterData("Position", ConnectorPosition.PARAMETER_ID, ConnectorPosition.ENUM_ID, NO_CONFIG, ConnectorPosition.Bottom.Element.Guid.ToString()),
            };
            AddNode(DomainIDs.CONNECTOR_DEFINITION_GUID, "Connector", m_nodeHeirarchy, config('o', "ffff00"), connectorDefinitionConnectors, connectorDefinitionParameters);

            List<NodeData.ParameterData> connectionDefinitionParameters = new List<NodeData.ParameterData>()
            {
                new NodeData.ParameterData("Connector1", DomainIDs.CONNECTION_DEFINITION_CONNECTOR1, DomainIDs.CONNECTION_TYPE, NO_CONFIG),
                new NodeData.ParameterData("Connector2", DomainIDs.CONNECTION_DEFINITION_CONNECTOR2, DomainIDs.CONNECTION_TYPE, NO_CONFIG),
            };
            AddNode(DomainIDs.CONNECTION_DEFINITION_GUID, "Connection", m_nodeHeirarchy, config('o', "ffbb00"), NO_CONNECTORS, connectionDefinitionParameters);


            m_connectorsMenu = new NodeType("Connectors", DomainIDs.NODE_MENU);
            m_nodeMenu.m_childTypes.Add(m_connectorsMenu);

            #region Parameter Definitions
            {
                NodeData.ConnectorData parameterOutput = new NodeData.ConnectorData(parameterDefinitionConnector1, DomainIDs.PARAMETER_OUTPUT_DEFINITION.Id, new List<Parameter>());
                NodeData.ConnectorData parameterConfigConnector = new NodeData.ConnectorData(parameterConfigConnectorID, DomainIDs.PARAMETER_CONFIG_CONNECTOR_DEFINITION.Id, new List<Parameter>());

                NodeData.ParameterData nameParameter = new NodeData.ParameterData("Name", DomainIDs.PARAMETER_NAME, BaseTypeString.PARAMETER_TYPE, NO_CONFIG);
                NodeData.ParameterData integerTypeParameter = new NodeData.ParameterData("Type", DomainIDs.PARAMETER_TYPE, INTEGER_SET_GUID, NO_CONFIG, BaseTypeInteger.PARAMETER_TYPE.Guid.ToString());
                NodeData.ParameterData decimalTypeParameter = new NodeData.ParameterData("Type", DomainIDs.PARAMETER_TYPE, DECIMAL_SET_GUID, NO_CONFIG, BaseTypeDecimal.PARAMETER_TYPE.Guid.ToString());
                NodeData.ParameterData dynamicEnumTypeParameter = new NodeData.ParameterData("Type", DomainIDs.PARAMETER_TYPE, DYNAMIC_ENUM_SET_GUID, NO_CONFIG);
                NodeData.ParameterData integerDefaultParameter = new NodeData.ParameterData("Default", DomainIDs.PARAMETER_DEFAULT, BaseTypeInteger.PARAMETER_TYPE, NO_CONFIG);
                NodeData.ParameterData decimalDefaultParameter = new NodeData.ParameterData("Default", DomainIDs.PARAMETER_DEFAULT, BaseTypeDecimal.PARAMETER_TYPE, NO_CONFIG);
                NodeData.ParameterData stringDefaultParameter = new NodeData.ParameterData("Default", DomainIDs.PARAMETER_DEFAULT, BaseTypeString.PARAMETER_TYPE, NO_CONFIG);
                NodeData.ParameterData booleanDefaultParameter = new NodeData.ParameterData("Default", DomainIDs.PARAMETER_DEFAULT, BaseTypeBoolean.PARAMETER_TYPE, NO_CONFIG);
                NodeData.ParameterData enumTypeParameter = new NodeData.ParameterData("Type", DomainIDs.PARAMETER_TYPE, ENUM_SET_GUID, NO_CONFIG);

                List<NodeData.ConnectorData> parameterConnectors = new List<NodeData.ConnectorData> { parameterOutput, parameterConfigConnector };

                //OutputDefinition parameterOutput = (data) => DomainIDs.PARAMETER_OUTPUT_DEFINITION.MakeWithoutParameters(parameterDefinitionConnector1, data, DomainConnectionRules.Instance);
                AddNode(BaseType.Integer.ParameterNodeType, "Integer", parameterMenu, config('p', "00aaaa"), parameterConnectors, new List<NodeData.ParameterData> { nameParameter, integerTypeParameter, integerDefaultParameter });
                AddNode(BaseType.Decimal.ParameterNodeType, "Decimal", parameterMenu, config('p', "00aaaa"), parameterConnectors, new List<NodeData.ParameterData> { nameParameter, decimalTypeParameter, decimalDefaultParameter });
                AddNode(BaseType.String.ParameterNodeType, "String", parameterMenu, config('p', "00aaaa"), parameterConnectors, new List<NodeData.ParameterData> { nameParameter, stringDefaultParameter });
                AddNode(BaseType.LocalizedString.ParameterNodeType, "Localized String", parameterMenu, config('p', "00aaaa"), parameterConnectors, new List<NodeData.ParameterData> { nameParameter });
                AddNode(BaseType.Boolean.ParameterNodeType, "Boolean", parameterMenu, config('p', "00aaaa"), parameterConnectors, new List<NodeData.ParameterData> { nameParameter, booleanDefaultParameter });
                AddNode(BaseType.Audio.ParameterNodeType, "Audio", parameterMenu, config('p', "00aaaa"), parameterConnectors, new List<NodeData.ParameterData> { nameParameter });
                AddNode(BaseType.DynamicEnumeration.ParameterNodeType, "Dynamic Enumeration", parameterMenu, config('p', "00aaaa"), parameterConnectors, new List<NodeData.ParameterData> { nameParameter, dynamicEnumTypeParameter, stringDefaultParameter });
                AddNode(BaseType.Set.ParameterNodeType, "Set", parameterMenu, config('p', "00aaaa"), parameterConnectors, new List<NodeData.ParameterData> { nameParameter, enumTypeParameter });
                AddEnumNode(parameterMenu);
                //AddNode(BaseType.Enumeration.ParameterNodeType, "Enumeration", parameterMenu, config('p', "00aaaa"), parameterOutput.Only().ToList(),
                //new List<NodeData.ParameterData> { nameParameter, enumTypeParameter,
                //new EnumDefaultParameter(m_types.GetEnumOptions, () => ParameterType.FromGuid(typeSelectionParameter.EditorSelected)});
            }
            #endregion

            #region Built In Connectors
            //ID<TConnector> connectorOutput1 = ID<TConnector>.Parse("d2dea7c3-4674-4ac7-a908-a9da30610092");
            //OutputDefinition inputOutputDefinition = (data) => DomainIDs.CONNECTOR_OUTPUT_DEFINITION.Make(connectorOutput1, data, DomainConnectionRules.Instance);
            //m_inputConnector = AddNode(ID<NodeTypeTemp>.ConvertFrom(ConnectorDefinitionData.Input.Id), "Input", m_connectorsMenu, config('i', ConnectorColor), inputOutputDefinition.Only(), (id, ng, c) => new ExternalFunction(ng, id, c));
            //m_outputConnector = AddNode(ID<NodeTypeTemp>.ConvertFrom(ConnectorDefinitionData.Output.Id), "Output", m_connectorsMenu, config('o', ConnectorColor), inputOutputDefinition.Only(), (id, ng, c) => new ExternalFunction(ng, id, c, new StringParameter("Name", DomainIDs.OUTPUT_NAME, BaseTypeString.PARAMETER_TYPE)));
            RefreshConnectorsMenu();
            #endregion

            NodeType configMenu = new NodeType("Config", DomainIDs.CONFIG_MENU);
            m_nodeMenu.m_childTypes.Add(configMenu);

            ID<TConnector> configOutput1 = ID<TConnector>.Parse("2d88af18-d66d-4f86-868e-5db9e020c99d");
            var configConnector = new NodeData.ConnectorData(configOutput1, DomainIDs.CONFIG_OUTPUT_DEFINITION.Id, new List<Parameter>());
            //OutputDefinition configConnector = (data) => DomainIDs.CONFIG_OUTPUT_DEFINITION.MakeWithoutParameters(configOutput1, data, DomainConnectionRules.Instance);
            //AddNode(DomainIDs.CONFIG_GUID, "Generic Config", configMenu, config('c', "aabb00"), configConnector.Only(), (id, ng, c) => new ExternalFunction(ng, id, c, new StringParameter("Key", DomainIDs.CONFIG_KEY, BaseTypeString.PARAMETER_TYPE), new StringParameter("Value", DomainIDs.CONFIG_VALUE, BaseTypeString.PARAMETER_TYPE)));

            IEnumerable<IConfigNodeDefinition> configNodeDefitions = m_pluginsConfig.GetConfigDefinitions();
            foreach (var configNodeDefinition in configNodeDefitions)
            {
                var cnd = configNodeDefinition;
                AddNode(cnd.Id, cnd.Name, configMenu, config('c', "aabb00"), new List<NodeData.ConnectorData> { configConnector }, new List<NodeData.ParameterData>(), () => cnd.MakeParameters().ToList());
            }

            var category = new NodeData.ParameterData("Category", DomainIDs.NODE_CATEGORY, DomainIDs.CATEGORY_TYPE, NO_CONFIG, DomainIDs.CATEGORY_NONE.ToString());
            var nodeName = new NodeData.ParameterData("Name", DomainIDs.NODE_NAME, BaseTypeString.PARAMETER_TYPE, NO_CONFIG);
            AddNode(DomainIDs.NODE_GUID, "Node", m_nodeMenu, config('n', "808080"), NodeConnectors, new List<NodeData.ParameterData> { nodeName, category });

            ConnectorDefinitions.Modified += () => RefreshConnectorsMenu();
        }

        public static List<NodeData.ConnectorData> NodeConnectors
        {
            get
            {
                return new List<NodeData.ConnectorData>
                {
                    new NodeData.ConnectorData(ID<TConnector>.Parse("cdfa9a9e-e6e9-4b9a-b4ff-683ce6e4ad9d"),DomainIDs.NODE_OUTPUT_CONFIG_DEFINITION.Id, new List<Parameter>()),
                    new NodeData.ConnectorData(ID<TConnector>.Parse("28067d32-aa0e-4063-8985-fea92e64f2f5"),DomainIDs.NODE_OUTPUT_PARAMETERS_DEFINITION.Id, new List<Parameter>()),
                    new NodeData.ConnectorData(ID<TConnector>.Parse("34417028-db60-4483-8c31-e817f0d2548c"),DomainIDs.NODE_OUTPUT_CONNECTORS_DEFINITION.Id, new List<Parameter>()),
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
                : base("Default", DomainIDs.PARAMETER_DEFAULT, TypeId, null)
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

            public Or<Guid, string> BetterValue
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

            public override string DisplayValue(Func<ID<LocalizedText>, string> localize)
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

            protected override void DecorruptFromNull()
            {
                //TODO: Do we need to do something here?
            }
        }

        private EditableGenerator AddEnumNode(NodeType parent)
        {
            ID<NodeTypeTemp> guid = BaseType.Enumeration.ParameterNodeType;
            string name = "Enumeration";
            NodeData.ConnectorData parameterOutput = new NodeData.ConnectorData(parameterDefinitionConnector1, DomainIDs.PARAMETER_OUTPUT_DEFINITION.Id, new List<Parameter>());
            NodeData.ConnectorData parameterConfigConnector = new NodeData.ConnectorData(parameterConfigConnectorID, DomainIDs.PARAMETER_CONFIG_CONNECTOR_DEFINITION.Id, new List<Parameter>());
            List<NodeData.ConnectorData> parameterConnectors = new List<NodeData.ConnectorData> { parameterOutput, parameterConfigConnector };
            NodeData.ParameterData nameParameter = new NodeData.ParameterData("Name", DomainIDs.PARAMETER_NAME, BaseTypeString.PARAMETER_TYPE, NO_CONFIG);
            NodeData.ParameterData enumTypeParameter = new NodeData.ParameterData("Type", DomainIDs.PARAMETER_TYPE, ENUM_SET_GUID, NO_CONFIG);

            NodeData data;
            data.Config = this.config('p', "00aaaa");
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

        private EditableGenerator AddNode(ID<NodeTypeTemp> guid, string name, NodeType parent, List<NodeData.ConfigData> config, List<NodeData.ConnectorData> connectors, List<NodeData.ParameterData> parameters, Func<List<Parameter>> extraParameters)
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

        private EditableGenerator AddNode(ID<NodeTypeTemp> guid, string name, NodeType parent, List<NodeData.ConfigData> config, List<NodeData.ConnectorData> connectors, List<NodeData.ParameterData> parameters)
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

        public EditableGenerator GetNode(ID<NodeTypeTemp> guid)
        {
            if (m_nodes.ContainsKey(guid))
                return m_nodes[guid];
            else
                return null;
        }

        public void RenameCategory(string name, Guid guid)
        {
            EnumerationData data = m_typeSet.GetEnumData(DomainIDs.CATEGORY_TYPE);
            var index = data.Elements.IndexOf(e => e.Guid == guid);
            var option = data.Elements[index];
            option.Name = name;
            data.Elements[index] = option;
            UpdateEnumeration(data);
            //m_categories.SetName(guid, name);
        }

        public void AddCategory(string name, Guid guid)
        {
            EnumerationData data = m_typeSet.GetEnumData(DomainIDs.CATEGORY_TYPE);
            data.Elements.Add(new EnumerationData.Element(name, guid));
            UpdateEnumeration(data);
            //m_categories.Add(guid, name);
        }

        public void RemoveCategory(Guid guid)
        {
            EnumerationData data = m_typeSet.GetEnumData(DomainIDs.CATEGORY_TYPE);
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
            m_typeSet.AddEnum(data);
            //m_types.Enumerations.Add(data.Guid, data.Name, data);
        }

        public void AddDynamicEnumType(DynamicEnumerationData data)
        {
            m_typeSet.AddDynamicEnum(data);
            //m_types.DynamicEnumerations.Add(data.TypeID, data.Name, data);
        }

        public void RemoveType(BaseType baseType, ParameterType guid)
        {
            m_typeSet.Remove(guid);
            //m_types.RemoveType(baseType, guid);
        }

        public static void ForEachNode(IEnumerable<ConversationNode> nodes, Action<NodeTypeData> categoryAction, Action<IntegerData> integerAction, Action<DecimalData> decimalAction, Action<DynamicEnumerationData> dynamicEnumAction, Action<EnumerationData> enumerationAction, Action<EnumerationData> enumerationValueAction, Action<NodeData> nodeAction, Action<ConnectorDefinitionData> connectorAction, Action<ConnectionDefinitionData> connectionAction)
        {
            ForEachNode(nodes.Select(n => n.m_data), categoryAction, integerAction, decimalAction, dynamicEnumAction, enumerationAction, enumerationValueAction, nodeAction, connectorAction, connectionAction);
        }
        public static void ForEachNode(IEnumerable<IEditable> nodes, Action<NodeTypeData> categoryAction, Action<IntegerData> integerAction, Action<DecimalData> decimalAction, Action<DynamicEnumerationData> dynamicEnumAction, Action<EnumerationData> enumerationAction, Action<EnumerationData> enumerationValueAction, Action<NodeData> nodeAction, Action<ConnectorDefinitionData> connectorAction, Action<ConnectionDefinitionData> connectionAction)
        {
            foreach (var node in nodes.OrderBy(n => n.NodeTypeID == DomainIDs.NODE_GUID ? 2 : 1))
            {
                if (node.NodeTypeID == DomainIDs.CATEGORY_GUID)
                {
                    var nameParameter = node.Parameters.Single(p => p.Id == DomainIDs.CATEGORY_NAME) as IStringParameter;
                    var name = nameParameter.Value;
                    var parentParameter = node.Parameters.Single(p => p.Id == DomainIDs.CATEGORY_PARENT) as IEnumParameter;
                    var parent = parentParameter.Value;
                    categoryAction(new NodeTypeData(name, node.NodeID.Guid, parent));
                }
                else if (node.NodeTypeID == BaseType.Integer.NodeType)
                {
                    var nameParameter = node.Parameters.Single(p => p.Id == DomainIDs.INTEGER_NAME) as IStringParameter;
                    var name = nameParameter.Value;
                    var minParameter = node.Parameters.Single(p => p.Id == DomainIDs.INTEGER_MIN) as IIntegerParameter;
                    var min = minParameter.Value;
                    var maxParameter = node.Parameters.Single(p => p.Id == DomainIDs.INTEGER_MAX) as IIntegerParameter;
                    var max = maxParameter.Value;
                    //var defParameter = node.Parameters.Single(p => p.Guid == DomainGUIDS.INTEGER_DEFAULT) as IIntegerParameter;
                    //var def = defParameter.Value;
                    integerAction(new IntegerData(name, ParameterType.Basic.FromGuid(node.NodeID.Guid), max, min/*, def*/));
                }
                else if (node.NodeTypeID == BaseType.Decimal.NodeType)
                {
                    var nameParameter = node.Parameters.Single(p => p.Id == DomainIDs.DECIMAL_NAME) as IStringParameter;
                    var name = nameParameter.Value;
                    var minParameter = node.Parameters.Single(p => p.Id == DomainIDs.DECIMAL_MIN) as IDecimalParameter;
                    var min = minParameter.Value;
                    var maxParameter = node.Parameters.Single(p => p.Id == DomainIDs.DECIMAL_MAX) as IDecimalParameter;
                    var max = maxParameter.Value;
                    //var defParameter = node.Parameters.Single(p => p.Guid == DomainGUIDS.DECIMAL_DEFAULT) as IDecimalParameter;
                    //var def = defParameter.Value;
                    decimalAction(new DecimalData(name, ParameterType.Basic.FromGuid(node.NodeID.Guid), max, min/*, def*/));
                }
                else if (node.NodeTypeID == BaseType.DynamicEnumeration.NodeType)
                {
                    var nameParameter = node.Parameters.Single(p => p.Id == DomainIDs.DYNAMIC_ENUM_NAME) as IStringParameter;
                    var name = nameParameter.Value;
                    dynamicEnumAction(new DynamicEnumerationData(name, ParameterType.Basic.FromGuid(node.NodeID.Guid)));
                }
                else if (node.NodeTypeID == BaseType.Enumeration.NodeType)
                {
                    ForEnumDeclaration(enumerationAction, node);
                }
                else if (node.NodeTypeID == DomainIDs.ENUMERATION_VALUE_DECLARATION)
                {
                    foreach (var enumTypeNode in node.Connectors.SelectMany(t => t.Connections).Select(t => t.Parent).Where(n => n.NodeTypeID == BaseType.Enumeration.NodeType))
                    {
                        ForEnumDeclaration(enumerationValueAction, enumTypeNode);
                    }
                }
                else if (node.NodeTypeID == DomainIDs.NODE_GUID)
                {
                    var nameParameter = node.Parameters.Single(p => p.Id == DomainIDs.NODE_NAME) as IStringParameter;
                    var name = nameParameter.Value;
                    var categoryParameter = node.Parameters.Single(p => p.Id == DomainIDs.NODE_CATEGORY) as IEnumParameter;
                    var category = categoryParameter.Value;

                    List<NodeData.ConnectorData> connectors = new List<NodeData.ConnectorData>();
                    foreach (var connectorNode in node.Connectors.Single(c => c.m_definition.Id == DomainIDs.NODE_OUTPUT_CONNECTORS_DEFINITION.Id).Connections)
                    {
                        connectors.Add(new NodeData.ConnectorData(ID<TConnector>.ConvertFrom(connectorNode.Parent.NodeID), ID<TConnectorDefinition>.ConvertFrom(connectorNode.Parent.NodeTypeID), connectorNode.Parent.Parameters.ToList()));
                    }

                    List<NodeData.ParameterData> parameters = new List<NodeData.ParameterData>();
                    IEnumerable<IEditable> parameterNodes = node.Connectors.Single(c => c.m_definition.Id == DomainIDs.NODE_OUTPUT_PARAMETERS_DEFINITION.Id).Connections.Select(l => l.Parent);
                    foreach (var parameterNode in parameterNodes) //.Where(n => BaseType.TypeExists(n.NodeTypeID)))
                    {
                        parameters.Add(BaseType.GetType(parameterNode.NodeTypeID).ReadDomainNode(parameterNode));
                    }

                    List<NodeData.ConfigData> config = new List<NodeData.ConfigData>();
                    var configs = node.Connectors.Single(c => c.m_definition.Id == DomainIDs.NODE_OUTPUT_CONFIG_DEFINITION.Id).Connections.Select(l => l.Parent);
                    foreach (var configNode in configs)
                    {
                        config.Add(new NodeData.ConfigData(configNode.NodeTypeID, configNode.Parameters));
                    }

                    nodeAction(new NodeData(name, category, ID<NodeTypeTemp>.FromGuid(node.NodeID.Guid), connectors, parameters, config));
                }
                else if (node.NodeTypeID == DomainIDs.CONNECTOR_DEFINITION_GUID)
                {
                    var nameParameter = node.Parameters.Single(p => p.Id == DomainIDs.CONNECTOR_DEFINITION_NAME) as IStringParameter;
                    var positionParameter = node.Parameters.Single(p => p.Id == ConnectorPosition.PARAMETER_ID) as IEnumParameter;
                    var name = nameParameter.Value;

                    var linkedNodes = node.Connectors.SelectMany(n => n.Connections.Select(l => l.Parent));
                    List<NodeData.ParameterData> parameters = new List<NodeData.ParameterData>();
                    foreach (var parameterNode in linkedNodes.Where(n => BaseType.TypeExists(n.NodeTypeID)))
                    {
                        parameters.Add(BaseType.GetType(parameterNode.NodeTypeID).ReadDomainNode(parameterNode));
                    }

                    connectorAction(new ConnectorDefinitionData(name, ID<TConnectorDefinition>.ConvertFrom(node.NodeID), parameters, ConnectorPosition.Read(positionParameter)));
                }
                else if (node.NodeTypeID == DomainIDs.CONNECTION_DEFINITION_GUID)
                {
                    var connector1Parameter = node.Parameters.Single(p => p.Id == DomainIDs.CONNECTION_DEFINITION_CONNECTOR1) as IEnumParameter;
                    var connector2Parameter = node.Parameters.Single(p => p.Id == DomainIDs.CONNECTION_DEFINITION_CONNECTOR2) as IEnumParameter;
                    connectionAction(new ConnectionDefinitionData(UnorderedTuple.Make(ID<TConnectorDefinition>.FromGuid(connector1Parameter.Value), ID<TConnectorDefinition>.FromGuid(connector2Parameter.Value))));
                }
            }
        }

        private static void ForEnumDeclaration(Action<EnumerationData> enumerationAction, IEditable node)
        {
            var nameParameter = node.Parameters.Single(p => p.Id == DomainIDs.ENUMERATION_NAME) as IStringParameter;
            var name = nameParameter.Value;
            //var defParameter = node.Parameters.Single(p => p.Guid == DomainGUIDS.PARAMETER_DEFAULT) as IDynamicEnumParameter;
            //var def = defParameter.Value;
            //var links = node.TransitionsOut.Single().Connections;

            IEnumerable<Output> links = node.Connectors.SelectMany(n => n.Connections);
            List<EnumerationData.Element> elements = new List<EnumerationData.Element>();
            foreach (Output link in links.Where(l => l.Parent.NodeTypeID == DomainIDs.ENUMERATION_VALUE_DECLARATION))
            {
                var valueParameter = link.Parent.Parameters.Single(p => p.Id == DomainIDs.ENUMERATION_VALUE_PARAMETER) as IStringParameter;
                elements.Add(new EnumerationData.Element(valueParameter.Value, link.Parent.NodeID.Guid));
            }
            enumerationAction(new EnumerationData(name, ParameterType.Basic.ConvertFrom(node.NodeID), elements/*, def*/));
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

            EnumerationData data = m_typeSet.GetEnumData(DomainIDs.CONNECTION_TYPE);
            var index = data.Elements.IndexOf(e => e.Guid == cdd.Id.Guid);
            var option = data.Elements[index];
            option.Name = cdd.Name;
            data.Elements[index] = option;
            UpdateEnumeration(data);
        }

        internal void AddConnector(ConnectorDefinitionData cdd)
        {
            ConnectorDefinitions.Add(cdd.Id, cdd);

            EnumerationData data = m_typeSet.GetEnumData(DomainIDs.CONNECTION_TYPE);
            data.Elements.Add(new EnumerationData.Element(cdd.Name, cdd.Id.Guid));
            UpdateEnumeration(data);
        }

        internal void RemoveConnector(ConnectorDefinitionData cdd)
        {
            ConnectorDefinitions.Remove(cdd.Id);

            EnumerationData data = m_typeSet.GetEnumData(DomainIDs.CONNECTION_TYPE);
            data.Elements.RemoveAll(e => e.Guid == cdd.Id.Guid);
            UpdateEnumeration(data);
        }

        private void RefreshConnectorsMenu()
        {
            //m_connectorsMenu.m_nodes.RemoveAll(g => !object.Equals(g, m_inputConnector) && !object.Equals(g, m_outputConnector));
            m_connectorsMenu.m_nodes.Clear();
            foreach (var data in ConnectorDefinitions.Values)
            {
                if (!data.Hidden)
                {
                    var d = data;
                    NodeData.ConnectorData connector = new NodeData.ConnectorData(ID<TConnector>.Parse("d7ac3a74-206d-48d5-b40f-30bc16dfdb67"), DomainIDs.CONNECTOR_OUTPUT_DEFINITION.Id, new List<Parameter>());
                    //OutputDefinition ConnectorConnectorDefinition = (e) => DomainIDs.CONNECTOR_OUTPUT_DEFINITION.MakeWithoutParameters( e, DomainConnectionRules.Instance);
                    AddNode(ID<NodeTypeTemp>.ConvertFrom(d.Id), d.Name, m_connectorsMenu, config('\0', ConnectorColor), new List<NodeData.ConnectorData> { connector }, d.Parameters.ToList());
                    //(id, ng, c) => new ExternalFunction(ng, id, c, d.Parameters.Select(p => p.Make(m_typeSet.Make)).ToArray()));
                }
            }
        }

        private List<NodeData.ConfigData> config(char shortcut, string color)
        {
            List<NodeData.ConfigData> result = new List<NodeData.ConfigData>();
            if (shortcut != '\0')
            {
                result.Add(GenericNodeConfigDefinition.Make("Shortcut", shortcut.ToString()));
            }
            result.Add(GenericNodeConfigDefinition.Make("Color", color));
            return result;
        }

        public bool IsCategoryDefinition(ID<NodeTypeTemp> id)
        {
            return id == DomainIDs.CATEGORY_GUID;
        }

        public bool IsTypeDefinition(ID<NodeTypeTemp> id)
        {
            return TypeDefinitionNodeIDs.All.Contains(id);
        }

        public bool IsConnectorDefinition(ID<NodeTypeTemp> id)
        {
            if (id == DomainIDs.CONNECTOR_DEFINITION_GUID)
                return true;
            else if (BaseType.BaseTypes.Any(t => id == t.ParameterNodeType))
                return true;
            else if (id == DomainIDs.CONFIG_GUID)
                return true;
            else
                return false;
        }

        public bool IsParameter(ID<NodeTypeTemp> id)
        {
            return BaseType.BaseTypes.Any(t => id == t.ParameterNodeType);
        }

        public bool IsConnector(ID<NodeTypeTemp> id)
        {
            return ConnectorDefinitions.Values.Any(d => id == ID<NodeTypeTemp>.ConvertFrom(d.Id));
        }

        public bool IsNodeDefinition(ID<NodeTypeTemp> id)
        {
            if (id == DomainIDs.NODE_GUID)
                return true;
            else if (IsParameter(id))
                return true;
            //else if (id == m_inputConnector.Guid)
            //    return true;
            //else if (id == m_outputConnector.Guid)
            //    return true;
            else if (IsConnector(id))
                return true;
            else if (id == DomainIDs.CONFIG_GUID)
                return true;
            else
                return false;
        }

        public static bool IsConnector(IDataSource datasource, ID<NodeTypeTemp> id)
        {
            if (!datasource.IsNodeDefinition(id))
                return false;
            if (id == DomainIDs.NODE_GUID)
                return false;
            if (BaseType.BaseTypes.Any(t => id == t.ParameterNodeType))
                return false;
            if (id == DomainIDs.CONFIG_GUID)
                return false;
            return true;
        }

        internal bool IsConfig(ID<NodeTypeTemp> iD)
        {
            IEnumerable<IConfigNodeDefinition> configNodeDefitions = m_pluginsConfig.GetConfigDefinitions();
            foreach (var configNodeDefinition in configNodeDefitions)
            {
                if (configNodeDefinition.Id == iD)
                    return true;
            }
            return false;
        }

        public CallbackDictionary<ID<TConnectorDefinition>, ConnectorDefinitionData> ConnectorDefinitions = new CallbackDictionary<ID<TConnectorDefinition>, ConnectorDefinitionData>()
        {
            { SpecialConnectors.Input.Id, SpecialConnectors.Input },
            { SpecialConnectors.Output.Id, SpecialConnectors.Output },
            { DomainIDs.PARAMETER_OUTPUT_DEFINITION                                .Id,       DomainIDs.PARAMETER_OUTPUT_DEFINITION                     },
            { DomainIDs.PARAMETER_CONFIG_CONNECTOR_DEFINITION                      .Id,       DomainIDs.PARAMETER_CONFIG_CONNECTOR_DEFINITION           },
            { DomainIDs.CONNECTOR_OUTPUT_DEFINITION                                .Id,       DomainIDs.CONNECTOR_OUTPUT_DEFINITION                     },
            { DomainIDs.CONFIG_OUTPUT_DEFINITION                                   .Id,       DomainIDs.CONFIG_OUTPUT_DEFINITION                        },
            { DomainIDs.NODE_OUTPUT_CONFIG_DEFINITION                              .Id,       DomainIDs.NODE_OUTPUT_CONFIG_DEFINITION                   },
            { DomainIDs.NODE_OUTPUT_PARAMETERS_DEFINITION                          .Id,       DomainIDs.NODE_OUTPUT_PARAMETERS_DEFINITION               },
            { DomainIDs.NODE_OUTPUT_CONNECTORS_DEFINITION                          .Id,       DomainIDs.NODE_OUTPUT_CONNECTORS_DEFINITION               },
            { DomainIDs.CONNECTOR_DEFINITION_OUTPUT_DEFINITION                     .Id,       DomainIDs.CONNECTOR_DEFINITION_OUTPUT_DEFINITION          },
            { DomainIDs.ENUM_VALUE_OUTPUT_DEFINITION                               .Id,       DomainIDs.ENUM_VALUE_OUTPUT_DEFINITION                    },
            { DomainIDs.ENUM_OUTPUT_DEFINITION                                     .Id,       DomainIDs.ENUM_OUTPUT_DEFINITION                          },
            //{ DomainIDs.CONNECTOR_DEFINITION_CONNECTION_DEFINITION                 .Id,       DomainIDs.CONNECTOR_DEFINITION_CONNECTION_DEFINITION      },
            //{ DomainIDs.CONNECTION_DEFINITION_CONNECTOR                            .Id,       DomainIDs.CONNECTION_DEFINITION_CONNECTOR                 },
        };
        private PluginsConfig m_pluginsConfig;


        public string GetTypeName(ParameterType type)
        {
            throw new NotImplementedException(); // Don't need an implementation for this because we won't be modifying editors for types in the domain domain
        }


        public Guid GetCategory(ID<NodeTypeTemp> type)
        {
            throw new NotImplementedException();
        }
    }
}
