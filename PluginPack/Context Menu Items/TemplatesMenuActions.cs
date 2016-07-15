using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGui>;
using System.Reflection;
using System.IO;
using ConversationEditor;
using Conversation.Serialization;

namespace PluginPack
{
    class TemplatesMenuActions : IMenuActionFactory<ConversationNode>
    {
        public IEnumerable<MenuAction<ConversationNode>> GetMenuActions(IGraphEditorControl<ConversationNode> control, IProject2 project, Action<IEnumerable<IErrorListElement>> log)
        {
            XmlGraphData<NodeUIData, ConversationEditorData> data;
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("PluginPack.Templates.Basic Conversation.xml"))
            {
                data = SerializationUtils.ConversationDeserializer(control.DataSource).Read(stream);
            }

            yield return new MenuAction<ConversationNode>("Basic Conversation", (a, b) => null, null, null, p =>
            {
                control.Insert(p, Tuple.Create(data.Nodes, data.EditorData.Groups, new object()));
            });
        }
    }
}
