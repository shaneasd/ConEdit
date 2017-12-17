using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;
using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGui>;
using Conversation.Serialization;

namespace ConversationEditor
{
    using TData = XmlGraphData<NodeUIData, ConversationEditorData>;
    using System.Windows;
    using System.Collections.ObjectModel;

    public class DomainFile : GraphFile, IDomainFile
    {
        DomainDomain m_datasource;
        //ConversationDataSource m_conversationDatasource;
        private ISerializer<TData> m_serializer;
        Func<IDomainUsage<ConversationNode, TransitionNoduleUIInfo>> m_domainUsage;
        public event Action ConversationDomainModified;

        private SaveableFileUndoable m_file;
        public override ISaveableFileUndoable UndoableFile { get { return m_file; } }

        public Id<FileInProject> Id { get; }

        /// <summary>
        /// Attempt to update datasources to reflect removal of this file from the project.
        /// Attempt will fail if file contains data that is used within domain or conversation files in the project and the user does not accept this.
        /// </summary>
        public bool CanRemove(Func<bool> prompt)
        {
            bool asked = false;
            Func<bool> check = () =>
            {
                if (asked)
                    return true;
                else
                {
                    asked = true;
                    return prompt();
                }
            };
            return m_nodes.All(node => CanRemoveFromData(node, check));
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="groups"></param>
        /// <param name="rawData">Represents the current contents of the file. Reference is not held. A copy is made.</param>
        /// <param name="file"></param>
        /// <param name="errors"></param>
        /// <param name="datasource"></param>
        /// <param name="serializer"></param>
        /// <param name="nodeFactory"></param>
        /// <param name="domainUsage"></param>
        /// <param name="getDocumentSource"></param>
        /// <param name="autoCompletePatterns"></param>
        public DomainFile(List<GraphAndUI<NodeUIData>> nodes, List<NodeGroup> groups, MemoryStream rawData, Id<FileInProject> file, DocumentPath path, ReadOnlyCollection<LoadError> errors, DomainDomain datasource, ISerializer<TData> serializer, INodeFactory nodeFactory, Func<IDomainUsage<ConversationNode, TransitionNoduleUIInfo>> domainUsage, Func<IDynamicEnumParameter, object, DynamicEnumParameter.Source> getDocumentSource, List<IAutoCompletePattern> autoCompletePatterns, UpToDateFile.BackEnd backEnd)
            : base(nodes, groups, errors, nodeFactory, null, getDocumentSource, NoAudio.Instance)
        {
            Id = file;
            m_file = new SaveableFileUndoable(rawData, path.FileInfo, SaveTo, backEnd);
            m_domainUsage = domainUsage;
            foreach (var node in m_nodes)
            {
                var n = node;
                node.Modified += () => NodeModified(n);
                node.Data.Linked += () => NodeLinked(n);
            }
            m_nodes.Inserting += (n) =>
            {
                AddToData(n.Data.Only(), m_datasource);
                ConversationDomainModified.Execute(); //No need to be picky about false positives
                n.Modified += () => NodeModified(n);
                n.Data.Linked += () => NodeLinked(n);
            };
            m_nodes.Removing += RemoveFromData;
            m_nodes.Clearing += ClearData; //Currently nothing clears the list
            m_datasource = datasource;
            //m_conversationDatasource = conversationDataSource;
            m_serializer = serializer;
            m_autoCompletePatterns = autoCompletePatterns;
        }

        public static DomainFile CreateEmpty(DirectoryInfo directory, DomainDomain datasource, ISerializer<TData> serializer, Func<FileInfo, bool> pathOk, INodeFactory nodeFactory, Func<IDomainUsage<ConversationNode, TransitionNoduleUIInfo>> domainUsage, Func<IDynamicEnumParameter, object, DynamicEnumParameter.Source> getDocumentSource, UpToDateFile.BackEnd backend, DirectoryInfo origin)
        {
            //Create a stream under an available filename
            FileInfo path = null;
            for (int i = 0; path == null; i++)
            {
                path = new FileInfo(directory.FullName + Path.DirectorySeparatorChar + "New Domain " + i + ".dom");
                if (!pathOk(path))
                    path = null;
            }

            using (MemoryStream m = new MemoryStream())
            {
                using (var stream = Util.LoadFileStream(path, FileMode.CreateNew, FileAccess.Write))
                {
                    serializer.Write(SerializationUtils.MakeDomainData(Enumerable.Empty<ConversationNode>(), new ConversationEditorData()), m);
                    m.CopyTo(stream);
                }

                var result = new DomainFile(new List<GraphAndUI<NodeUIData>>(), new List<NodeGroup>(), m, Id<FileInProject>.New(), DocumentPath.FromPath(path, origin), new ReadOnlyCollection<LoadError>(new LoadError[0]), datasource, serializer, nodeFactory, domainUsage, getDocumentSource, new List<IAutoCompletePattern>(), backend);
                result.m_file.Save(); //Make sure the file starts life as a valid xml document
                return result;
            }
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
                m_datasource.RenameType(BaseType.Integer, data.Name, data.TypeId);
            };
            Action<DecimalData> decimalAction = data =>
            {
                //m_conversationDatasource.ModifyDecimalType(data);
                m_datasource.RenameType(BaseType.Decimal, data.Name, data.TypeId);
            };
            Action<LocalizedStringData> localizedStringAction = data =>
            {
                m_datasource.RenameType(BaseType.LocalizedString, data.Name, data.TypeId);
            };
            Action<DynamicEnumerationData> dynamicEnumAction = data =>
            {
                //There's no data that the conversation domain needs
                m_datasource.RenameType(BaseType.DynamicEnumeration, data.Name, data.TypeId);
            };
            Action<LocalDynamicEnumerationData> localDynamicEnumAction = data =>
            {
                //There's no data that the conversation domain needs
                m_datasource.RenameType(BaseType.LocalDynamicEnumeration, data.Name, data.TypeId);
            };
            Action<EnumerationData> enumAction = data =>
            {
                //No impact on the conversation datasource
                m_datasource.RenameType(BaseType.Enumeration, data.Name, data.TypeId);
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
            Action<ConnectionDefinitionData> connectionAction = data =>
            {
                //Doesn't affect the domain domain
            };

            if (m_datasource.IsConnector(node.Data.NodeTypeId))
            {
                var nodeConnector = node.Data.Connectors.Single(c => c.Definition.Id == DomainIDs.ConnectorOutputDefinition.Id);
                var nodes = nodeConnector.Connections.Select(c => c.Parent).Where(n => n.NodeTypeId == DomainIDs.NodeGuid);
                DomainDomain.ForEachNode(nodes, categoryAction, integerAction, decimalAction, localizedStringAction, dynamicEnumAction, localDynamicEnumAction, enumAction, enumValueAction, nodeAction, connectorAction, connectionAction);
            }
            else if (m_datasource.IsParameter(node.Data.NodeTypeId))
            {
                var nodeConnector = node.Data.Connectors.Single(c => c.Definition.Id == DomainIDs.ParameterOutputDefinition.Id);
                var nodes = nodeConnector.Connections.Select(c => c.Parent).Where(n => n.NodeTypeId == DomainIDs.NodeGuid);
                DomainDomain.ForEachNode(nodes, categoryAction, integerAction, decimalAction, localizedStringAction, dynamicEnumAction, localDynamicEnumAction, enumAction, enumValueAction, nodeAction, connectorAction, connectionAction);
            }
            else if (m_datasource.IsConfig(node.Data.NodeTypeId))
            {
                var nodeConnector = node.Data.Connectors.Single(c => c.Definition.Id == DomainIDs.ConfigOutputDefinition.Id);
                var connected = nodeConnector.Connections.Select(c => c.Parent);

                var nodes = connected.Where(n => n.NodeTypeId == DomainIDs.NodeGuid);
                DomainDomain.ForEachNode(nodes, categoryAction, integerAction, decimalAction, localizedStringAction, dynamicEnumAction, localDynamicEnumAction, enumAction, enumValueAction, nodeAction, connectorAction, connectionAction);

                //Don't currently need to handle parameter config affecting nodes
                //var parameters = connected.Where(n => m_datasource.IsParameter(n.NodeTypeID));
                //var nodes2 = parameters.SelectMany(p=>p.Connectors.Where(c=>c.m_definition.Id == DomainIDs.PARAMETER_OUTPUT_DEFINITION.Id))
                //DomainDomain.ForEachNode
            }
            else
            {
                DomainDomain.ForEachNode(node.Only(), categoryAction, integerAction, decimalAction, localizedStringAction, dynamicEnumAction, localDynamicEnumAction, enumAction, enumValueAction, nodeAction, connectorAction, connectionAction);
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
            Action<LocalizedStringData> localizedStringAction = data => { }; //Can't link localized string definitions to anything
            Action<DynamicEnumerationData> dynamicEnumAction = data => { }; //Can't link dynamic enum definitions to anything
            Action<LocalDynamicEnumerationData> localDynamicEnumAction = data => { }; //Can't link local dynamic enum definitions to anything
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
            Action<ConnectionDefinitionData> connectionAction = data => { }; //Can't line connection definitions to anything
            DomainDomain.ForEachNode(node.Only(), categoryAction, integerAction, decimalAction, localizedStringAction, dynamicEnumAction, localDynamicEnumAction, enumAction, enumValueAction, nodeAction, connectorAction, connectionAction);
            ConversationDomainModified.Execute(); //No need to be picky about false positives
        }

        private static void AddToData(IEnumerable<IConversationNodeData> nodes, DomainDomain datasource)
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
            Action<LocalizedStringData> localizedStringAction = data =>
            {
                datasource.AddLocalizedStringType(data);
            };
            Action<DynamicEnumerationData> dynamicEnumAction = data =>
            {
                datasource.AddDynamicEnumType(data);
                //conversationDatasource.AddDynamicEnumType(data);
            };
            Action<LocalDynamicEnumerationData> localDynamicEnumAction = data =>
            {
                datasource.AddLocalDynamicEnumType(data);
                //conversationDatasource.AddLocalDynamicEnumType(data);
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
            Action<ConnectionDefinitionData> connectionAction = data =>
            {
                //No action required as it doesn't affect the domain domain
            };
            DomainDomain.ForEachNode(nodes, categoryAction, integerAction, decimalAction, localizedStringAction, dynamicEnumAction, localDynamicEnumAction, enumAction, enumValueAction, nodeAction, connectorAction, connectionAction);
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
                m_datasource.RemoveType(BaseType.Integer, data.TypeId);
                //m_conversationDatasource.RemoveType(data.TypeID);
            };
            Action<DecimalData> decimalAction = data =>
            {
                m_datasource.RemoveType(BaseType.Decimal, data.TypeId);
                //m_conversationDatasource.RemoveType(data.TypeID);
            };
            Action<LocalizedStringData> localizedStringAction = data =>
            {
                m_datasource.RemoveType(BaseType.LocalizedString, data.TypeId);
            };
            Action<DynamicEnumerationData> dynamicEnumAction = data =>
            {
                m_datasource.RemoveType(BaseType.DynamicEnumeration, data.TypeId);
                //m_conversationDatasource.RemoveType(data.TypeID);
            };
            Action<LocalDynamicEnumerationData> localDynamicEnumAction = data =>
            {
                m_datasource.RemoveType(BaseType.LocalDynamicEnumeration, data.TypeId);
                //m_conversationDatasource.RemoveType(data.TypeID);
            };
            Action<EnumerationData> enumAction = data =>
            {
                m_datasource.RemoveType(BaseType.Enumeration, data.TypeId);
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
            Action<ConnectionDefinitionData> connectionAction = data =>
            {
                //Doesn't affect domain domain
            };
            DomainDomain.ForEachNode(node.Only(), categoryAction, integerAction, decimalAction, localizedStringAction, dynamicEnumAction, localDynamicEnumAction, enumAction, enumValueAction, nodeAction, connectorAction, connectionAction);
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
            m_serializer.Write(SerializationUtils.MakeDomainData(Nodes, new ConversationEditorData(Groups)), file);
        }

        //TODO: Cache this and apply updates rather than regenerating every time
        public IDomainData Data
        {
            get
            {
                DomainData data = new DomainData();
                DomainDomain.ForEachNode(m_nodes, data.NodeTypes.Add, data.Integers.Add, data.Decimals.Add, data.LocalizedStrings.Add, data.DynamicEnumerations.Add, data.LocalDynamicEnumerations.Add, data.Enumerations.Add, a => { }, data.Nodes.Add, data.Connectors.Add, data.Connections.Add);
                return data;
            }
        }

        internal static IEnumerable<IDomainFile> Load(IEnumerable<Tuple<Id<FileInProject>, DocumentPath>> paths, DomainDomain source, Func<DocumentPath, DomainSerializerDeserializer> serializerdeserializer, INodeFactory nodeFactory, Func<IDomainUsage<ConversationNode, TransitionNoduleUIInfo>> domainUsage, Func<IDynamicEnumParameter, object, DynamicEnumParameter.Source> getDocumentSource, UpToDateFile.BackEnd backend)
        {
            var streamsAndPaths = paths.Select((x) =>
            {
                var fileId = x.Item1;
                DocumentPath path = x.Item2;
                try
                {
                    using (var stream = Util.LoadFileStream(path.FileInfo, FileMode.Open, FileAccess.Read))
                    {
                        return (Either<Tuple<MemoryStream, Id<FileInProject>, DocumentPath>, MissingDomainFile>)Tuple.Create(StreamUtil.Copy(stream), fileId, path);
                    }
                }
                catch (MyFileLoadException e)
                {
                    Console.Out.WriteLine(e.Message);
                    Console.Out.WriteLine(e.StackTrace);
                    Console.Out.WriteLine(e.InnerException.Message);
                    Console.Out.WriteLine(e.InnerException.StackTrace);
                    if (path.Exists)
                        MessageBox.Show("File: " + path.AbsolutePath + " could not be accessed");
                    else
                        MessageBox.Show("File: " + path.AbsolutePath + " does not exist");

                    MissingDomainFile temp = null;
                    try
                    {
                        temp = new MissingDomainFile(fileId, path);
                        Either<Tuple<MemoryStream, Id<FileInProject>, DocumentPath>, MissingDomainFile> result = temp;
                        return result;
                    }
                    catch
                    {
                        temp?.Dispose();
                        throw;
                    }
                }
            }).Evaluate();

            //We make the, hopefully, valid assumption here that the deserializers for the various concepts within a domain file have the same version requirements.
            Dictionary<DocumentPath, DeserializerVersionMismatchException> failedToParseFiles = new Dictionary<DocumentPath, DeserializerVersionMismatchException>();

            foreach (var sp in streamsAndPaths)
            {
                sp.Do(stream =>
                {
                    try
                    {
                        var categoryData = serializerdeserializer(stream.Item3).CategoriesDeserializer.Read(stream.Item1);
                        DomainFile.AddToData(categoryData.Nodes.Select(n => n.GraphData), source);
                    }
                    catch (DeserializerVersionMismatchException e)
                    {
                        failedToParseFiles[stream.Item3] = e;
                    }
                }, a => { });
            }
            foreach (var sp in streamsAndPaths)
            {
                sp.Do(stream =>
                {
                    try
                    {
                        var typeData = serializerdeserializer(stream.Item3).TypesDeserializer.Read(stream.Item1);
                        DomainFile.AddToData(typeData.Nodes.Select(n => n.GraphData), source);
                    }
                    catch (DeserializerVersionMismatchException e)
                    {
                        failedToParseFiles[stream.Item3] = e;
                    }
                }, a => { });
            }
            foreach (var sp in streamsAndPaths)
            {
                sp.Do(stream =>
                {
                    try
                    {
                        var connectorData = serializerdeserializer(stream.Item3).ConnectorsDeserializer.Read(stream.Item1);
                        DomainFile.AddToData(connectorData.Nodes.Select(n => n.GraphData), source);
                    }
                    catch (DeserializerVersionMismatchException e)
                    {
                        failedToParseFiles[stream.Item3] = e;
                    }
                }, a => { });
            }
            foreach (var sp in streamsAndPaths)
            {
                sp.Do(stream =>
                {
                    try
                    {
                        var nodeData = serializerdeserializer(stream.Item3).NodesDeserializer.Read(stream.Item1);
                        DomainFile.AddToData(nodeData.Nodes.Select(n => n.GraphData), source);
                    }
                    catch (DeserializerVersionMismatchException e)
                    {
                        failedToParseFiles[stream.Item3] = e;
                    }
                }, a => { });
            }

            {
                var result = streamsAndPaths.Select(a => a.Transformed<IDomainFile>(stream =>
                    {
                        try
                        {
                            var editorData = serializerdeserializer(stream.Item3).EditorDataDeserializer.Read(stream.Item1);
                            DomainFile.AddToData(editorData.Nodes.Select(n => n.GraphData), source);
                            var allData = serializerdeserializer(stream.Item3).EverythingDeserializer.Read(stream.Item1);

                            List<IAutoCompletePattern> autoCompletePatterns = new List<IAutoCompletePattern>();
                            var nodeData = serializerdeserializer(stream.Item3).AutoCompleteSuggestionsDeserializer.Read(stream.Item1);
                            autoCompletePatterns.AddRange(AutoCompletePattern.Generate(nodeData, source));

                            return new DomainFile(allData.Nodes.ToList(), editorData.EditorData.Groups.ToList(), stream.Item1, stream.Item2, stream.Item3, allData.Errors, source, serializerdeserializer(stream.Item3).Serializer, nodeFactory, domainUsage, getDocumentSource, autoCompletePatterns, backend);
                        }
                        catch (DeserializerVersionMismatchException e)
                        {
                            failedToParseFiles[stream.Item3] = e;
                            return new MissingDomainFile(stream.Item2, stream.Item3);
                        }
                    }, b => b));

                if (failedToParseFiles.Any())
                {
                    StringBuilder message = new StringBuilder("Failed to parse files:\n");
                    foreach (var kvp in failedToParseFiles)
                    {
                        message.Append("\n");
                        message.Append(kvp.Key.RelativePath);
                        message.Append(": ");
                        message.Append(kvp.Value.Message);
                    }
                    MessageBox.Show(message.ToString());
                }

                return result;
            }
        }

        List<IAutoCompletePattern> m_autoCompletePatterns;

        public IEnumerable<string> AutoCompleteSuggestions(IParameter p, string s, Func<ParameterType, DynamicEnumParameter.Source> enumSource)
        {
            return m_autoCompletePatterns.SelectMany(acp => acp.AutoCompleteSuggestions(p, s, enumSource));
        }
    }
}
