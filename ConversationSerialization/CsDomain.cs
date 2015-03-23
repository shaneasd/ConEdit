﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom.Compiler;
using Conversation;
using Utilities;
using System.CodeDom;
using Microsoft.CSharp;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Drawing;

namespace Conversation.Serialization
{
    public static class CodeDomHelper
    {
        public static CodeExpression MakeGuid(Guid guid)
        {
            return new CodeObjectCreateExpression(new CodeTypeReference(typeof(Guid)), new CodePrimitiveExpression(guid.ToString()));
        }

        public static CodeExpression MakeID<T>(ID<T> id)
        {
            return new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(typeof(ID<T>)), "Parse"), new CodePrimitiveExpression(id.Serialized()));
        }

        public static CodeExpression MakeTuple<T1, T2>(CodeExpression a, CodeExpression b)
        {
            return new CodeObjectCreateExpression(new CodeTypeReference(typeof(Tuple<T1, T2>)), a, b);
        }
    }

    public static class CsDomain<TNodeUI, TUIRawData, TEditorData> where TNodeUI : INodeUI<TNodeUI>
    {
        public class Serializer : ISerializer<IEnumerable<DomainData>>
        {
            static readonly CSharpCodeProvider generator = new CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v3.5" } });
            private static readonly List<NodeData.ConfigData> ParameterConfig = new List<NodeData.ConfigData>(); //Parameter config is not relevant to parsing of conversations and so is not written out to the C#

            private static string RemoveSpaces(string a)
            {
                var result = a.Replace(' ', '_').Replace('-', '_').Where(c => char.IsLetterOrDigit(c) || c == '_');
                if (result.Any())
                    if (char.IsDigit(result.First()))
                        result = '_'.Only().Concat(result);
                return new string(result.ToArray());
            }

            private readonly string m_namespace;
            private readonly Dictionary<ParameterType, string> m_basicTypeMap;
            public Serializer(Dictionary<ParameterType, string> basicTypeMap, string @namespace)
            {
                m_namespace = @namespace;
                m_basicTypeMap = basicTypeMap;
            }

            private static string BestName(string start, HashSet<string> usedNames)
            {
                var guess = RemoveSpaces(start);
                while (usedNames.Contains(guess))
                    guess += "_";
                usedNames.Add(guess);
                return guess;
            }

            public void Write(IEnumerable<DomainData> alldata, System.IO.Stream stream)
            {
                CodeCompileUnit file = new CodeCompileUnit();

                HashSet<string> usedNames = new HashSet<string>() { "Node" };

                DomainData data = ConvertData(alldata, usedNames);

                Dictionary<ParameterType, string> basicTypeMap = MakeTypeMap(data);
                file.Namespaces.Add(GenerateTypes(data));
                var connectors = GenerateConnectors(data, basicTypeMap);
                file.Namespaces.Add(connectors.Item1);
                var categoryNameSpaces = GenerateCategories(data);
                var nodeNames = GenerateNodes(data, basicTypeMap, connectors, categoryNameSpaces);
                foreach (var n in categoryNameSpaces.Item1)
                    file.Namespaces.Add(n);

                var serializationNamespace = new CodeNamespace(m_namespace);
                foreach (CodeNamespace @namespace in file.Namespaces)
                    serializationNamespace.Imports.Add(new CodeNamespaceImport(@namespace.Name)); //This one will likely need everything else
                CodeTypeDeclaration deserializer = GenerateDeserializer(nodeNames);
                serializationNamespace.Types.Add(deserializer);
                List<CodeTypeDeclaration> processors = GenerateProcessors(nodeNames);
                foreach (var processor in processors)
                    serializationNamespace.Types.Add(processor);

                file.Namespaces.Add(serializationNamespace);

                WriteToStream(stream, file);
            }

            private List<CodeTypeDeclaration> GenerateProcessors(Dictionary<ID<NodeTypeTemp>, string> nodeNames)
            {
                CodeTypeDeclaration actionProcessor = new CodeTypeDeclaration("IProcessor") { Attributes = MemberAttributes.Public, IsInterface = true };
                CodeTypeDeclaration funcProcessor = new CodeTypeDeclaration("IProcessor") { Attributes = MemberAttributes.Public, IsInterface = true };
                funcProcessor.TypeParameters.Add(new CodeTypeParameter("T"));

                foreach (var node in nodeNames)
                {
                    var nodeName = node.Value;
                    var processNodeT = new CodeMemberMethod() { Attributes = MemberAttributes.Public, Name = "ProcessNode" };
                    processNodeT.ReturnType = new CodeTypeReference("T");
                    processNodeT.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(nodeName), "node"));
                    funcProcessor.Members.Add(processNodeT);

                    var processNode = new CodeMemberMethod() { Attributes = MemberAttributes.Public, Name = "ProcessNode" };
                    processNode.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(nodeName), "node"));
                    actionProcessor.Members.Add(processNode);
                }

                return new List<CodeTypeDeclaration> { actionProcessor, funcProcessor };
            }

            private static CodeTypeDeclaration GenerateDeserializer(Dictionary<ID<NodeTypeTemp>, string> nodeNames)
            {
                CodeTypeDeclaration deserializer = new CodeTypeDeclaration("Deserializer") { TypeAttributes = TypeAttributes.Public, IsClass = true };
                deserializer.BaseTypes.Add(typeof(IDeserializer<RuntimeConversation.Conversation>));

                //var connectorLookupFactory = new CodeMemberMethod() { Attributes = MemberAttributes.Private | MemberAttributes.Static , Name = "MakeConnectorLookup"};
                //var lookuptype = new CodeTypeReference(typeof(Dictionary<ID<TConnector>, Func<RuntimeConversation.Node, RuntimeConversation.Connector>>));
                //connectorLookupFactory.ReturnType = lookuptype;
                //connectorLookupFactory.Statements.Add(new CodeVariableDeclarationStatement(lookuptype, "result", new CodeObjectCreateExpression(lookuptype)));
                //foreach ( var kvp in basicTypeMap )
                //connectorLookupFactory.Statements.Add( new CodeMethodInvokeExpression(new CodeMethodReferenceExpression("result", "Add"), CodeDomHelper.MakeTuple(  CodeDomHelper.MakeGuid(kvp.Key), new CodeFieldReferenceExpression( kvp.Value))
                //deserializer.Members.Add(connectorLookupFactory );

                {
                    var read = new CodeMemberMethod() { Attributes = MemberAttributes.Public, ReturnType = new CodeTypeReference(typeof(RuntimeConversation.Conversation)), Name = "Read" };
                    read.Parameters.Add(new CodeParameterDeclarationExpression(typeof(System.IO.Stream), "stream"));
                    // Func<ID<NodeTypeTemp>, ID<NodeTemp>, IEnumerable<Parameter>, Or<RuntimeConversation.Node, Error>> datasource
                    read.Statements.Add(new CodeVariableDeclarationStatement(typeof(CustomDeserializer), "backend", new CodeObjectCreateExpression(typeof(CustomDeserializer), new CodeMethodReferenceExpression(null, "GetNode"))));
                    read.Statements.Add(new CodeMethodReturnStatement(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeVariableReferenceExpression("backend"), "Read"), new CodeArgumentReferenceExpression("stream"))));

                    //read.Statements.Add(new CodeTryCatchFinallyStatement(
                    deserializer.Members.Add(read);
                }

                {
                    //Func<ID<NodeTypeTemp>, ID<NodeTemp>, IEnumerable<Parameter>, PointF, Or<RuntimeConversation.Node, Error>> m_datasource
                    var getNode = new CodeMemberMethod() { Attributes = MemberAttributes.Private, ReturnType = new CodeTypeReference(typeof(Or<RuntimeConversation.NodeBase, Error>)), Name = "GetNode" };
                    getNode.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(ID<NodeTypeTemp>)), "typeid"));
                    getNode.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(ID<NodeTemp>)), "id"));
                    getNode.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(IEnumerable<CustomDeserializer.Parameter>)), "parameters"));
                    getNode.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(PointF)), "position"));

                    foreach (var node in nodeNames)
                    {
                        getNode.Statements.Add(new CodeConditionStatement(new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("typeid"), "Equals", CodeDomHelper.MakeID(node.Key)),
                                               new CodeVariableDeclarationStatement(new CodeTypeReference(node.Value), "node", new CodeObjectCreateExpression(new CodeTypeReference(node.Value), new CodeArgumentReferenceExpression("id"), new CodeArgumentReferenceExpression("position"))),
                                               new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("node"), "GetParameters", new CodeArgumentReferenceExpression("parameters"))),
                                               new CodeMethodReturnStatement(new CodeVariableReferenceExpression("node"))
                                               ));
                    }
                    getNode.Statements.Add(new CodeMethodReturnStatement(new CodeObjectCreateExpression(typeof(Error), new CodePrimitiveExpression("Failed to create node"))));
                    deserializer.Members.Add(getNode);
                }
                return deserializer;
            }

            private static DomainData ConvertData(IEnumerable<DomainData> alldata, HashSet<string> usedNames)
            {
                DomainData data = new DomainData();
                foreach (var a in alldata)
                {
                    foreach (var b in a.Connectors)
                    {
                        string name = BestName(b.Name, usedNames);
                        var parameterNames = new HashSet<string>() { name };
                        List<NodeData.ParameterData> parameters = b.Parameters.Select(p => new NodeData.ParameterData(BestName(p.Name, parameterNames), p.Id, p.Type, ParameterConfig, p.Default)).ToList();
                        data.Connectors.Add(new ConnectorDefinitionData(name, b.Id, parameters, b.Position));
                    }
                    foreach (var b in a.Decimals)
                        data.Decimals.Add(new DecimalData(BestName(b.Name, usedNames), b.TypeID, b.Max, b.Min));
                    foreach (var b in a.DynamicEnumerations)
                        data.DynamicEnumerations.Add(new DynamicEnumerationData(BestName(b.Name, usedNames), b.TypeID));
                    foreach (var b in a.Enumerations)
                    {
                        var enumValueNames = new HashSet<string>() { };
                        data.Enumerations.Add(new EnumerationData(BestName(b.Name, usedNames), b.TypeID, b.Elements.Select(e => new EnumerationData.Element(BestName(e.Name, enumValueNames), e.Guid))));
                    }
                    foreach (var b in a.Integers)
                        data.Integers.Add(new IntegerData(BestName(b.Name, usedNames), b.TypeID, b.Max, b.Min));
                    foreach (var b in a.Nodes)
                    {
                        string name = BestName(b.Name, usedNames);
                        var parameterNames = new HashSet<string>() { name };
                        List<NodeData.ParameterData> parameters = b.Parameters.Select(p => new NodeData.ParameterData(BestName(p.Name, parameterNames), p.Id, p.Type, ParameterConfig, p.Default)).ToList();
                        data.Nodes.Add(new NodeData(name, b.Type, b.Guid, b.Connectors, parameters, b.Config));
                    }
                    foreach (var b in a.NodeTypes)
                        data.NodeTypes.Add(new NodeTypeData(BestName(b.Name, usedNames), b.Guid, b.Parent));
                    //data.Connectors.AddRange(a.Connectors);
                    //data.Decimals.AddRange(a.Decimals);
                    //data.DynamicEnumerations.AddRange(a.DynamicEnumerations);
                    //data.Enumerations.AddRange(a.Enumerations);
                    //data.Integers.AddRange(a.Integers);
                    //data.Nodes.AddRange(a.Nodes);
                    //data.NodeTypes.AddRange(a.NodeTypes);
                }
                return data;
            }

            private Dictionary<ID<NodeTypeTemp>, string> GenerateNodes(DomainData data, Dictionary<ParameterType, string> basicTypeMap, Tuple<CodeNamespace, Func<ID<TConnectorDefinition>, string>> connectors, Tuple<List<CodeNamespace>, Func<Guid, CodeNamespace>> categoryNameSpaces)
            {
                {
                    var baseNamespace = categoryNameSpaces.Item2(Guid.Empty);
                    var baseNode = new CodeTypeDeclaration("Node") { IsClass = true, TypeAttributes = TypeAttributes.Public | TypeAttributes.Abstract };
                    baseNode.BaseTypes.Add(new CodeTypeReference(typeof(RuntimeConversation.NodeBase)));

                    var process = new CodeMemberMethod() { Attributes = MemberAttributes.Public | MemberAttributes.Abstract, Name = "Process" };
                    process.Parameters.Add(new CodeParameterDeclarationExpression("IProcessor", "processor"));

                    var processT = new CodeMemberMethod() { Attributes = MemberAttributes.Public | MemberAttributes.Abstract, Name = "Process" };
                    processT.TypeParameters.Add(new CodeTypeParameter("T"));
                    processT.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("IProcessor", new CodeTypeReference("T")), "processor"));
                    processT.ReturnType = new CodeTypeReference("T");

                    var constructor = new CodeConstructor() { Attributes = MemberAttributes.Public };
                    constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(ID<NodeTemp>), "id"));
                    constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(PointF), "position"));
                    constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("id"));
                    constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("position"));
                    baseNode.Members.Add(constructor);

                    var Connector = new CodeMemberMethod() { Attributes = MemberAttributes.Public | MemberAttributes.Abstract, ReturnType = new CodeTypeReference("Connector"), Name = "Connector" };
                    Connector.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(ID<TConnector>)), "connector"));

                    var Connect = new CodeMemberMethod() { Attributes = MemberAttributes.Public | MemberAttributes.Override, Name = "Connect" };
                    Connect.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(ID<TConnector>)), "thisConnectorID"));
                    Connect.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(RuntimeConversation.NodeBase)), "other"));
                    Connect.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(ID<TConnector>)), "otherConnectorID"));
                    Connect.Statements.Add(new CodeMethodInvokeExpression(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "Connector", new CodeArgumentReferenceExpression("thisConnectorID")), "ConnectTo",
                                                                          new CodeMethodInvokeExpression(new CodeCastExpression(new CodeTypeReference("Node"), new CodeArgumentReferenceExpression("other")), "Connector", new CodeArgumentReferenceExpression("otherConnectorID"))));

                    baseNode.Members.Add(Connect);
                    baseNode.Members.Add(process);
                    baseNode.Members.Add(processT);
                    baseNode.Members.Add(Connector);
                    baseNamespace.Types.Add(baseNode);
                }


                Dictionary<ID<NodeTypeTemp>, string> result = new Dictionary<ID<NodeTypeTemp>, string>();
                foreach (var nodeType in data.Nodes)
                {
                    CodeTypeDeclaration type = new CodeTypeDeclaration(nodeType.Name) { IsClass = true, Attributes = MemberAttributes.Final };
                    type.BaseTypes.Add(new CodeTypeReference("Node"));
                    type.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(RuntimeConversation.NodeTypeIDAttribute)), new CodeAttributeArgument(new CodePrimitiveExpression(nodeType.Guid.Guid.ToString()))));
                    var @namespace = categoryNameSpaces.Item2(nodeType.Type ?? Guid.Empty);

                    //type.Members.Add(new CodeMemberField(typeof(ID<NodeTemp>), "m_id"));

                    {
                        var constructor = new CodeConstructor() { Attributes = MemberAttributes.Public };
                        constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(ID<NodeTemp>), "id"));
                        constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(PointF), "position"));
                        constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("id"));
                        constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("position"));
                        //constructor.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, "m_id"), new CodeArgumentReferenceExpression("id")));
                        type.Members.Add(constructor);

                        var connector = new CodeMemberMethod() { Attributes = MemberAttributes.Public | MemberAttributes.Override, Name = "Connector", ReturnType = new CodeTypeReference("Connector") };
                        connector.Parameters.Add(new CodeParameterDeclarationExpression(typeof(ID<TConnector>), "connector"));
                        foreach (var c in nodeType.Connectors)
                        {
                            var connectorName = "id" + c.Id.Guid.ToString("N");
                            type.Members.Add(new CodeMemberField(connectors.Item2(c.TypeID), connectorName) { Attributes = MemberAttributes.Public });
                            //constructor.Parameters.Add(new CodeParameterDeclarationExpression(connectors.Item2(c.TypeID), connectorName));

                            var makeConnector = new CodeObjectCreateExpression(connectors.Item2(c.TypeID), new CodeThisReferenceExpression(), CodeDomHelper.MakeID(c.Id));
                            foreach (var parameter in c.Parameters)
                            {
                                string randomVariableName = "_" + Guid.NewGuid().ToString("N");
                                constructor.Statements.Add(new CodeVariableDeclarationStatement(basicTypeMap[parameter.TypeId], randomVariableName));
                                constructor.Statements.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("TypeDeserializer"), "Deserialize", new CodeDirectionExpression(FieldDirection.Out, new CodeVariableReferenceExpression(randomVariableName)), new CodePrimitiveExpression(parameter.ValueAsString()))));
                                makeConnector.Parameters.Add(new CodeVariableReferenceExpression(randomVariableName));
                            }
                            constructor.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), connectorName), makeConnector));
                            connector.Statements.Add(new CodeConditionStatement(new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("connector"), "Equals", CodeDomHelper.MakeID(c.Id)),
                                                     new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, connectorName))));
                        }
                        connector.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(null)));
                        type.Members.Add(connector);
                    }

                    var getParameters = new CodeMemberMethod() { Attributes = MemberAttributes.Public, Name = "GetParameters" };
                    getParameters.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(IEnumerable<CustomDeserializer.Parameter>)), "parameters"));

                    foreach (var parameter in nodeType.Parameters)
                    {
                        var p = GenerateParameter(basicTypeMap, parameter);
                        var pname = p.Name;
                        type.Members.Add(p);
                        getParameters.Statements.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeTypeReferenceExpression("TypeDeserializer"), "Deserialize"),
                                                     new CodeDirectionExpression(FieldDirection.Out, new CodeFieldReferenceExpression(null, pname)),
                                                     new CodeMethodInvokeExpression(null, "GetParameter", new CodeArgumentReferenceExpression("parameters"), CodeDomHelper.MakeGuid(parameter.Id.Guid)))));
                    }

                    var process = new CodeMemberMethod() { Attributes = MemberAttributes.Public | MemberAttributes.Override, Name = "Process" };
                    process.Parameters.Add(new CodeParameterDeclarationExpression("IProcessor", "processor"));
                    process.Statements.Add(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeArgumentReferenceExpression("processor"), "ProcessNode"), new CodeThisReferenceExpression()));
                    type.Members.Add(process);

                    var processT = new CodeMemberMethod() { Attributes = MemberAttributes.Public | MemberAttributes.Override, Name = "Process" };
                    processT.TypeParameters.Add(new CodeTypeParameter("T"));
                    processT.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("IProcessor", new CodeTypeReference("T")), "processor"));
                    processT.Statements.Add(new CodeMethodReturnStatement(new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeArgumentReferenceExpression("processor"), "ProcessNode"), new CodeThisReferenceExpression())));
                    processT.ReturnType = new CodeTypeReference("T");
                    type.Members.Add(processT);

                    type.Members.Add(getParameters);

                    @namespace.Types.Add(type);

                    result[nodeType.Guid] = @namespace.Name + "." + nodeType.Name;
                }
                return result;
            }

            private Tuple<CodeNamespace, Func<ID<TConnectorDefinition>, string>> GenerateConnectors(DomainData data, Dictionary<ParameterType, string> basicTypeMap)
            {
                CodeNamespace connectorsNamespace = new CodeNamespace(m_namespace + ".Nodes.Connectors");
                connectorsNamespace.Imports.Add(new CodeNamespaceImport("System"));
                connectorsNamespace.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
                connectorsNamespace.Imports.Add(new CodeNamespaceImport("System.Runtime.InteropServices"));
                connectorsNamespace.Imports.Add(new CodeNamespaceImport(m_namespace + ".Types"));

                {
                    var connector = new CodeTypeDeclaration() { TypeAttributes = TypeAttributes.Abstract | TypeAttributes.Public, IsClass = true, Name = "Connector" };
                    connector.BaseTypes.Add(new CodeTypeReference(typeof(RuntimeConversation.ConnectorBase)));
                    var constructor = new CodeConstructor() { Attributes = MemberAttributes.Public };
                    constructor.Parameters.Add(new CodeParameterDeclarationExpression("Node", "parent"));
                    constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(ID<TConnector>), "id"));
                    constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("id"));
                    constructor.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "m_parent"), new CodeArgumentReferenceExpression("parent")));
                    connector.Members.Add(constructor);


                    var m_parent = new CodeMemberField { Attributes = MemberAttributes.Private, Type = new CodeTypeReference("Node"), Name = "m_parent" };
                    var Parent = new CodeMemberProperty { Attributes = MemberAttributes.Public, Type = new CodeTypeReference("Node"), Name = "Parent", HasGet = true, HasSet = false };
                    Parent.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "m_parent")));
                    connector.Members.Add(m_parent);
                    connector.Members.Add(Parent);

                    var connectorListType = new CodeTypeReference(typeof(List<>));
                    connectorListType.TypeArguments.Add("Connector");
                    var m_connections = new CodeMemberField(connectorListType, "m_connections");
                    m_connections.InitExpression = new CodeObjectCreateExpression(connectorListType);
                    connector.Members.Add(m_connections);
                    var Connections = new CodeMemberProperty() { Attributes = MemberAttributes.Public, Name = "Connections", HasGet = true, HasSet = false, Type = connectorListType };
                    Connections.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "m_connections")));
                    connector.Members.Add(Connections);

                    var ConnectTo = new CodeMemberMethod() { Attributes = MemberAttributes.Public, Name = "ConnectTo" };
                    ConnectTo.Parameters.Add(new CodeParameterDeclarationExpression("Connector", "other"));
                    ConnectTo.Statements.Add(new CodeMethodInvokeExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "m_connections"), "Add", new CodeArgumentReferenceExpression("other")));
                    ConnectTo.Statements.Add(new CodeMethodInvokeExpression(new CodeFieldReferenceExpression(new CodeArgumentReferenceExpression("other"), "m_connections"), "Add", new CodeThisReferenceExpression()));
                    connector.Members.Add(ConnectTo);

                    connectorsNamespace.Types.Add(connector);
                }

                Dictionary<ID<TConnectorDefinition>, string> connectorTypes = new Dictionary<ID<TConnectorDefinition>, string>();

                foreach (var connectorType in data.Connectors)
                {
                    var type = WriteConnectorType(basicTypeMap, connectorType);
                    var name = type.Name;
                    connectorsNamespace.Types.Add(type);
                    connectorTypes.Add(connectorType.Id, connectorsNamespace.Name + "." + name);
                }

                Func<ID<TConnectorDefinition>, string> access = guid => connectorTypes[guid];

                return Tuple.Create(connectorsNamespace, access);
            }

            private Tuple<List<CodeNamespace>, Func<Guid, CodeNamespace>> GenerateCategories(DomainData data)
            {
                Func<string, CodeNamespace> makeNamespace = name =>
                {
                    var result = new CodeNamespace(name);
                    result.Imports.Add(new CodeNamespaceImport(m_namespace + ".Nodes.Connectors"));
                    result.Imports.Add(new CodeNamespaceImport("System"));
                    result.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
                    result.Imports.Add(new CodeNamespaceImport("System.Runtime.InteropServices"));
                    result.Imports.Add(new CodeNamespaceImport(m_namespace + ".Types"));
                    return result;
                };

                CodeNamespace rootNodeNamespace = makeNamespace(m_namespace + ".Nodes");
                List<CodeNamespace> list = new List<CodeNamespace>() { rootNodeNamespace };

                Dictionary<Guid, string> categoryName = data.NodeTypes.ToDictionary(a => a.Guid, a => a.Name);
                Dictionary<Guid, Guid> categoryParent = data.NodeTypes.ToDictionary(a => a.Guid, a => a.Parent);
                Dictionary<Guid, CodeNamespace> categoryNameSpace = new Dictionary<Guid, CodeNamespace>();
                foreach (var category in data.NodeTypes)
                {
                    Func<Guid, string, string> generateNamespace = null;
                    generateNamespace = (guid, theRest) =>
                    {
                        if (!categoryName.ContainsKey(guid))
                            return m_namespace + ".Nodes" + theRest;
                        else
                            return generateNamespace(categoryParent[guid], "." + categoryName[guid] + theRest);
                    };
                    categoryNameSpace[category.Guid] = makeNamespace(generateNamespace(category.Guid, ""));
                    list.Add(categoryNameSpace[category.Guid]);
                }
                return new Tuple<List<CodeNamespace>, Func<Guid, CodeNamespace>>(list, guid => categoryNameSpace.ContainsKey(guid) ? categoryNameSpace[guid] : rootNodeNamespace);
            }

            private Dictionary<ParameterType, string> MakeTypeMap(DomainData data)
            {
                Dictionary<ParameterType, string> basicTypeMap = new Dictionary<ParameterType, string>(m_basicTypeMap);
                foreach (var x in data.Decimals)
                    basicTypeMap[x.TypeID] = x.Name;
                foreach (var x in data.DynamicEnumerations)
                    basicTypeMap[x.TypeID] = x.Name;
                foreach (var x in data.Enumerations)
                {
                    basicTypeMap[x.TypeID] = x.Name;

                    var name = ReadonlySetOf(x.Name);
                    basicTypeMap[ParameterType.Set.Of(x.TypeID)] = name;
                }
                foreach (var x in data.Integers)
                    basicTypeMap[x.TypeID] = x.Name;
                return basicTypeMap;
            }

            private static string ReadonlySetOf(string type)
            {
                CodeTypeReference c = new CodeTypeReference(typeof(ReadonlySet<>));
                c.TypeArguments.Add(type);
                var name = generator.GetTypeOutput(c);
                return name;
            }

            public static string ListOf(string type)
            {
                CodeTypeReference c = new CodeTypeReference(typeof(List<>));
                c.TypeArguments.Add(type);
                var name = generator.GetTypeOutput(c);
                return name;
            }

            private CodeMemberField GenerateParameter(Dictionary<ParameterType, string> basicTypeMap, NodeData.ParameterData parameterData)
            {
                var name = parameterData.Name;

                var typeName = basicTypeMap[parameterData.Type];
                CodeTypeReference type = new CodeTypeReference(typeName);

                //var @default = new CodeSnippetExpression(parameterData.Default); //No need to provide a default as the conversation should specify a value for every node

                CodeMemberField result = new CodeMemberField(type, name) { Attributes = MemberAttributes.Public };
                result.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(RuntimeConversation.ParameterIDAttribute)), new CodeAttributeArgument(new CodePrimitiveExpression(parameterData.Id.Guid.ToString()))));

                return result;
            }

            private CodeTypeDeclaration WriteConnectorType(Dictionary<ParameterType, string> basicTypeMap, ConnectorDefinitionData connectorType)
            {
                CodeTypeDeclaration type = new CodeTypeDeclaration(connectorType.Name) { IsClass = true };
                type.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(RuntimeConversation.ConnectorTypeIDAttribute)), new CodeAttributeArgument(new CodePrimitiveExpression(connectorType.Id.Guid.ToString()))));

                type.BaseTypes.Add(new CodeTypeReference("Connector"));

                //type.Members.Add(new CodeMemberField(typeof(ID<TConnector>), "m_id"));

                var constructor = new CodeConstructor() { Attributes = MemberAttributes.Public };
                constructor.Parameters.Add(new CodeParameterDeclarationExpression("Node", "parent"));
                constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(ID<TConnector>), "id"));
                constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("parent"));
                constructor.BaseConstructorArgs.Add(new CodeArgumentReferenceExpression("id"));
                foreach (var parameter in connectorType.Parameters)
                {
                    var pname = parameter.Name;
                    constructor.Parameters.Add(new CodeParameterDeclarationExpression(basicTypeMap[parameter.Type], pname));
                    constructor.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), pname), new CodeArgumentReferenceExpression(pname)));
                }
                type.Members.Add(constructor);

                //Implement the ID getter: public abstract ID<TConnector> ID { get; }
                //var id = new CodeMemberProperty() { Attributes = MemberAttributes.Public | MemberAttributes.Override, HasGet = true, Type = new CodeTypeReference(typeof(ID<TConnector>)), Name = "ID" };
                //id.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, "m_id")));
                //type.Members.Add(id);

                //connectorType.Position can be ignored because it's only relevant to the editor

                var parameterFields = connectorType.Parameters.Select(d => GenerateParameter(basicTypeMap, d)).ToArray();

                type.Members.AddRange(parameterFields);

                return type;
            }

            private CodeNamespace GenerateTypes(DomainData data)
            {
                CodeNamespace typesNamespace = new CodeNamespace(m_namespace + ".Types");
                typesNamespace.Imports.Add(new CodeNamespaceImport("System"));
                typesNamespace.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
                typesNamespace.Imports.Add(new CodeNamespaceImport("System.Runtime.InteropServices"));

                CodeTypeDeclaration TypeDeserializer = new CodeTypeDeclaration("TypeDeserializer") { TypeAttributes = TypeAttributes.Public };
                TypeDeserializer.BaseTypes.Add(new CodeTypeReference(typeof(RuntimeConversation.TypeDeserializerBase)));
                TypeDeserializer.IsClass = true;

                foreach (var enumeration in data.Enumerations)
                {
                    typesNamespace.Types.Add(GenerateEnum(enumeration));

                    //The enum
                    {
                        var name = enumeration.Name;
                        var deserializer = new CodeMemberMethod() { Attributes = MemberAttributes.Public | MemberAttributes.Static, Name = "Deserialize" };
                        deserializer.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(name), "a") { Direction = FieldDirection.Out });
                        deserializer.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(string)), "value"));
                        foreach (var val in enumeration.Elements)
                        {
                            var valName = val.Name;
                            deserializer.Statements.Add(new CodeConditionStatement(new CodeMethodInvokeExpression(new CodePrimitiveExpression(val.Guid.ToString()), "Equals", new CodeArgumentReferenceExpression("value")),
                                                                                   new CodeAssignStatement(new CodeArgumentReferenceExpression("a"),
                                                                                                           new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(name), valName)),
                                                                                                           new CodeMethodReturnStatement()));
                        }
                        deserializer.Statements.Add(new CodeAssignStatement(new CodeArgumentReferenceExpression("a"), new CodePrimitiveExpression(0)));
                        TypeDeserializer.Members.Add(deserializer);
                    }

                    //Set of the enum
                    {
                        var deserializer = new CodeMemberMethod() { Attributes = MemberAttributes.Public | MemberAttributes.Static, Name = "Deserialize" };
                        deserializer.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(ReadonlySetOf(enumeration.Name)), "a") { Direction = FieldDirection.Out });
                        deserializer.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(string)), "value"));
                        //string[] values;
                        deserializer.Statements.Add(new CodeVariableDeclarationStatement(typeof(string[]), "values"));
                        //values = value.Split('+');
                        deserializer.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("values"), new CodeMethodInvokeExpression(new CodeArgumentReferenceExpression("value"), "Split", new CodePrimitiveExpression('+'))));
                        //List<T> result = new List<T>();
                        deserializer.Statements.Add(new CodeVariableDeclarationStatement(ListOf(enumeration.Name), "result", new CodeObjectCreateExpression(ListOf(enumeration.Name))));

                        //for (int i = 0; i < values.Length; i++)
                        var forloop = new CodeIterationStatement(new CodeVariableDeclarationStatement(typeof(int), "i", new CodePrimitiveExpression(0)),
                                                                 new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("i"), CodeBinaryOperatorType.LessThan, new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("values"), "Length")),
                                                                 new CodeAssignStatement(new CodeVariableReferenceExpression("i"), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("i"), CodeBinaryOperatorType.Add, new CodePrimitiveExpression(1))));
                        deserializer.Statements.Add(forloop);
                        //{
                        //string s = values[i];
                        forloop.Statements.Add(new CodeVariableDeclarationStatement(typeof(string), "s", new CodeIndexerExpression(new CodeVariableReferenceExpression("values"), new CodeVariableReferenceExpression("i"))));
                        //if (s.Length > 0)
                        var conditionalAdd = new CodeConditionStatement(new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("s"), "Length"), CodeBinaryOperatorType.GreaterThan, new CodePrimitiveExpression(0)));
                        forloop.Statements.Add(conditionalAdd);
                        //T v;
                        conditionalAdd.TrueStatements.Add(new CodeVariableDeclarationStatement(enumeration.Name, "v"));
                        //Deserialize(out v, s);
                        conditionalAdd.TrueStatements.Add(new CodeMethodInvokeExpression(null, "Deserialize", new CodeDirectionExpression(FieldDirection.Out, new CodeVariableReferenceExpression("v")), new CodeVariableReferenceExpression("s")));
                        //result.Add(v);
                        conditionalAdd.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("result"), "Add", new CodeVariableReferenceExpression("v")));
                        //}

                        //a = new Utilities.ReadonlySet<T>(result);
                        deserializer.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("a"), new CodeObjectCreateExpression(ReadonlySetOf(enumeration.Name), new CodeVariableReferenceExpression("result"))));

                        TypeDeserializer.Members.Add(deserializer);
                    }
                }
                foreach (var integer in data.Integers)
                {
                    typesNamespace.Types.Add(GenerateNumeric(integer.Name, integer.TypeID.Guid, integer.Max ?? int.MaxValue, integer.Min ?? int.MinValue));
                    var name = integer.Name;

                    var deserializer = new CodeMemberMethod() { Attributes = MemberAttributes.Public | MemberAttributes.Static, Name = "Deserialize" };
                    deserializer.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(name), "a") { Direction = FieldDirection.Out });
                    deserializer.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(string)), "value"));
                    deserializer.Statements.Add(new CodeAssignStatement(new CodeArgumentReferenceExpression("a"),
                                                                        new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(name), "FromValue",
                                                                        new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(int)), "Parse", new CodeArgumentReferenceExpression("value")))));
                    TypeDeserializer.Members.Add(deserializer);
                }
                foreach (var @decimal in data.Decimals)
                {
                    typesNamespace.Types.Add(GenerateNumeric(@decimal.Name, @decimal.TypeID.Guid, @decimal.Max ?? decimal.MaxValue, @decimal.Min ?? decimal.MinValue));
                    var name = @decimal.Name;

                    var deserializer = new CodeMemberMethod() { Attributes = MemberAttributes.Public | MemberAttributes.Static, Name = "Deserialize" };
                    deserializer.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(name), "a") { Direction = FieldDirection.Out });
                    deserializer.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(string)), "value"));
                    deserializer.Statements.Add(new CodeAssignStatement(new CodeArgumentReferenceExpression("a"),
                                                                        new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(name), "FromValue",
                                                                        new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(decimal)), "Parse", new CodeArgumentReferenceExpression("value"), new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(CultureInfo)), "InvariantCulture")))));
                    TypeDeserializer.Members.Add(deserializer);
                }
                foreach (var dynamicEnum in data.DynamicEnumerations)
                {
                    typesNamespace.Types.Add(GenerateDynamicEnumeration(dynamicEnum));
                    var name = dynamicEnum.Name;

                    var deserializer = new CodeMemberMethod() { Attributes = MemberAttributes.Public | MemberAttributes.Static, Name = "Deserialize" };
                    deserializer.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(name), "a") { Direction = FieldDirection.Out });
                    deserializer.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(string)), "value"));
                    deserializer.Statements.Add(new CodeAssignStatement(new CodeArgumentReferenceExpression("a"),
                                                                        new CodeCastExpression(name, new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("Enum"), "Parse", new CodeTypeOfExpression(new CodeTypeReference(name)), new CodeArgumentReferenceExpression("value")))));
                    TypeDeserializer.Members.Add(deserializer);
                }

                typesNamespace.Types.Add(TypeDeserializer);
                return typesNamespace;
            }

            private static void WriteToStream(System.IO.Stream stream, CodeCompileUnit file)
            {
                CodeGeneratorOptions options = new CodeGeneratorOptions() { BracingStyle = "C" };
                TextWriter t = new StreamWriter(stream);
                generator.GenerateCodeFromCompileUnit(file, t, options);
                t.Flush(); //Never close/dispose t because that would close the underlying stream
            }

            private static CodeTypeDeclaration GenerateEnum(EnumerationData enumeration)
            {
                var name = enumeration.Name;
                CodeTypeDeclaration type = new CodeTypeDeclaration(name) { IsEnum = true };
                type.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(RuntimeConversation.TypeIDAttribute)), new CodeAttributeArgument(new CodePrimitiveExpression(enumeration.TypeID.Guid.ToString()))));

                foreach (var element in enumeration.Elements)
                {
                    var elementName = element.Name;
                    CodeMemberField f = new CodeMemberField(name, elementName);
                    f.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(RuntimeConversation.EnumValueIDAttribute)), new CodeAttributeArgument(new CodePrimitiveExpression(element.Guid.ToString()))));
                    type.Members.Add(f);
                }

                return type;
            }

            private CodeTypeDeclaration GenerateNumeric<T>(string name, Guid guid, T max, T min)
            {
                CodeTypeDeclaration result = new CodeTypeDeclaration(name) { IsStruct = true };
                result.CustomAttributes.Add(new CodeAttributeDeclaration("RuntimeConversation.TypeIDAttribute", new CodeAttributeArgument(new CodePrimitiveExpression(guid.ToString()))));

                result.Members.Add(new CodeMemberField(typeof(T), "Max") { Attributes = MemberAttributes.Const, InitExpression = new CodePrimitiveExpression(max) });
                result.Members.Add(new CodeMemberField(typeof(T), "Min") { Attributes = MemberAttributes.Const, InitExpression = new CodePrimitiveExpression(min) });
                result.Members.Add(new CodeMemberField(typeof(T), "m_value") { Attributes = MemberAttributes.Private });

                var maxField = new CodeFieldReferenceExpression(null, "Max");
                var minField = new CodeFieldReferenceExpression(null, "Min");
                var value = new CodeArgumentReferenceExpression("value");
                var m_value = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "m_value");

                var constructor = new CodeConstructor();
                constructor.Parameters.Add(new CodeParameterDeclarationExpression(typeof(T), "value"));
                constructor.Statements.Add(new CodeAssignStatement(m_value, value));
                result.Members.Add(constructor);

                var inRange = new CodeMemberMethod() { Name = "InRange", Attributes = MemberAttributes.Public | MemberAttributes.Static, ReturnType = new CodeTypeReference(typeof(bool)) };
                inRange.Parameters.Add(new CodeParameterDeclarationExpression(typeof(T), "value"));
                inRange.Statements.Add(new CodeMethodReturnStatement(new CodeBinaryOperatorExpression(
                    new CodeBinaryOperatorExpression(value, CodeBinaryOperatorType.LessThanOrEqual, maxField),
                    CodeBinaryOperatorType.BooleanAnd,
                    new CodeBinaryOperatorExpression(value, CodeBinaryOperatorType.GreaterThanOrEqual, minField))));
                result.Members.Add(inRange);

                var fromValueClamped = new CodeMemberMethod() { Name = "FromValueClamped", Attributes = MemberAttributes.Public | MemberAttributes.Static, ReturnType = new CodeTypeReference(name) };
                fromValueClamped.Parameters.Add(new CodeParameterDeclarationExpression(typeof(T), "value"));
                fromValueClamped.Statements.Add(new CodeConditionStatement(new CodeBinaryOperatorExpression(value, CodeBinaryOperatorType.GreaterThan, maxField),
                                                                          new CodeMethodReturnStatement(new CodeObjectCreateExpression(name, maxField))));
                fromValueClamped.Statements.Add(new CodeConditionStatement(new CodeBinaryOperatorExpression(value, CodeBinaryOperatorType.LessThan, minField),
                                                                          new CodeMethodReturnStatement(new CodeObjectCreateExpression(name, minField))));
                fromValueClamped.Statements.Add(new CodeMethodReturnStatement(new CodeObjectCreateExpression(name, value)));
                result.Members.Add(fromValueClamped);

                var fromValue = new CodeMemberMethod() { Name = "FromValue", Attributes = MemberAttributes.Public | MemberAttributes.Static, ReturnType = new CodeTypeReference(name) };
                fromValue.Parameters.Add(new CodeParameterDeclarationExpression(typeof(T), "value"));
                fromValue.Statements.Add(new CodeConditionStatement(new CodeBinaryOperatorExpression(value, CodeBinaryOperatorType.GreaterThan, maxField),
                                                          new CodeThrowExceptionStatement(new CodeObjectCreateExpression(typeof(ArgumentOutOfRangeException), new CodePrimitiveExpression("value cannot be greater than Max in FromValue. Consider using FromValueClamped if clamping is desired")))));
                fromValue.Statements.Add(new CodeConditionStatement(new CodeBinaryOperatorExpression(value, CodeBinaryOperatorType.LessThan, minField),
                                                          new CodeThrowExceptionStatement(new CodeObjectCreateExpression(typeof(ArgumentOutOfRangeException), new CodePrimitiveExpression("value cannot be less than Max in FromValue. Consider using FromValueClamped if clamping is desired")))));
                fromValue.Statements.Add(new CodeMethodReturnStatement(new CodeObjectCreateExpression(name, value)));
                result.Members.Add(fromValue);

                result.Members.Add(new CodeSnippetTypeMember("public static implicit operator " + typeof(T).ToString() + "(" + name + " value) { return value.m_value; }"));

                return result;
            }

            private static CodeTypeDeclaration GenerateDynamicEnumeration(DynamicEnumerationData dynamicEnum)
            {
                var name = dynamicEnum.Name;
                CodeTypeDeclaration type = new CodeTypeDeclaration(name) { IsEnum = true };
                type.CustomAttributes.Add(new CodeAttributeDeclaration("RuntimeConversation.TypeIDAttribute", new CodeAttributeArgument(new CodePrimitiveExpression(dynamicEnum.TypeID.Guid.ToString()))));

                foreach (var element in dynamicEnum.Options)
                {
                    var elementName = element;
                    CodeMemberField f = new CodeMemberField(name, elementName);
                    type.Members.Add(f);
                }

                return type;
            }
        }
    }
}
