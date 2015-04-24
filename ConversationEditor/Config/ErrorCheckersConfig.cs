using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Utilities;
using System.Windows;

namespace ConversationEditor
{
    internal class ErrorCheckersConfig : IConfigParameter
    {
        private List<ErrorCheckerAssembly> m_assemblies = new List<ErrorCheckerAssembly>();
        public IEnumerable<ErrorCheckerAssembly> Assemblies { get { return m_assemblies; } }

        public void Load(XElement root)
        {
            m_assemblies = new List<ErrorCheckerAssembly>();
            var node = root.Element("ErrorCheckers");
            if (node != null)
            {
                var assemblyNodes = node.Elements("Assembly");
                foreach (var assemblyNode in assemblyNodes)
                {
                    var filename = assemblyNode.Attribute("filename").Value;
                    try
                    {
                        var assembly = new ErrorCheckerAssembly(filename);
                        m_assemblies.Add(assembly);

                        foreach (var checker in assemblyNode.Elements("Checker"))
                        {
                            assembly.SetEnabled(checker.Attribute("type").Value, bool.Parse(checker.Attribute("enabled").Value));
                        }
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
            var node = new XElement("ErrorCheckers");
            root.Add(node);
            foreach (var assembly in m_assemblies)
            {
                XElement assemblyNode = assembly.m_assembly.FileName == null ?
                                        new XElement("ExecutingAssembly") :
                                        new XElement("Assembly", new XAttribute("filename", assembly.m_assembly.FileName));
                node.Add(assemblyNode);

                foreach (var checker in assembly.Types)
                    assemblyNode.Add(new XElement("Checker", new XAttribute("type", checker.SerializeName), new XAttribute("enabled", checker.Enabled)));
            }
        }

        public event Action ValueChanged;

        public void AddAssembly(ErrorCheckerAssembly assembly)
        {
            m_assemblies.Add(assembly);
        }

        internal void Save()
        {
            ValueChanged.Execute();
        }

        internal ErrorCheckerAssembly GetAssembly(PluginAssembly pluginAssembly)
        {
            return m_assemblies.SingleOrDefault(eca => eca.m_assembly.Equals(pluginAssembly));
        }

        public void RefreshConfig(PluginsConfig pluginsConfig)
        {
            foreach (var pluginAssembly in pluginsConfig.UnfilteredAssemblies(MainAssemblies.Include))
            {
                ErrorCheckerAssembly configECA = GetAssembly(pluginAssembly);

                if (configECA == null)
                {
                    configECA = new ErrorCheckerAssembly(pluginAssembly);
                    AddAssembly(configECA);
                }
            }
            Save();
        }
    }
}
