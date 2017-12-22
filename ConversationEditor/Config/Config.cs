using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Xml;
using System.Windows.Forms;
using Utilities;
using Conversation;
using System.Reflection;

namespace ConversationEditor
{
    [Serializable()]
    public class ConfigLoadFailedException : Exception
    {
        public ConfigLoadFailedException()
            : base("Failed to load config")
        {
        }

        public ConfigLoadFailedException(string message)
            : base(message)
        {
        }
        public ConfigLoadFailedException(string message, Exception innerException) :
            base(message, innerException)
        {
        }
        protected ConfigLoadFailedException(System.Runtime.Serialization.SerializationInfo info,
           System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }

    internal class Config : Disposable
    {
        string m_file;
        public Config(string file, WillEdit willEdit)
        {
            m_file = file;
            ParameterEditors = new MapConfig<ParameterType, Guid>("ParameterEditors", kvp => new KeyValuePair<string, string>(kvp.Key.Serialized(), kvp.Value.ToString()),
                                                                      kvp => new KeyValuePair<ParameterType, Guid>(ParameterType.Parse(kvp.Key), Guid.Parse(kvp.Value)),
                                                                      a => ParameterEditorCustomization.DefaultEditor(a, willEdit));
            ConversationNodeRenderers = new MapConfig<Id<NodeTypeTemp>, Guid>("ConversationNodeRenderers", kvp => new KeyValuePair<string, string>(kvp.Key.Serialized(), kvp.Value.ToString()),
                                                                      kvp => new KeyValuePair<Id<NodeTypeTemp>, Guid>(Id<NodeTypeTemp>.Parse(kvp.Key), Guid.Parse(kvp.Value)),
                                                                      a => EditableUIFactory.Instance.Guid);
            InitParameters();
            LoadRoot(file);
        }

        private void InitParameters()
        {
            m_parameters.Add(ParameterEditors);
            m_parameters.Add(ErrorCheckers);
            m_parameters.Add(GraphView);
            m_parameters.Add(ConversationNodeRenderers);
            m_parameters.Add(DomainNodeRenderers);
            m_parameters.Add(ProjectNodeRenderers);
            m_parameters.Add(NodeEditors);
            m_parameters.Add(Plugins);
            m_parameters.Add(ProjectHistory);
            m_parameters.Add(ExportPath);
            m_parameters.Add(ColorScheme);
            m_parameters.Add(AudioCustomization);
            m_parameters.Add(FileFilters);

            foreach (var p in m_parameters)
                p.ValueChanged += Write;
        }

        public List<IConfigParameter> m_parameters = new List<IConfigParameter>();
        public MapConfig<ParameterType, Guid> ParameterEditors { get; }
        public MapConfig<Id<NodeTypeTemp>, Guid> ConversationNodeRenderers { get; }
        public ErrorCheckersConfig ErrorCheckers { get; } = new ErrorCheckersConfig();
        public GraphViewConfig GraphView { get; } = new GraphViewConfig();
        //public readonly TypeMapConfig<ID<NodeTypeTemp>, NodeRendererChoice> ConversationNodeRenderers = new TypeMapConfig<ID<NodeTypeTemp>, NodeRendererChoice>("NodeRenderers", nodeType => nodeType.Serialized(), (a, t) => new NodeRendererChoice(a, t), nodeType => NodeRendererChoice.DefaultConversation(nodeType));
        public TypeMapConfig<Id<NodeTypeTemp>, NodeRendererChoice> DomainNodeRenderers { get; } = new TypeMapConfig<Id<NodeTypeTemp>, NodeRendererChoice>("DomainNodeRenderers", nodeType => nodeType.Serialized(), (a, t) => new NodeRendererChoice(a, t), nodeType => NodeRendererChoice.DefaultDomain(nodeType));
        public TypeMapConfig<Id<NodeTypeTemp>, NodeRendererChoice> ProjectNodeRenderers { get; } = new TypeMapConfig<Id<NodeTypeTemp>, NodeRendererChoice>("ProjectNodeRenderers", nodeType => nodeType.Serialized(), (a, t) => new NodeRendererChoice(a, t), nodeType => NodeRendererChoice.DefaultDomain(nodeType));
        public TypeMapConfig<Id<NodeTypeTemp>, NodeEditorChoice> NodeEditors { get; } = new TypeMapConfig<Id<NodeTypeTemp>, NodeEditorChoice>("NodeEditors", nodeType => nodeType.Serialized(), (a, t) => new NodeEditorChoice(a, t), nodeType => NodeEditorChoice.Default(nodeType));
        public PluginsConfig Plugins { get; } = new PluginsConfig();
        public ConfigParameterList<string> ProjectHistory { get; } = new ConfigParameterList<string>("ProjectHistory", () => new JustStringConfigParameter());
        public ConfigParameterString ExportPath { get; } = new ConfigParameterString("ExportPath");
        public ColorsConfig ColorScheme { get; } = new ColorsConfig();
        public ConfigParameterString AudioCustomization { get; } = new ConfigParameterString("AudioCustomization");
        public FileFilterConfig FileFilters { get; } = new FileFilterConfig();

        string TryLoad(string file)
        {
            try
            {
                //Try to load the file and if its missing just create a new one and return
                XDocument doc = null;
                try
                {
                    doc = XDocument.Load(file);
                }
                catch (FileNotFoundException)
                {
                    this.Write();
                    return null;
                }

                var root = doc.Element(ROOT);
                if (root == null)
                {
                    return "Config xml root element missing";
                }
                else
                {
                    var version = root.Attribute("xmlversion");
                    if (version == null || version.Value != XML_VERSION)
                        return "Unrecognised config xml version";
                    else
                    {
                        foreach (var p in m_parameters)
                            p.Load(root);
                        return null;
                    }
                }
            }
            catch
            {
                throw;
                //return "Error reading config xml";
            }
        }

        void LoadRoot(string file)
        {
            string error = TryLoad(file);
            if (error != null)
            {
                var respose = MessageBox.Show(error + ". Would you like to delete config.xml and start over? If you click no the application will close and you must manually fix your config.", "Config error", MessageBoxButtons.YesNo);
                if (respose == DialogResult.Yes)
                {
                    File.Delete(file);
                    TryLoad(file);
                }
                else
                {
                    throw new ConfigLoadFailedException();
                }
            }
        }

        public const string XML_VERSION = "1.0";
        public const string ROOT = "Root";

        private void Write()
        {
            XDocument doc = new XDocument();
            var root = new XElement(ROOT, new XAttribute("xmlversion", XML_VERSION));
            doc.Add(root);

            foreach (var p in m_parameters)
                p.Write(root);

            if (File.Exists(m_file))
                File.Delete(m_file);
            using (XmlTextWriter writer = new XmlTextWriter(m_file, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 4;
                doc.WriteTo(writer);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "<ProjectHistory>k__BackingField",
            Justification = "https://connect.microsoft.com/VisualStudio/feedback/details/1798055/code-analysis-rule-ca2213-disposablefieldsshouldbedisposed-doesnt-understand-read-only-auto-properties-being-disposed")]
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ParameterEditors.Dispose();
                ProjectHistory.Dispose();
                ConversationNodeRenderers.Dispose();
            }
        }
    }
}