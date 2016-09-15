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
            private ConversationNode<INodeGui> m_node;
            private Func<Id<LocalizedText>, string> m_localize;

            public LogElement(IConversationEditorControlData<ConversationNode<INodeGui>, TransitionNoduleUIInfo> file, ConversationNode<INodeGui> node, Func<Id<LocalizedText>, string> localize)
            {
                File = file;
                m_node = node;
                m_localize = localize;
            }

            public IConversationEditorControlData<ConversationNode<INodeGui>, TransitionNoduleUIInfo> File { get; }

            public string Message
            {
                get
                {
                    return m_node.NodeName + ":  " + string.Join(", ", m_node.Parameters.Select(p => p.Name + ": " + p.DisplayValue(m_localize)).ToArray());
                }
            }

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

        public IEnumerable<MenuAction<ConversationNode<INodeGui>>> GetMenuActions(IGraphEditorControl<ConversationNode<INodeGui>> control, IProject2 project, Action<IEnumerable<IErrorListElement>> log, Func<Id<LocalizedText>, string> localize)
        {
            //XmlGraphData<NodeUIData, ConversationEditorData> data;
            //Assembly assembly = Assembly.GetExecutingAssembly();
            //using (Stream stream = assembly.GetManifestResourceStream("PluginPack.Templates.Basic Conversation.xml"))
            //{
            //    data = SerializationUtils.ConversationDeserializer(control.DataSource).Read(stream);
            //}
            yield return new MenuAction<ConversationNode>("Find Nodes of Type", (a, b) => () =>
            {
                var nodesofType = project.ConversationFilesCollection.SelectMany(f => f.Nodes.Where(n => n.Type == a.Type).Select(n => new { Node = n, File = f }));
                log(nodesofType.Select(n => new LogElement(n.File, n.Node, localize)));
            }
            , null, null, null);
        }
    }
}
