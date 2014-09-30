﻿using System;
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
        event Action ConversationDomainModified;
    }

    public class DomainFile : GraphFile, IDomainFile
    {
        DomainDomain m_datasource;
        //ConversationDataSource m_conversationDatasource;
        private ISerializer<TData> m_serializer;
        Func<IDomainUsage<ConversationNode, TransitionNoduleUIInfo>> m_domainUsage;
        public event Action ConversationDomainModified;

        private SaveableFileUndoable m_file;
        public override ISaveableFileUndoable UndoableFile { get { return m_file; } }

        /// <summary>
        /// Attempt to update datasources to reflect removal of this file from the project.
        /// Attempt will fail if file contains data that is used within domain or conversation files in the project and the user does not accept this.
        /// </summary>
        public bool CanRemove(Func<bool> prompt)
        {
            bool asked = false;
            prompt = () =>
            {
                if (asked)
                    return true;
                else
                {
                    asked = true;
                    return prompt();
                }
            };
            return m_nodes.All(node => CanRemoveFromData(node, prompt));
        }

        public void Removed()
        {
            ClearData();
            ConversationDomainModified.Execute();
        }

        private void ClearData()
        {
            foreach (var node in m_nodes)
                RemoveFromData(node);
        }

        public DomainFile(List<GraphAndUI<NodeUIData>> nodes, List<NodeGroup> groups, MemoryStream rawData, FileInfo file, List<Error> errors, DomainDomain datasource, ConversationDataSource conversationDataSource, ISerializer<TData> serializer, INodeFactory<ConversationNode> nodeFactory, Func<IDomainUsage<ConversationNode, TransitionNoduleUIInfo>> domainUsage)
            : base(nodes, groups, errors, nodeFactory, null)
        {
            m_file = new SaveableFileUndoable(rawData, file, SaveTo);
            m_domainUsage = domainUsage;
            foreach (var node in m_nodes)
            {
                var n = node;
                node.Modified += () => NodeModified(n);
                node.Linked += () => NodeLinked(n);
            }
            m_nodes.Inserting += (n) =>
            {
                AddToData(n.m_data.Only(), m_datasource);
                ConversationDomainModified.Execute(); //No need to be picky about false positives
                n.Modified += () => NodeModified(n);
                n.Linked += () => NodeLinked(n);
            };
            m_nodes.Removing += RemoveFromData;
            m_nodes.Clearing += ClearData; //Currently nothing clears the list
            m_datasource = datasource;
            //m_conversationDatasource = conversationDataSource;
            m_serializer = serializer;
        }

        public static DomainFile CreateEmpty(DirectoryInfo directory, DomainDomain datasource, ConversationDataSource conversationDatasource, ISerializer<TData> serializer, Func<FileInfo, bool> pathOk, INodeFactory<ConversationNode> nodeFactory, Func<IDomainUsage<ConversationNode, TransitionNoduleUIInfo>> domainUsage)
        {
            //Create a stream under an available filename
            FileInfo path = null;
            for (int i = 0; path == null; i++)
            {
                path = new FileInfo(directory.FullName + Path.DirectorySeparatorChar + "New Domain " + i + ".dom");
                if (!pathOk(path))
                    path = null;
            }

            MemoryStream m = new MemoryStream();
            using (var stream = Util.LoadFileStream(path, FileMode.CreateNew))
            {
                serializer.Write(SerializationUtils.MakeDomainData(Enumerable.Empty<ConversationNode>(), new ConversationEditorData()), m);
                m.CopyTo(stream);
            }

            return new DomainFile(new List<GraphAndUI<NodeUIData>>(), new List<NodeGroup>(), m, path, new List<Error>(), datasource, conversationDatasource, serializer, nodeFactory, domainUsage);
        }

        private void NodeModified(ConversationNode node)
        {
            Action<NodeTypeData> categoryAction = category =>
            {
                //m_conversationDatasource.RenameCategory(category);
                m_datasource.RenameCategory(category.Name, category.Guid);
            };
            Action<IntegerData> integerAction = data =>
            {
                //m_conversationDatasource.ModifyIntegerType(data);
                m_datasource.RenameType(BaseType.Integer, data.Name, data.TypeID);
            };
            Action<DecimalData> decimalAction = data =>
            {
                //m_conversationDatasource.ModifyDecimalType(data);
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
                m_datasource.RenameType(BaseType.Enumeration, data.Name, data.TypeID);
            };
            Action<EnumerationData> enumValueAction = data =>
            {
                //m_conversationDatasource.UpdateEnumeration(data);
                m_datasource.UpdateEnumeration(data);
            };
            Action<NodeData> nodeAction = data =>
            {
                //m_conversationDatasource.NodeDefinitionModified(data);
                //Doesn't affect the domain domain
            };
            Action<ConnectorDefinitionData> connectorAction = data =>
            {
                //Update conversation datasource?
                m_datasource.ModifyConnector(data);
            };

            if (m_datasource.IsConnector(node.Type))
            {
                var nodeConnector = node.Connectors.Single(c => c.m_definition.Id == DomainIDs.CONNECTOR_OUTPUT_DEFINITION.Id);
                var nodes = nodeConnector.Connections.Select(c => c.Parent).Where(n => n.NodeTypeID == DomainIDs.NODE_GUID);
                DomainDomain.ForEachNode(nodes, categoryAction, integerAction, decimalAction, dynamicEnumAction, enumAction, enumValueAction, nodeAction, connectorAction);
            }
            else if (m_datasource.IsParameter(node.Type))
            {
                var nodeConnector = node.Connectors.Single(c => c.m_definition.Id == DomainIDs.PARAMETER_OUTPUT_DEFINITION.Id);
                var nodes = nodeConnector.Connections.Select(c => c.Parent).Where(n => n.NodeTypeID == DomainIDs.NODE_GUID);
                DomainDomain.ForEachNode(nodes, categoryAction, integerAction, decimalAction, dynamicEnumAction, enumAction, enumValueAction, nodeAction, connectorAction);
            }
            else if (m_datasource.IsConfig(node.Type))
            {
                var nodeConnector = node.Connectors.Single(c => c.m_definition.Id == DomainIDs.CONFIG_OUTPUT_DEFINITION.Id);
                var nodes = nodeConnector.Connections.Select(c => c.Parent).Where(n => n.NodeTypeID == DomainIDs.NODE_GUID);
                DomainDomain.ForEachNode(nodes, categoryAction, integerAction, decimalAction, dynamicEnumAction, enumAction, enumValueAction, nodeAction, connectorAction);
            }
            else
            {
                DomainDomain.ForEachNode(node.Only(), categoryAction, integerAction, decimalAction, dynamicEnumAction, enumAction, enumValueAction, nodeAction, connectorAction);
            }
            ConversationDomainModified.Execute(); //No need to be picky about false positives
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
            Action<EnumerationData> enumAction = data =>
            {
                m_datasource.UpdateEnumeration(data);
                //m_conversationDatasource.UpdateEnumeration(data);
            };
            Action<EnumerationData> enumValueAction = data => { }; //It's only possible to link to an enum definition and that's already handled by enumAction
            Action<NodeData> nodeAction = data =>
            {
                //Doesn't affect the domain domain
                //m_conversationDatasource.NodeDefinitionModified(data);
            };
            Action<ConnectorDefinitionData> connectorAction = data =>
            {
                m_datasource.ModifyConnector(data);
                //update conversation datasource when connector definition is linked to a parameter
            };
            DomainDomain.ForEachNode(node.Only(), categoryAction, integerAction, decimalAction, dynamicEnumAction, enumAction, enumValueAction, nodeAction, connectorAction);
            ConversationDomainModified.Execute(); //No need to be picky about false positives
        }

        private static void AddToData(IEnumerable<IEditable> nodes, DomainDomain datasource)
        {
            Action<NodeTypeData> categoryAction = category =>
            {
                datasource.AddCategory(category.Name, category.Guid);
                //conversationDatasource.AddCategory(category);
            };
            Action<IntegerData> integerAction = data =>
            {
                datasource.AddIntegerType(data);
                //conversationDatasource.AddIntegerType(data);
            };
            Action<DecimalData> decimalAction = data =>
            {
                datasource.AddDecimalType(data);
                //conversationDatasource.AddDecimalType(data);
            };
            Action<DynamicEnumerationData> dynamicEnumAction = data =>
            {
                datasource.AddDynamicEnumType(data);
                //conversationDatasource.AddDynamicEnumType(data);
            };
            Action<EnumerationData> enumAction = data =>
            {
                datasource.AddEnumType(data); datasource.UpdateEnumeration(data);
                //conversationDatasource.AddEnumType(data);
            };
            Action<EnumerationData> enumValueAction = data => { }; //No action required here. We'll let the enumeration definition node handle it
            Action<NodeData> nodeAction = data =>
            {
                //conversationDatasource.AddNodeType(data);
            }; //No action required as it doesn't affect the domain domain
            Action<ConnectorDefinitionData> connectorAction = data =>
            {
                datasource.AddConnector(data);
                //conversationDatasource.AddConnector(data);
            };
            DomainDomain.ForEachNode(nodes, categoryAction, integerAction, decimalAction, dynamicEnumAction, enumAction, enumValueAction, nodeAction, connectorAction);
        }

        /// <summary>
        /// Remove the node's content from the domain and conversation datasources.
        /// </summary>
        protected override void RemoveFromData(ConversationNode node)
        {
            Action<NodeTypeData> categoryAction = category =>
            {
                m_datasource.RemoveCategory(category.Guid);
                //m_conversationDatasource.RemoveCategory(category.Guid);
            };
            Action<IntegerData> integerAction = data =>
            {
                m_datasource.RemoveType(BaseType.Integer, data.TypeID);
                //m_conversationDatasource.RemoveType(data.TypeID);
            };
            Action<DecimalData> decimalAction = data =>
            {
                m_datasource.RemoveType(BaseType.Decimal, data.TypeID);
                //m_conversationDatasource.RemoveType(data.TypeID);
            };
            Action<DynamicEnumerationData> dynamicEnumAction = data =>
            {
                m_datasource.RemoveType(BaseType.DynamicEnumeration, data.TypeID);
                //m_conversationDatasource.RemoveType(data.TypeID);
            };
            Action<EnumerationData> enumAction = data =>
            {
                m_datasource.RemoveType(BaseType.Enumeration, data.TypeID);
                //m_conversationDatasource.RemoveType(data.TypeID);
            };
            Action<EnumerationData> enumValueAction = data =>
            {
                m_datasource.UpdateEnumeration(data);
                //m_conversationDatasource.UpdateEnumeration(data);
            };
            Action<NodeData> nodeAction = data =>
            {
                //Doesn't affect domain domain
                //m_conversationDatasource.RemoveNodeType(data.Guid);
            };
            Action<ConnectorDefinitionData> connectorAction = data =>
            {
                m_datasource.RemoveConnector(data);
                //m_conversationDatasource.RemoveConnector(data.Id);
            };
            DomainDomain.ForEachNode(node.Only(), categoryAction, integerAction, decimalAction, dynamicEnumAction, enumAction, enumValueAction, nodeAction, connectorAction);
            ConversationDomainModified.Execute(); //No need to be picky about false positives
        }

        /// <summary>
        /// True if a call to RemoveFromData will not fail with the specified input
        /// </summary>
        protected override bool CanRemoveFromData(ConversationNode node, Func<bool> prompt)
        {
            bool needToAsk = m_domainUsage().Usages(node).Any();
            if (needToAsk)
            {
                return prompt();
            }
            return true;
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

        public static IEnumerable<Or<DomainFile, MissingDomainFile>> Load(IEnumerable<FileInfo> paths, DomainDomain source, ConversationDataSource conversationDatasource, DomainSerializerDeserializer serializerdeserializer, INodeFactory<ConversationNode> nodeFactory, Func<IDomainUsage<ConversationNode, TransitionNoduleUIInfo>> domainUsage)
        {
            //List<FileStream> streams = new List<FileStream>();

            var streamsAndPaths = paths.Select(path =>
            {
                try
                {
                    using (var stream = Util.LoadFileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        MemoryStream m = new MemoryStream((int)stream.Length);
                        stream.CopyTo(m);
                        m.Position = 0;
                        return (Or<Tuple<MemoryStream, FileInfo>, MissingDomainFile>)Tuple.Create(m, path);
                    }
                }
                catch (MyFileLoadException e)
                {
                    Console.Out.WriteLine(e.Message);
                    Console.Out.WriteLine(e.StackTrace);
                    Console.Out.WriteLine(e.InnerException.Message);
                    Console.Out.WriteLine(e.InnerException.StackTrace);
                    MessageBox.Show("File: " + path.Name + " exists but could not be accessed");
                    return (Or<Tuple<MemoryStream, FileInfo>, MissingDomainFile>)(new MissingDomainFile(path));
                }
            }).Evaluate();

            foreach (var sp in streamsAndPaths)
            {
                sp.Do( stream =>
                {
                    var categoryData = serializerdeserializer.CategoriesDeserializer.Read(stream.Item1);
                    DomainFile.AddToData(categoryData.Nodes.Select(n => n.GraphData), source);
                }, a=>{});
            }
            foreach (var sp in streamsAndPaths)
            {
                sp.Do(stream =>
                {
                    var typeData = serializerdeserializer.TypesDeserializer.Read(stream.Item1);
                    DomainFile.AddToData(typeData.Nodes.Select(n => n.GraphData), source);
                }, a=>{});
            }
            foreach (var sp in streamsAndPaths)
            {
                sp.Do(stream =>
                {
                    var connectorData = serializerdeserializer.ConnectorsDeserializer.Read(stream.Item1);
                    DomainFile.AddToData(connectorData.Nodes.Select(n => n.GraphData), source);
                }, a=>{});
            }
            foreach (var sp in streamsAndPaths)
            {
                sp.Do(stream =>
                {
                    var nodeData = serializerdeserializer.NodesDeserializer.Read(stream.Item1);
                    DomainFile.AddToData(nodeData.Nodes.Select(n => n.GraphData), source);
                }, a=>{});
            }

            return streamsAndPaths.Select(a => a.TransformedOr(stream =>
                {
                    var editorData = serializerdeserializer.EditorDataDeserializer.Read(stream.Item1);
                    DomainFile.AddToData(editorData.Nodes.Select(n => n.GraphData), source);
                    var allData = serializerdeserializer.EverythingDeserializer.Read(stream.Item1);
                    return new DomainFile(allData.Nodes.ToList(), editorData.EditorData.Groups.ToList(), stream.Item1, stream.Item2, allData.Errors.ToList(), source, conversationDatasource, serializerdeserializer.Serializer, nodeFactory, domainUsage);
                }, b => b));
            //var data = streams.Select(stream =>
            //{
            //    return new
            //    {
            //        editorData = editorData,
            //        errors = allData.Errors,
            //        allNodes = allData.Nodes,
            //        path = stream.FileInfo(),
            //        memory = 
            //    };
            //}).Evaluate();
        }
    }
}
