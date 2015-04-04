using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConversationEditor.Controllers;
using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGUI>;
using System.Reflection;
using System.IO;
using ConversationEditor;
using Conversation.Serialization;

namespace PluginPack
{
    class TemplatesMenuActions : IMenuActionFactory<ConversationNode>
    {
        public IEnumerable<MenuAction2<ConversationNode>> GetMenuActions(ConversationEditor.GraphEditorControl<ConversationNode> control)
        {
            XmlGraphData<NodeUIData, ConversationEditorData> data;
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("PluginPack.Templates.Basic Conversation.xml"))
            {
                data = SerializationUtils.ConversationDeserializer(control.DataSource).Read(stream);
            }

            yield return new MenuAction2<ConversationNode>("Basic Conversation", (a, b) => null, null, null, p =>
            {
                control.Insert(p, Tuple.Create(data.Nodes, data.EditorData.Groups));
            });
        }
    }
}
