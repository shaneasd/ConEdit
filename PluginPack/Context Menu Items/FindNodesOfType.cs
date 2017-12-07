using ConversationEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGui>;
using Conversation;
using Utilities;

namespace PluginPack
{
    //TODO: distinguish different types of context menu in folder structure
    public class FindNodesOfType : IMenuActionFactory<ConversationNode>
    {
        private class LogElement : IErrorListElement
        {
            private ConversationNode<INodeGui> m_node;
            private Func<Id<LocalizedStringType>, Id<LocalizedText>, string> m_localize;

            public LogElement(IConversationEditorControlData<ConversationNode<INodeGui>, TransitionNoduleUIInfo> file, ConversationNode<INodeGui> node, ILocalizationEngine localizer)
            {
                File = file;
                m_node = node;
                m_localize = localizer.Localize;
            }

            public IConversationEditorControlData<ConversationNode<INodeGui>, TransitionNoduleUIInfo> File { get; }

            public string Message
            {
                get
                {
                    return m_node.Data.Name + ":  " + string.Join(", ", m_node.Data.Parameters.Select(p => p.Name + ": " + p.DisplayValue(m_localize)).ToArray());
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

        public IEnumerable<MenuAction<ConversationNode<INodeGui>>> GetMenuActions(IGraphEditorControl<ConversationNode<INodeGui>> control, IProject2 project, Action<IEnumerable<IErrorListElement>> log, ILocalizationEngine localizer)
        {
            //XmlGraphData<NodeUIData, ConversationEditorData> data;
            //Assembly assembly = Assembly.GetExecutingAssembly();
            //using (Stream stream = assembly.GetManifestResourceStream("PluginPack.Templates.Basic Conversation.xml"))
            //{
            //    data = SerializationUtils.ConversationDeserializer(control.DataSource).Read(stream);
            //}
            yield return new MenuAction<ConversationNode>("Find Nodes of Type", (a, b) => () =>
            {
                var nodesofType = project.ConversationFilesCollection.SelectMany(f => f.Nodes.Where(n => n.Data.NodeTypeId == a.Data.NodeTypeId).Select(n => new { Node = n, File = f }));
                log(nodesofType.Select(n => new LogElement(n.File, n.Node, localizer)));
            }
            , null, null, null);
        }
    }
}
