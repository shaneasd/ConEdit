using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;

using ConversationNode = Conversation.ConversationNode<Conversation.INodeGUI>;

namespace ConversationEditor
{
    public delegate Output OutputDefinition(IEditable parent);

    public class DomainDomain : IDataSource
    {
        public class Types
        {
            public static Guid INTEGER_SET_GUID = Guid.Parse("07ca7287-20c0-4ba5-ae28-e17ea97554d6");
            public static Guid DECIMAL_SET_GUID = Guid.Parse("0c1e5fa8-97ff-450b-a01c-5d09ea6dbd78");
            public static Guid ENUM_SET_GUID = Guid.Parse("e7526632-95ca-4981-8b45-56cea272ddd0");
            public static Guid DYNAMIC_ENUM_SET_GUID = Guid.Parse("b3278dc3-6c2b-471a-a1c9-de39691af302");

            //TODO: Make private
            public TypeSet m_typeSet = BaseTypeSet.Make(); //Tracks types which are usable by the domain. This includes types defined by the user as well as all base types.
            private CustomizableTypeSet<IntegerData> m_integer;
            private CustomizableTypeSet<DecimalData> m_decimal;
            private CustomizableTypeSet<EnumerationData> m_enumeration;
            private CustomizableTypeSet<DynamicEnumerationData> m_dynamicEnumeration;

            /// <summary>
            /// Mapping of enum type guid to all the possible values that are currently linked to that type definition node
            /// </summary>
            private readonly Dictionary<ID<ParameterType>, IEnumerable<EnumerationData.Element>> m_enumOptions = new Dictionary<ID<ParameterType>, IEnumerable<EnumerationData.Element>>();
            public Dictionary<ID<ParameterType>, IEnumerable<EnumerationData.Element>> GetEnumOptions() { return m_enumOptions; }

            private readonly Dictionary<ID<ParameterType>, DynamicEnumParameter.Source> m_dynamicEnumParameterSources = new Dictionary<ID<ParameterType>, DynamicEnumParameter.Source>();
            private DynamicEnumParameter.Source GetSource(ID<ParameterType> typeid)
            {
                if (!m_dynamicEnumParameterSources.ContainsKey(typeid))
                    m_dynamicEnumParameterSources[typeid] = new DynamicEnumParameter.Source();
                return m_dynamicEnumParameterSources[typeid];
            }

            public Parameter MakeParameter(ID<ParameterType> typeid, string name, ID<Parameter> id)
            {
                return m_typeSet.Make(typeid, name, id);
            }

            public ICustomizableTypeSet<IntegerData> Integers { get { return m_integer; } }
            public ICustomizableTypeSet<DecimalData> Decimals { get { return m_decimal; } }
            public ICustomizableTypeSet<EnumerationData> Enumerations { get { return m_enumeration; } }
            public ICustomizableTypeSet<DynamicEnumerationData> DynamicEnumerations { get { return m_dynamicEnumeration; } }

            public Types()
            {
                m_integer = new CustomizableTypeSet<IntegerData>(a => m_typeSet.AddInteger(a),
                                                                 m_typeSet.IsInteger,
                                                                 (n, id, guid) => new IntegerParameter(n, id, guid),
                                                                 new MutableEnumeration(new[] { new Tuple<Guid, string>(BaseTypeInteger.PARAMETER_TYPE.Guid, "Integer") },
                                                                 DomainIDs.TYPES_GUID, ""));
                m_decimal = new CustomizableTypeSet<DecimalData>(a => m_typeSet.AddDecimal(a), m_typeSet.IsDecimal, (n, id, guid) => new DecimalParameter(n, id, guid), new MutableEnumeration(new[] { new Tuple<Guid, string>(BaseTypeDecimal.PARAMETER_TYPE.Guid, "Decimal") }, DomainIDs.TYPES_GUID, ""));
                m_enumeration = new CustomizableTypeSet<EnumerationData>(a => m_typeSet.AddEnum(a), m_typeSet.IsEnum,
                    (n, id, guid) => new EnumParameter(n, id, new WrapperEnumeration(() => m_enumOptions[guid].Select(e => e.Guid), g => m_enumOptions[guid].Single(e => e.Guid == g).Name, () => "", guid)),
                    new MutableEnumeration(new Tuple<Guid, string>[0], DomainIDs.TYPES_GUID, ""));
                m_dynamicEnumeration = new CustomizableTypeSet<DynamicEnumerationData>(a => m_typeSet.AddDynamicEnum(a), m_typeSet.IsDynamicEnum, (n, id, guid) => new DynamicEnumParameter(n, id, GetSource(guid), guid), new MutableEnumeration(new Tuple<Guid, string>[0], DomainIDs.TYPES_GUID, ""));
            }

            public interface ICustomizableTypeSet
            {
                IEnumeration Enum { get; }
                void SetName(Guid guid, string name);
                void Remove(Guid guid);
                bool Is(ID<ParameterType> type);
            }

            public interface ICustomizableTypeSet<TData> : ICustomizableTypeSet
            {
                void Add(ID<ParameterType> guid, string name, TData data);
            }

            public class CustomizableTypeSet<TData> : ICustomizableTypeSet<TData>
            {
                public readonly MutableEnumeration m_enumeration;
                private Func<string, ID<Parameter>, ID<ParameterType>, Parameter> m_factory;
                //private Action<ID<ParameterType>, Func<string, ID<Parameter>, Parameter>> m_typeSetAdd;
                private Action<TData> m_typeSetAdd;
                private Func<ID<ParameterType>, bool> m_is;

                public CustomizableTypeSet(Action<TData> typeSetAdd, Func<ID<ParameterType>, bool> @is, Func<string, ID<Parameter>, ID<ParameterType>, Parameter> factory, MutableEnumeration enumeration)
                //public CustomizableTypeSet(Action<ID<ParameterType>, Func<string, ID<Parameter>, Parameter>> typeSetAdd, Func<ID<ParameterType>, bool> @is, Func<string, ID<Parameter>, ID<ParameterType>, Parameter> factory, MutableEnumeration enumeration)
                {
                    m_enumeration = enumeration;
                    m_typeSetAdd = typeSetAdd;
                    m_is = @is;
                    m_factory = factory;
                }

                public IEnumeration Enum { get { return m_enumeration; } }

                public void SetName(Guid guid, string name)
                {
                    m_enumeration.SetName(guid, name);
                }

                public void Add(ID<ParameterType> guid, string name, TData data)
                {
                    m_typeSetAdd(data);
                    //m_typeSetAdd(guid, (n, id) => m_factory(n, id, guid));
                    m_enumeration.Add(guid.Guid, name);
                }

                public void Remove(Guid guid)
                {
                    m_enumeration.Remove(guid);
                }

                public bool Is(ID<ParameterType> type)
                {
                    return m_is(type);
                }
            }

            Dictionary<ID<NodeTypeTemp>, ICustomizableTypeSet> mapping
            {
                get
                {
                    return new Dictionary<ID<NodeTypeTemp>, ICustomizableTypeSet>()
                    {
                        { BaseType.Integer.NodeType, Integers },
                        { BaseType.Decimal.NodeType, Decimals },
                        { BaseType.Enumeration.NodeType, Enumerations },
                        { BaseType.DynamicEnumeration.NodeType, DynamicEnumerations },
                    };
                }
            }

            internal void RenameType(BaseType baseType, string name, ID<ParameterType> guid)
            {
                mapping[baseType.NodeType].SetName(guid.Guid, name);
            }

            internal void RemoveType(BaseType baseType, ID<ParameterType> guid)
            {
                mapping[baseType.NodeType].Remove(guid.Guid);
                m_typeSet.Remove(guid);
            }

            internal void UpdateEnumOptions(ID<ParameterType> id, List<EnumerationData.Element> elements)
            {
                m_enumOptions[id] = elements;
            }
        }

        //Types m_types;

        TypeSet m_typeSet = BaseTypeSet.Make();

        //private MutableEnumeration m_categories;

        public static ID<ParameterType> INTEGER_SET_GUID = ID<ParameterType>.Parse("07ca7287-20c0-4ba5-ae28-e17ea97554d6");
        public static ID<ParameterType> DECIMAL_SET_GUID = ID<ParameterType>.Parse("0c1e5fa8-97ff-450b-a01c-5d09ea6dbd78");
        public static ID<ParameterType> ENUM_SET_GUID = ID<ParameterType>.Parse("e7526632-95ca-4981-8b45-56cea272ddd0");
        public static ID<ParameterType> DYNAMIC_ENUM_SET_GUID = ID<ParameterType>.Parse("b3278dc3-6c2b-471a-a1c9-de39691af302");


        NodeType m_nodeHeirarchy;
        Dictionary<ID<NodeTypeTemp>, EditableGenerator> m_nodes = new Dictionary<ID<NodeTypeTemp>, EditableGenerator>();

        NodeType m_nodeMenu;
        NodeType m_connectorsMenu;

        static readonly List<NodeData.ConnectorData> NO_CONNECTORS = new List<NodeData.ConnectorData>();
        IEnumerable<OutputDefinition> NO_OUTPUT_DEFINITIONS = Enumerable.Empty<OutputDefinition>();

        public DomainDomain(PluginsConfig pluginsConfig)
        {
            //m_types = new Types();
            m_pluginsConfig = pluginsConfig;

            var categoryNone = Tuple.Create(DomainIDs.CATEGORY_NONE, "None");
            EnumerationData categoryData = new EnumerationData("Categories", DomainIDs.CATEGORY_TYPE, new[] { new EnumerationData.Element("None", DomainIDs.CATEGORY_NONE) });
            m_typeSet.AddEnum(categoryData, true);
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
                if (!(new[] { allEnums, allDynamicEnums, allIntegers, allDecimals }).Any(e => e.Guid == id))
                {
                    allEnums.Elements = m_typeSet.VisibleEnums.Select(e => new EnumerationData.Element(e.Name, e.Guid.Guid)).ToList();
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
                    new NodeData.ParameterData("Name", DomainIDs.CATEGORY_NAME, BaseTypeString.PARAMETER_TYPE),
                    new NodeData.ParameterData("Parent", DomainIDs.CATEGORY_PARENT, DomainIDs.CATEGORY_TYPE),
                };

                AddNode(DomainIDs.CATEGORY_GUID, "Category", m_nodeHeirarchy, config('x', "808080"), NO_CONNECTORS, parameters);
            }
            #endregion

            #region Custom Type Definition
            List<NodeData.ParameterData> integerParameters = new List<NodeData.ParameterData>()
            {
                new NodeData.ParameterData("Name", DomainIDs.INTEGER_NAME, BaseTypeString.PARAMETER_TYPE),
                new NodeData.ParameterData("Max", DomainIDs.INTEGER_MAX, BaseTypeInteger.PARAMETER_TYPE),
                new NodeData.ParameterData("Min", DomainIDs.INTEGER_MIN, BaseTypeInteger.PARAMETER_TYPE),
            };
            AddNode(BaseType.Integer.NodeType, "Integer", m_nodeHeirarchy, config('t', "808080"), NO_CONNECTORS, integerParameters);

            List<NodeData.ParameterData> decimalParameters = new List<NodeData.ParameterData>()
            {
                new NodeData.ParameterData("Name", DomainIDs.DECIMAL_NAME, BaseTypeString.PARAMETER_TYPE),
                new NodeData.ParameterData("Max", DomainIDs.DECIMAL_MAX, BaseTypeDecimal.PARAMETER_TYPE),
                new NodeData.ParameterData("Min", DomainIDs.DECIMAL_MIN, BaseTypeDecimal.PARAMETER_TYPE),
            };
            AddNode(BaseType.Decimal.NodeType, "Decimal", m_nodeHeirarchy, config('d', "808080"), NO_CONNECTORS, decimalParameters);

            List<NodeData.ParameterData> dynamicEnumParameters = new List<NodeData.ParameterData>()
            {
                new NodeData.ParameterData("Name", DomainIDs.DYNAMIC_ENUM_NAME, BaseTypeString.PARAMETER_TYPE),
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
                new NodeData.ParameterData("Name", DomainIDs.ENUMERATION_NAME, BaseTypeString.PARAMETER_TYPE),
            };
            AddNode(BaseType.Enumeration.NodeType, "Enumeration", enumerationMenu, config('e', "808080"), enumerationConnectors, enumerationParameters);

            ID<TConnector> enumerationValueOutput1 = ID<TConnector>.Parse("ef845d1a-11a2-45d1-bab2-de8104b46c51");
            List<NodeData.ConnectorData> enumerationValueConnectors = new List<NodeData.ConnectorData>()
            {
                new NodeData.ConnectorData(enumerationValueOutput1, DomainIDs.ENUM_VALUE_OUTPUT_DEFINITION.Id, new List<Parameter>()),
            };
            List<NodeData.ParameterData> enumerationValuesParameters = new List<NodeData.ParameterData>()
            {
                new NodeData.ParameterData("Name", DomainIDs.ENUMERATION_VALUE_PARAMETER, BaseTypeString.PARAMETER_TYPE),
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
            };
            List<NodeData.ParameterData> connectorDefinitionParameters = new List<NodeData.ParameterData>()
            {
                new NodeData.ParameterData("Name", DomainIDs.CONNECTOR_DEFINITION_NAME, BaseTypeString.PARAMETER_TYPE),
                new NodeData.ParameterData("Position", ConnectorPosition.PARAMETER_ID, ConnectorPosition.ENUM_ID, ConnectorPosition.Bottom.Element.Name),
            };
            AddNode(DomainIDs.CONNECTOR_DEFINITION_GUID, "Connector", m_nodeHeirarchy, config('o', "ffff00"), connectorDefinitionConnectors, connectorDefinitionParameters);
            m_connectorsMenu = new NodeType("Connectors", DomainIDs.NODE_MENU);
            m_nodeMenu.m_childTypes.Add(m_connectorsMenu);

            #region Parameter Definitions
            {
                ID<TConnector> parameterDefinitionConnector1 = ID<TConnector>.Parse("1fd8a64d-271e-42b8-bfd8-85e5174bbf9d");
                NodeData.ConnectorData parameterOutput = new NodeData.ConnectorData(parameterDefinitionConnector1, DomainIDs.PARAMETER_OUTPUT_DEFINITION.Id, new List<Parameter>());
                NodeData.ParameterData nameParameter = new NodeData.ParameterData("Name", DomainIDs.PARAMETER_NAME, BaseTypeString.PARAMETER_TYPE);
                NodeData.ParameterData integerTypeParameter = new NodeData.ParameterData("Type", DomainIDs.PARAMETER_TYPE, INTEGER_SET_GUID, BaseTypeInteger.Data.Name);
                NodeData.ParameterData decimalTypeParameter = new NodeData.ParameterData("Type", DomainIDs.PARAMETER_TYPE, DECIMAL_SET_GUID, BaseTypeDecimal.Data.Name);
                NodeData.ParameterData dynamicEnumTypeParameter = new NodeData.ParameterData("Type", DomainIDs.PARAMETER_TYPE, DYNAMIC_ENUM_SET_GUID);
                NodeData.ParameterData integerDefaultParameter = new NodeData.ParameterData("Default", DomainIDs.PARAMETER_DEFAULT, BaseTypeInteger.PARAMETER_TYPE);
                NodeData.ParameterData decimalDefaultParameter = new NodeData.ParameterData("Default", DomainIDs.PARAMETER_DEFAULT, BaseTypeDecimal.PARAMETER_TYPE);
                NodeData.ParameterData stringDefaultParameter = new NodeData.ParameterData("Default", DomainIDs.PARAMETER_DEFAULT, BaseTypeString.PARAMETER_TYPE);
                NodeData.ParameterData booleanDefaultParameter = new NodeData.ParameterData("Default", DomainIDs.PARAMETER_DEFAULT, BaseTypeBoolean.PARAMETER_TYPE);


                //OutputDefinition parameterOutput = (data) => DomainIDs.PARAMETER_OUTPUT_DEFINITION.MakeWithoutParameters(parameterDefinitionConnector1, data, DomainConnectionRules.Instance);
                AddNode(BaseType.Integer.ParameterNodeType, "Integer", parameterMenu, config('p', "00aaaa"), parameterOutput.Only().ToList(), new List<NodeData.ParameterData> { nameParameter, integerTypeParameter, integerDefaultParameter });
                AddNode(BaseType.Decimal.ParameterNodeType, "Decimal", parameterMenu, config('p', "00aaaa"), parameterOutput.Only().ToList(), new List<NodeData.ParameterData> { nameParameter, decimalTypeParameter, decimalDefaultParameter });
                AddNode(BaseType.String.ParameterNodeType, "String", parameterMenu, config('p', "00aaaa"), parameterOutput.Only().ToList(), new List<NodeData.ParameterData> { nameParameter, stringDefaultParameter });
                AddNode(BaseType.LocalizedString.ParameterNodeType, "Localized String", parameterMenu, config('p', "00aaaa"), parameterOutput.Only().ToList(), new List<NodeData.ParameterData> { nameParameter });
                AddNode(BaseType.Boolean.ParameterNodeType, "Boolean", parameterMenu, config('p', "00aaaa"), parameterOutput.Only().ToList(), new List<NodeData.ParameterData> { nameParameter, booleanDefaultParameter });
                AddNode(BaseType.Audio.ParameterNodeType, "Audio", parameterMenu, config('p', "00aaaa"), parameterOutput.Only().ToList(), new List<NodeData.ParameterData> { nameParameter });
                AddNode(BaseType.DynamicEnumeration.ParameterNodeType, "Dynamic Enumeration", parameterMenu, config('p', "00aaaa"), parameterOutput.Only().ToList(), new List<NodeData.ParameterData> { nameParameter, dynamicEnumTypeParameter, stringDefaultParameter });
                AddEnumNode(parameterMenu);
                //AddNode(BaseType.Enumeration.ParameterNodeType, "Enumeration", parameterMenu, config('p', "00aaaa"), parameterOutput.Only().ToList(),
                //new List<NodeData.ParameterData> { nameParameter, enumTypeParameter,
                //new EnumDefaultParameter(m_types.GetEnumOptions, () => ID<ParameterType>.FromGuid(typeSelectionParameter.EditorSelected)});
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

            var category = new NodeData.ParameterData("Category", DomainIDs.NODE_CATEGORY, DomainIDs.CATEGORY_TYPE);
            var nodeName = new NodeData.ParameterData("Name", DomainIDs.NODE_NAME, BaseTypeString.PARAMETER_TYPE);
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
            public new static readonly ID<ParameterType> TypeId = ID<ParameterType>.Parse("82e83436-f1b0-4f71-8882-51c171d14ff3");

            Func<Dictionary<ID<ParameterType>, IEnumerable<EnumerationData.Element>>> m_enumOptions;

            private Dictionary<ID<ParameterType>, IEnumerable<EnumerationData.Element>> EnumOptions { get { return m_enumOptions(); } }

            Func<ID<ParameterType>> m_currentEnumType;

            Guid m_valueGuid;
            string m_value;

            public EnumDefaultParameter(Func<Dictionary<ID<ParameterType>, IEnumerable<EnumerationData.Element>>> enumOptions, Func<ID<ParameterType>> currentEnumType)
                : base("Default", DomainIDs.PARAMETER_DEFAULT, TypeId)
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
                    ID<ParameterType> guid = m_currentEnumType();
                    if (EnumOptions.ContainsKey(guid))
                        return EnumOptions[guid].Select(a => a.Name);
                    else
                        return new string[0];
                }
            }
        }

        private EditableGenerator AddEnumNode(NodeType parent)
        {
            ID<NodeTypeTemp> guid = BaseType.Enumeration.ParameterNodeType;
            string name = "Enumeration";
            ID<TConnector> parameterDefinitionConnector1 = ID<TConnector>.Parse("1fd8a64d-271e-42b8-bfd8-85e5174bbf9d");
            NodeData.ConnectorData parameterOutput = new NodeData.ConnectorData(parameterDefinitionConnector1, DomainIDs.PARAMETER_OUTPUT_DEFINITION.Id, new List<Parameter>());
            NodeData.ParameterData nameParameter = new NodeData.ParameterData("Name", DomainIDs.PARAMETER_NAME, BaseTypeString.PARAMETER_TYPE);
            NodeData.ParameterData enumTypeParameter = new NodeData.ParameterData("Type", DomainIDs.PARAMETER_TYPE, ENUM_SET_GUID);

            NodeData data;
            data.Config = this.config('p', "00aaaa");
            data.Connectors = parameterOutput.Only().ToList();
            data.Guid = guid;
            data.Name = name;
            data.Parameters = new List<NodeData.ParameterData> { nameParameter, enumTypeParameter };
            data.Type = parent.Guid;

            Func<Dictionary<ID<ParameterType>, IEnumerable<EnumerationData.Element>>> options = () => m_typeSet.VisibleEnums.ToDictionary(e => e.Guid, e => e.Elements.Select(a => a));

            var generator = new GenericEditableGenerator2(data, m_typeSet, ConnectorDefinitions, DomainConnectionRules.Instance,
                p => new List<Parameter> { new EnumDefaultParameter(options, () => ID<ParameterType>.FromGuid((p.Single(a => a.Id == enumTypeParameter.Id) as IEnumParameter).EditorSelected)) });
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

        //private EditableGenerator AddNode(ID<NodeTypeTemp> guid, string name, NodeType parent, List<NodeData.ConfigData> config, IEnumerable<OutputDefinition> outputs, Func<ID<NodeTemp>, EditableGenerator, IEnumerable<Func<IEditable, Output>>, ExternalFunction> func)
        //{
        //    var generator = new GenericEditableGenerator(name, guid, config,
        //        (id, g) =>
        //        {
        //            Func<OutputDefinition, Func<IEditable, Output>> curryNewID = output => data => output(data);
        //            return func(id, g, outputs.Select(curryNewID));
        //        });
        //    parent.m_nodes.Add(generator);
        //    m_nodes[guid] = generator;
        //    return generator;
        //}

        public IEnumerable<ID<ParameterType>> ParameterTypes
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

        public void RenameType(BaseType baseType, string name, ID<ParameterType> guid)
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

        public void RemoveType(BaseType baseType, ID<ParameterType> guid)
        {
            m_typeSet.Remove(guid);
            //m_types.RemoveType(baseType, guid);
        }

        public static void ForEachNode(IEnumerable<ConversationNode> nodes, Action<NodeTypeData> categoryAction, Action<IntegerData> integerAction, Action<DecimalData> decimalAction, Action<DynamicEnumerationData> dynamicEnumAction, Action<EnumerationData> enumerationAction, Action<EnumerationData> enumerationValueAction, Action<NodeData> nodeAction, Action<ConnectorDefinitionData> connectorAction)
        {
            ForEachNode(nodes.Select(n => n.m_data), categoryAction, integerAction, decimalAction, dynamicEnumAction, enumerationAction, enumerationValueAction, nodeAction, connectorAction);
        }
        public static void ForEachNode(IEnumerable<IEditable> nodes, Action<NodeTypeData> categoryAction, Action<IntegerData> integerAction, Action<DecimalData> decimalAction, Action<DynamicEnumerationData> dynamicEnumAction, Action<EnumerationData> enumerationAction, Action<EnumerationData> enumerationValueAction, Action<NodeData> nodeAction, Action<ConnectorDefinitionData> connectorAction)
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
                    integerAction(new IntegerData(name, ID<ParameterType>.FromGuid(node.NodeID.Guid), max, min/*, def*/));
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
                    decimalAction(new DecimalData(name, ID<ParameterType>.FromGuid(node.NodeID.Guid), max, min/*, def*/));
                }
                else if (node.NodeTypeID == BaseType.DynamicEnumeration.NodeType)
                {
                    var nameParameter = node.Parameters.Single(p => p.Id == DomainIDs.DYNAMIC_ENUM_NAME) as IStringParameter;
                    var name = nameParameter.Value;
                    dynamicEnumAction(new DynamicEnumerationData(name, ID<ParameterType>.FromGuid(node.NodeID.Guid)));
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
            enumerationAction(new EnumerationData(name, ID<ParameterType>.ConvertFrom(node.NodeID), elements/*, def*/));
        }

        internal void UpdateEnumeration(EnumerationData data)
        {
            //m_types.UpdateEnumOptions(data.Guid, data.Elements);
            m_typeSet.ModifyEnum(data);
        }

        public bool IsInteger(ID<ParameterType> type)
        {
            return m_typeSet.IsInteger(type);
            //return m_types.Integers.Is(type);
        }

        public bool IsDecimal(ID<ParameterType> type)
        {
            return m_typeSet.IsDecimal(type);
            //return m_types.Decimals.Is(type);
        }

        public bool IsEnum(ID<ParameterType> type)
        {
            //TODO: Figure out if these special cases still need to exist
            return m_typeSet.IsEnum(type) || type == DomainIDs.TYPES_GUID || type == DomainIDs.CATEGORY_TYPE || type == ConnectorPosition.ENUM_ID;
            //return m_types.Enumerations.Is(type) || type == DomainIDs.TYPES_GUID || type == DomainIDs.CATEGORY_TYPE || type == ConnectorPosition.ENUM_ID;
        }

        public bool IsDynamicEnum(ID<ParameterType> type)
        {
            //TODO: Figure out if this special case still needs to exist
            return m_typeSet.IsDynamicEnum(type) || type == EnumDefaultParameter.TypeId;
            //return m_types.DynamicEnumerations.Is(type) || type == EnumDefaultParameter.TypeId;
        }

        //CallbackDictionary<ID<TConnectorDefinition>, ConnectorDefinitionData> m_connectorDefinitions = new CallbackDictionary<ID<TConnectorDefinition>, ConnectorDefinitionData>()
        //{
        //    { SpecialConnectors.Input.Id, SpecialConnectors.Input },
        //    { SpecialConnectors.Output.Id, SpecialConnectors.Output },
        //};
        //private GenericEditableGenerator m_inputConnector;
        //private GenericEditableGenerator m_outputConnector;

        internal void ModifyConnector(ConnectorDefinitionData data)
        {
            ConnectorDefinitions[data.Id] = data;
        }

        internal void AddConnector(ConnectorDefinitionData data)
        {
            ConnectorDefinitions.Add(data.Id, data);
        }

        internal void RemoveConnector(ConnectorDefinitionData data)
        {
            ConnectorDefinitions.Remove(data.Id);
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

        //TODO: This contains both connectors for domain domain node as well as regular nodes. It shouldn't be both
        public static readonly CallbackDictionary<ID<TConnectorDefinition>, ConnectorDefinitionData> ConnectorDefinitions = new CallbackDictionary<ID<TConnectorDefinition>, ConnectorDefinitionData>()
        {
            { SpecialConnectors.Input.Id, SpecialConnectors.Input },
            { SpecialConnectors.Output.Id, SpecialConnectors.Output },
            {  DomainIDs.PARAMETER_OUTPUT_DEFINITION                                .Id,       DomainIDs.PARAMETER_OUTPUT_DEFINITION                     },
            {  DomainIDs.CONNECTOR_OUTPUT_DEFINITION                                .Id,       DomainIDs.CONNECTOR_OUTPUT_DEFINITION                     },
            {  DomainIDs.CONFIG_OUTPUT_DEFINITION                                   .Id,       DomainIDs.CONFIG_OUTPUT_DEFINITION                        },
            {  DomainIDs.NODE_OUTPUT_CONFIG_DEFINITION                              .Id,       DomainIDs.NODE_OUTPUT_CONFIG_DEFINITION                   },
            {  DomainIDs.NODE_OUTPUT_PARAMETERS_DEFINITION                          .Id,       DomainIDs.NODE_OUTPUT_PARAMETERS_DEFINITION               },
            {  DomainIDs.NODE_OUTPUT_CONNECTORS_DEFINITION                          .Id,       DomainIDs.NODE_OUTPUT_CONNECTORS_DEFINITION               },
            {  DomainIDs.CONNECTOR_DEFINITION_OUTPUT_DEFINITION                     .Id,       DomainIDs.CONNECTOR_DEFINITION_OUTPUT_DEFINITION          },
            {  DomainIDs.ENUM_VALUE_OUTPUT_DEFINITION                               .Id,       DomainIDs.ENUM_VALUE_OUTPUT_DEFINITION                    },
            {  DomainIDs.ENUM_OUTPUT_DEFINITION                                     .Id,       DomainIDs.ENUM_OUTPUT_DEFINITION                          },
        };
        private PluginsConfig m_pluginsConfig;
    }
}
