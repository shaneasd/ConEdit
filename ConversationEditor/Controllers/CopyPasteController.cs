using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Conversation;
using Utilities;
using Conversation.Serialization;

namespace ConversationEditor
{
    using ConversationNode = ConversationNode<INodeGui>;

    internal abstract class CopyPasteController<TNode, TTransitionUI> where TNode : IRenderable<IGui>
    {
        public abstract Tuple<IEnumerable<GraphAndUI<NodeUIData>>, IEnumerable<NodeGroup>, PointF, object> Duplicate(IEnumerable<TNode> nodes, IEnumerable<NodeGroup> groups, IDataSource datasource);
        public abstract void Copy(IEnumerable<TNode> nodes, IEnumerable<NodeGroup> groups);
        public abstract void Cut(IEnumerable<TNode> nodes, IEnumerable<NodeGroup> groups);
        public abstract Tuple<IEnumerable<GraphAndUI<NodeUIData>>, IEnumerable<NodeGroup>, object, bool> Paste(IDataSource datasource);
    }

    internal class ConversationCopyPasteController : CopyPasteController<ConversationNode, TransitionNoduleUIInfo>
    {
        private bool m_shouldDuplicate;

        public static ConversationCopyPasteController Instance { get; } = new ConversationCopyPasteController();

        public override Tuple<IEnumerable<GraphAndUI<NodeUIData>>, IEnumerable<NodeGroup>, PointF, object> Duplicate(IEnumerable<ConversationNode> nodes, IEnumerable<NodeGroup> groups, IDataSource datasource)
        {
            var area = NodeSet.GetArea(nodes.Concat<IRenderable<IGui>>(groups));
            PointF loc = (new PointF(50, 50)).Plus(area.Center());

            using (MemoryStream m = new MemoryStream())
            {
                CopyToStream(nodes, groups, m);
                m.Position = 0;
                var nodesAndGroups = ReadFromStream(datasource, m);
                return Tuple.Create(nodesAndGroups.Item1, nodesAndGroups.Item2, loc, nodesAndGroups.Item3);
            }
        }

        private static void CopyToStream(IEnumerable<ConversationNode> nodes, IEnumerable<NodeGroup> groups, Stream m)
        {
            var serializer = SerializationUtils.ConversationSerializer;
            serializer.Write(SerializationUtils.MakeConversationData(nodes.Cast<ConversationNode>(), new ConversationEditorData(groups)), m);
        }

        public override void Copy(IEnumerable<ConversationNode> nodes, IEnumerable<NodeGroup> groups)
        {
            using (MemoryStream m = new MemoryStream())
            {
                CopyToStream(nodes, groups, m);
                m.Position = 0;
                Clipboard.SetDataObject(m.ToArray());
            }
            m_shouldDuplicate = true;
        }

        public override void Cut(IEnumerable<ConversationNode> nodes, IEnumerable<NodeGroup> groups)
        {
            using (MemoryStream m = new MemoryStream())
            {
                CopyToStream(nodes, groups, m);
                m.Position = 0;
                Clipboard.SetDataObject(m.ToArray());
            }
            m_shouldDuplicate = false;
        }

        public override Tuple<IEnumerable<GraphAndUI<NodeUIData>>, IEnumerable<NodeGroup>, object, bool> Paste(IDataSource datasource)
        {
            Tuple<IEnumerable<GraphAndUI<NodeUIData>>, IEnumerable<NodeGroup>, object, bool> result = null;
            var clipboardData = Clipboard.GetDataObject();
            if (clipboardData.GetDataPresent(typeof(byte[])))
            {
                var bytes = (byte[])clipboardData.GetData(typeof(byte[]));
                using (MemoryStream m = new MemoryStream(bytes))
                {
                    result = ReadFromStream(datasource, m);
                }
            }
            result = result ?? Tuple.Create(Enumerable.Empty<GraphAndUI<NodeUIData>>(), Enumerable.Empty<NodeGroup>(), new object(), false);

            m_shouldDuplicate = true;

            return result;
        }

        private Tuple<IEnumerable<GraphAndUI<NodeUIData>>, IEnumerable<NodeGroup>, object, bool> ReadFromStream(IDataSource datasource, Stream m)
        {
            var deserializer = SerializationUtils.ConversationDeserializer(datasource);
            var data = deserializer.Read(m);
            var groups = data.EditorData.Groups;
            return Tuple.Create(data.Nodes, groups, data.DocumentId, m_shouldDuplicate);
        }
    }
}
