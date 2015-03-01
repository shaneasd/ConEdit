﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Utilities;
using System.Reflection;
using System.Windows;
using Conversation;

namespace ConversationEditor
{
    public class PluginsConfig : IConfigParameter
    {
        private List<PluginAssembly> m_filteredAssemblies = new List<PluginAssembly>();
        public IEnumerable<PluginAssembly> FilteredAssemblies { get { return m_filteredAssemblies; } }
        public IEnumerable<PluginAssembly> UnfilteredAssemblies { get { return PluginSelector.AllPlugins.Except(FilteredAssemblies); } }
        public IEnumerable<NodeRendererChoice> NodeRenderers
        {
            get
            {
                var assemblies = new[] { Assembly.GetAssembly(typeof(EditableUI)), Assembly.GetAssembly(typeof(DomainNodeRenderer)) }.Concat(UnfilteredAssemblies.Select(a => a.Assembly));

                foreach (Assembly assembly in assemblies)
                {
                    var types = assembly.GetExportedTypes();
                    foreach (Type type in types)
                    {
                        if (type.IsSubclassOf(typeof(NodeUI.IFactory)))
                        {
                            yield return new NodeRendererChoice((NodeUI.IFactory)type.GetConstructor(new Type[0]).Invoke(new object[0]));
                        }
                    }
                }
            }
        }

        public IEnumerable<NodeEditorChoice> NodeEditors
        {
            get
            {
                var assemblies = Assembly.GetAssembly(typeof(DefaultNodeEditorFactory)).Only().Concat(UnfilteredAssemblies.Select(a => a.Assembly));
                foreach (Assembly assembly in assemblies)
                {
                    var types = assembly.GetExportedTypes();
                    foreach (Type type in types)
                    {
                        if (type.IsSubclassOf(typeof(NodeEditorFactory)))
                            yield return new NodeEditorChoice(type);
                    }
                }
            }
        }

        public IEnumerable<NodeRendererChoice> GetRenderersFor(ID<NodeTypeTemp> nodeType)
        {
            return NodeRenderers.Where(c => c.WillRender(nodeType));
        }

        public IEnumerable<NodeEditorChoice> GetEditorsFor(ID<NodeTypeTemp> nodeType)
        {
            return NodeEditors.Where(c => c.WillEdit(nodeType));
        }

        public IEnumerable<IConfigNodeDefinition> GetConfigDefinitions()
        {
            foreach (var assembly in Assembly.GetAssembly(typeof(ConfigNodeDefinitions)).Only().Concat(UnfilteredAssemblies.Select(a => a.Assembly)))
            {
                var factories = assembly.GetExportedTypes().Where(t => t.GetInterfaces().Contains(typeof(IConfigNodeDefinitionFactory)));
                foreach (var factoryType in factories)
                {
                    var constructor = factoryType.GetConstructor(Type.EmptyTypes);
                    var factory = constructor.Invoke(new object [0]) as IConfigNodeDefinitionFactory;
                    foreach (var configNodeDefinition in factory.ConfigNodeDefinitions())
                        yield return configNodeDefinition;
                }
            }
        }

        public void Load(XElement root)
        {
            m_filteredAssemblies = new List<PluginAssembly>();
            var node = root.Element("Plugins");
            if (node != null)
            {
                var assemblyNodes = node.Elements("Assembly");
                foreach (var assemblyNode in assemblyNodes)
                {
                    var filename = assemblyNode.Attribute("filename").Value;
                    try
                    {
                        var assembly = new PluginAssembly(filename);
                        m_filteredAssemblies.Add(assembly);
                    }
                    catch
                    {
                        MessageBox.Show("Failed to load assembly '" + filename + "'. This assembly will not be saved in the config file.");
                    }
                }
            }
        }

        public void Write(XElement root)
        {
            var node = new XElement("Plugins");
            root.Add(node);
            foreach (var assembly in m_filteredAssemblies)
            {
                var assemblyNode = new XElement("Assembly", new XAttribute("filename", assembly.FileName));
                node.Add(assemblyNode);
            }
        }

        public event Action ValueChanged;

        internal void SetFilteredAssemblies(List<PluginAssembly> m_plugins)
        {
            m_filteredAssemblies.Clear();
            m_filteredAssemblies.AddRange(m_plugins);
            ValueChanged.Execute();
        }
    }
}
