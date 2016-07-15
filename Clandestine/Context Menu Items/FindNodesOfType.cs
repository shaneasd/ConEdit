using ConversationEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGui>;
using Conversation;
using Utilities;

namespace Clandestine
{
    //TODO: distinguish different types of context menu in folder structure
    public class FindNodesOfType : IMenuActionFactory<ConversationNode>
    {
        private class LogElement : IErrorListElement
        {
            public LogElement(IConversationEditorControlData<ConversationNode<INodeGui>, TransitionNoduleUIInfo> file, ConversationNode<INodeGui> node)
            {
                File = file;
                m_node = node;
            }

            public IConversationEditorControlData<ConversationNode<INodeGui>, TransitionNoduleUIInfo> File
            {
                get; private set;
            }

            public string Message
            {
                get
                {
                    return m_node.NodeName + ":  " + string.Join(", ", m_node.Parameters.Select(p => p.Name + ": " + p.ValueAsString()).ToArray());
                }
            }

            ConversationNode<INodeGui> m_node;
            public IEnumerable<ConversationNode<INodeGui>> Nodes
            {
                get
                {
                    return m_node.Only();
                }
            }

            public IEnumerator<Tuple<ConversationNode<INodeGui>, IConversationEditorControlData<ConversationNode<INodeGui>, TransitionNoduleUIInfo>>> MakeEnumerator()
            {
                if (Nodes.Any())
                    return Nodes.Select(n => new Tuple<ConversationNode, IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo>>(n, File)).InfiniteRepeat().GetEnumerator();
                else
                    return null;
            }
        }

        public IEnumerable<MenuAction<ConversationNode<INodeGui>>> GetMenuActions(IGraphEditorControl<ConversationNode<INodeGui>> control, IProject2 project, Action<IEnumerable<IErrorListElement>> log)
        {
            //XmlGraphData<NodeUIData, ConversationEditorData> data;
            //Assembly assembly = Assembly.GetExecutingAssembly();
            //using (Stream stream = assembly.GetManifestResourceStream("PluginPack.Templates.Basic Conversation.xml"))
            //{
            //    data = SerializationUtils.ConversationDeserializer(control.DataSource).Read(stream);
            //}

            yield return new MenuAction<ConversationNode>("Find Nodes of Type", (a, b) => () =>
            {
                var nodesofType = project.ConversationFilesCollection.SelectMany(f => f.Nodes.Where(n => n.Type == a.Type));
                log(nodesofType.Select(n => new LogElement(control.CurrentFile, n)));
            }
            , null, null, null);
        }
    }
}
