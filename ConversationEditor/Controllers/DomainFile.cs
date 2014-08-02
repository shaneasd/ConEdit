using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;
using ConversationNode = Conversation.ConversationNode<Conversation.INodeGUI>;
using Conversation.Serialization;

namespace ConversationEditor
{
    using TData = XmlGraphData<NodeUIData, ConversationEditorData>;
    using System.Windows;

    public interface IDomainFile : IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo>, IInProject
    {
        DomainData Data { get; }
    }

    public class DomainFile : GraphFile, IDomainFile
    {
        DomainDomain m_datasource;
        ConversationDataSource m_conversationDatasource;
        private ISerializer<TData> m_serializer;

        private SaveableFileUndoable m_file;
        public override ISaveableFileUndoable UndoableFile { get { return m_file; } }

        /// <summary>
        /// This domain file has been removed from the project so its contents should be removed from the datasource
        /// </summary>
        public void Removed()
        {
            ClearData();
        }

        public DomainFile(List<GraphAndUI<NodeUIData>> nodes, List<NodeGroup> groups, FileInfo file, List<Error> errors, DomainDomain datasource, ConversationDataSource conversationDataSource, ISerializer<TData> serializer, INodeFactory<ConversationNode> nodeFactory)
            : base(nodes, groups, errors, nodeFactory)
        {
            m_file = new SaveableFileUndoable(file, SaveTo);
            foreach (var node in m_nodes)
            {
                var n = node;
                node.Modified += () => NodeModified(n);
                node.Linked += () => NodeLinked(n);
            }
            m_nodes.Inserting += (n) =>
            {
                AddToData(n.m_data.Only(), m_datasource, m_conversationDatasource);
                n.Modified += () => NodeModified(n);
                n.Linked += () => NodeLinked(n);
            };
            m_nodes.Removing += (n) => RemoveFromData(n);
            m_nodes.Clearing += ClearData;
            m_datasource = datasource;
            m_conversationDatasource = conversationDataSource;
            m_serializer = serializer;
        }

        public static DomainFile CreateEmpty(DirectoryInfo directory, DomainDomain datasource, ConversationDataSource conversationDatasource, ISerializer<TData> serializer, Func<FileInfo, bool> pathOk, INodeFactory<ConversationNode> nodeFactory)
        {
            //Create a stream under an available filename
            FileInfo path = null;
            for (int i = 0; path == null; i++)
            {
                path = new FileInfo(directory.FullName + Path.DirectorySeparatorChar + "New Domain " + i + ".dom");
                if (!pathOk(path))
                    path = null;
            }

            using (var stream = Util.LoadFileStream(path, FileMode.CreateNew))
            {
                serializer.Write(SerializationUtils.MakeDomainData(Enumerable.Empty<ConversationNode>(), new ConversationEditorData()), stream);
            }

            return new DomainFile(new List<GraphAndUI<NodeUIData>>(), new List<NodeGroup>(), path, new List<Error>(), datasource, conversationDatasource, serializer, nodeFactory);
        }

        //TODO: Update conversation datasource with these changes
        // NodeModified
        // NodeLinked
        // RemoveFromData

        private void NodeModified(ConversationNode node)
        {
            Action<NodeTypeData> categoryAction = category =>
            {
                m_conversationDatasource.RenameCategory(category);
                m_datasource.RenameCategory(category.Name, category.Guid);
            };
            Action<IntegerData> integerAction = data =>
            {
                m_conversationDatasource.ModifyIntegerType(data);
                m_datasource.RenameType(BaseType.Integer, data.Name, data.TypeID);
            };
            Action<DecimalData> decimalAction = data =>
            {
                m_conversationDatasource.ModifyDecimalType(data);
                m_datasource.RenameType(BaseType.Decimal, data.Name, data.TypeID);
            };
            Action<DynamicEnumerationData> dynamicEnumAction = data =>
            {
                //There's no data that the conversation domain needs
                m_datasource.RenameType(BaseType.DynamicEnumeration, data.Name, data.TypeID);
            };
            Action<EnumerationData> enumAction = data =>
            {
                //No impact on the conversation datasource
                m_datasource.RenameType(BaseType.Enumeration, data.Name, data.Guid);
            };
            Action<EnumerationData> enumValueAction = data =>
            {
                m_conversationDatasource.UpdateEnumeration(data);
                m_datasource.UpdateEnumeration(data);
            };
            Action<NodeData> nodeAction = data =>
            {
                m_conversationDatasource.NodeDefinitionModified(data);
                //Doesn't affect the domain domain
            };
            Action<ConnectorDefinitionData> connectorAction = data =>
            {
                //TODO: Update conversation datasource?
                m_datasource.ModifyConnector(data);
            };

            if ( m_datasource.IsConnector(node.Type))
            {
                var nodeConnector = node.Connectors.Single(c => c.m_definition.Id == DomainIDs.CONNECTOR_OUTPUT_DEFINITION.Id);
                var nodes = nodeConnector.Connections.Select(c => c.Parent).Where(n => n.NodeTypeID == DomainIDs.NODE_GUID);
                DomainDomain.ForEachNode(nodes, categoryAction, integerAction, decimalAction, dynamicEnumAction, enumAction, enumValueAction, nodeAction, connectorAction);
            }
            else if ( m_datasource.IsParameter(node.Type))
            {
                var nodeConnector = node.Connectors.Single(c => c.m_definition.Id == DomainIDs.PARAMETER_OUTPUT_DEFINITION.Id);
                var nodes = nodeConnector.Connections.Select(c => c.Parent).Where(n => n.NodeTypeID == DomainIDs.NODE_GUID);
                DomainDomain.ForEachNode(nodes, categoryAction, integerAction, decimalAction, dynamicEnumAction, enumAction, enumValueAction, nodeAction, connectorAction);
            }
            else if ( m_datasource.IsConfig(node.Type))
            {
                var nodeConnector = node.Connectors.Single(c => c.m_definition.Id == DomainIDs.CONFIG_OUTPUT_DEFINITION.Id);
                var nodes = nodeConnector.Connections.Select(c => c.Parent).Where(n => n.NodeTypeID == DomainIDs.NODE_GUID);
                DomainDomain.ForEachNode(nodes, categoryAction, integerAction, decimalAction, dynamicEnumAction, enumAction, enumValueAction, nodeAction, connectorAction);
            }
            else
            {
                DomainDomain.ForEachNode(node.Only(), categoryAction, integerAction, decimalAction, dynamicEnumAction, enumAction, enumValueAction, nodeAction, connectorAction);
            }
        }

        /// <summary>
        /// Triggered when the set of linked nodes of 'node' changes. This includes unlinking as well as linking.
        /// </summary>
        private void NodeLinked(ConversationNode node)
        {
            Action<NodeTypeData> categoryAction = category => { }; //Can't link categories to anything
            Action<IntegerData> integerAction = data => { }; //Can't link integer definitions to anything
            Action<DecimalData> decimalAction = data => { }; //Can't link decimal definitions to anything
            Action<DynamicEnumerationData> dynamicEnumAction = data => { }; //Can't link dynamic enum definitions to anything
            Action<EnumerationData> enumAction = data => { m_datasource.UpdateEnumeration(data); };
            Action<EnumerationData> enumValueAction = data => { }; //No action required here. We'll let the enumeration definition node handle it
            Action<NodeData> nodeAction = data => { }; //Doesn't affect the domain domain
            Action<ConnectorDefinitionData> connectorAction = data => { m_datasource.ModifyConnector(data); };
            DomainDomain.ForEachNode(node.Only(), categoryAction, integerAction, decimalAction, dynamicEnumAction, enumAction, enumValueAction, nodeAction, connectorAction);
        }

        private static void AddToData(IEnumerable<IEditable> nodes, DomainDomain datasource, ConversationDataSource conversationDatasource)
        {
            Action<NodeTypeData> categoryAction = category => { datasource.AddCategory(category.Name, category.Guid); conversationDatasource.AddCategory(category); };
            Action<IntegerData> integerAction = data => { datasource.AddIntegerType(data); conversationDatasource.AddIntegerType(data); };
            Action<DecimalData> decimalAction = data => { datasource.AddDecimalType(data); conversationDatasource.AddDecimalType(data); };
            Action<DynamicEnumerationData> dynamicEnumAction = data => { datasource.AddDynamicEnumType(data); conversationDatasource.AddDynamicEnumType(data); };
            Action<EnumerationData> enumAction = data => { datasource.AddEnumType(data); datasource.UpdateEnumeration(data); conversationDatasource.AddEnumType(data); };
            Action<EnumerationData> enumValueAction = data => { }; //No action required here. We'll let the enumeration definition node handle it
            Action<NodeData> nodeAction = data => { conversationDatasource.AddNodeType(data); }; //No action required as it doesn't affect the domain domain
            Action<ConnectorDefinitionData> connectorAction = data => { datasource.AddConnector(data); conversationDatasource.AddConnector(data); };
            DomainDomain.ForEachNode(nodes, categoryAction, integerAction, decimalAction, dynamicEnumAction, enumAction, enumValueAction, nodeAction, connectorAction);
        }

        private void RemoveFromData(ConversationNode node)
        {
            Action<NodeTypeData> categoryAction = category => m_datasource.RemoveCategory(category.Guid);
            Action<IntegerData> integerAction = data => { m_datasource.RemoveType(BaseType.Integer, data.TypeID); };
            Action<DecimalData> decimalAction = data => { m_datasource.RemoveType(BaseType.Decimal, data.TypeID); };
            Action<DynamicEnumerationData> dynamicEnumAction = data => { m_datasource.RemoveType(BaseType.DynamicEnumeration, data.TypeID); };
            Action<EnumerationData> enumAction = data => { m_datasource.RemoveType(BaseType.Enumeration, data.Guid); };
            Action<EnumerationData> enumValueAction = data => { m_datasource.UpdateEnumeration(data); };
            Action<NodeData> nodeAction = data => { }; //No action required as it doesn't affect the domain domain
            Action<ConnectorDefinitionData> connectorAction = data => { m_datasource.RemoveConnector(data); };
            DomainDomain.ForEachNode(node.Only(), categoryAction, integerAction, decimalAction, dynamicEnumAction, enumAction, enumValueAction, nodeAction, connectorAction);
        }

        void ClearData() //TODO: Should check whether removing a domain file will cause warnings and check with the user before proceeding
        {
            foreach (var node in m_nodes)
            {
                RemoveFromData(node);
            }
        }

        private void SaveTo(Stream file)
        {
            file.Position = 0;
            file.SetLength(0);
            m_serializer.Write(SerializationUtils.MakeDomainData(Nodes, new ConversationEditorData { Groups = Groups }), file);
        }

        //TODO: Cache this and apply updates rather than regenerating every time
        public DomainData Data
        {
            get
            {
                DomainData data = new DomainData();
                DomainDomain.ForEachNode(m_nodes, data.NodeTypes.Add, data.Integers.Add, data.Decimals.Add, data.DynamicEnumerations.Add, data.Enumerations.Add, a => { }, data.Nodes.Add, data.Connectors.Add);
                return data;
            }
        }

        public static IEnumerable<Or<DomainFile, MissingDomainFile>> Load(IEnumerable<FileInfo> paths, DomainDomain source, ConversationDataSource conversationDatasource, DomainSerializerDeserializer serializerdeserializer, INodeFactory<ConversationNode> nodeFactory)
        {
            List<FileStream> streams = new List<FileStream>();
            try
            {
                foreach (var path in paths)
                {
                    FileStream loaded = null;
                    try
                    {
                        if (path.Exists)
                        {
                            try
                            {
                                loaded = Util.LoadFileStream(path, FileMode.Open, FileAccess.Read);
                            }
                            catch (FileLoadException e)
                            {
                                Console.Out.WriteLine(e.Message);
                                Console.Out.WriteLine(e.StackTrace);
                                Console.Out.WriteLine(e.InnerException.Message);
                                Console.Out.WriteLine(e.InnerException.StackTrace);
                                MessageBox.Show("File: " + path.Name + " exists but could not be accessed");
                            }
                        }
                        if (loaded != null)
                        {
                            streams.Add(loaded);
                            loaded = null;
                        }
                        else
                            yield return new MissingDomainFile(path);
                    }
                    finally
                    {
                        if (loaded != null)
                            loaded.Close();
                    }
                }

                foreach (var stream in streams)
                {
                    var categoryData = serializerdeserializer.CategoriesDeserializer.Read(stream);
                    DomainFile.AddToData(categoryData.Nodes.Select(n => n.GraphData), source, conversationDatasource);
                }
                foreach (var stream in streams)
                {
                    var typeData = serializerdeserializer.TypesDeserializer.Read(stream);
                    DomainFile.AddToData(typeData.Nodes.Select(n => n.GraphData), source, conversationDatasource);
                }
                foreach (var stream in streams)
                {
                    var connectorData = serializerdeserializer.ConnectorsDeserializer.Read(stream);
                    DomainFile.AddToData(connectorData.Nodes.Select(n => n.GraphData), source, conversationDatasource);
                }
                foreach (var stream in streams)
                {
                    var nodeData = serializerdeserializer.NodesDeserializer.Read(stream);
                    DomainFile.AddToData(nodeData.Nodes.Select(n => n.GraphData), source, conversationDatasource);
                }
                var data = streams.Select(stream =>
                {
                    var editorData = serializerdeserializer.EditorDataDeserializer.Read(stream);
                    DomainFile.AddToData(editorData.Nodes.Select(n => n.GraphData), source, conversationDatasource);
                    var allData = serializerdeserializer.EverythingDeserializer.Read(stream);
                    return new
                    {
                        editorData = editorData,
                        errors = allData.Errors,
                        allNodes = allData.Nodes,
                        path = stream.FileInfo(),
                    };
                }).Evaluate();

                foreach (var d in data)
                    yield return new DomainFile(d.allNodes.ToList(), d.editorData.EditorData.Groups.ToList(), d.path, d.errors.ToList(), source, conversationDatasource, serializerdeserializer.Serializer, nodeFactory);
            }
            finally
            {
                foreach (var stream in streams)
                    stream.Close();
            }
        }
    }
}
