using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Utilities;
using Conversation;

namespace ConversationEditor
{
    using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGui>;
    //using ConversationNode = IRenderable<ConversationEditor.INodeGui>;

    internal class NodeSet : IReadOnlyNodeSet
    {
        public static RectangleF GetArea(IEnumerable<IRenderable<IGui>> data)
        {
            if (data.Any())
            {
                var areas = data.Select(n => n.Renderer.Area);
                return areas.Skip(1).Aggregate(areas.First(), (c, d) => RectangleF.Union(c, d));
            }
            else
            {
                return RectangleF.Empty;
            }
        }

        HashSet<Id<NodeTemp>> m_nodes = new HashSet<Id<NodeTemp>>();
        HashSet<NodeGroup> m_groups = new HashSet<NodeGroup>();

        public event Action Changed;

        public void Add(Id<NodeTemp> node)
        {
            if (!m_nodes.Contains(node))
            {
                m_nodes.Add(node);
                Changed.Execute();
            }
        }

        public void Add(NodeGroup node)
        {
            if (!m_groups.Contains(node))
            {
                m_groups.Add(node);
                Changed.Execute();
            }
        }

        public void Remove(Id<NodeTemp> node)
        {
            m_nodes.Remove(node);
            Changed.Execute();
        }

        public void Remove(NodeGroup group)
        {
            m_groups.Remove(group);
            Changed.Execute();
        }

        public void Clear()
        {
            m_nodes.Clear();
            m_groups.Clear();
            Changed.Execute();
        }

        public NodeSet()
        {
        }



        //public RectangleF Area
        //{
        //    get
        //    {
        //        return NodeSet.GetArea(Renderable);
        //    }
        //}

        public NodeSet(IEnumerable<Id<NodeTemp>> nodes, IEnumerable<NodeGroup> groups)
            : this()
        {
            m_nodes.UnionWith(nodes);
            m_groups.UnionWith(groups);
        }

        internal NodeSet Clone()
        {
            return new NodeSet(m_nodes, m_groups);
        }

        public override bool Equals(object obj)
        {
            NodeSet other = obj as NodeSet;
            if (other != null)
            {
                return m_nodes.SetEquals(other.m_nodes);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public int Count()
        {
            return m_nodes.Count() + m_groups.Count;
        }

        public IEnumerable<Id<NodeTemp>> Nodes
        {
            get { return m_nodes; }
        }

        public IEnumerable<NodeGroup> Groups
        {
            get { return m_groups; }
        }

        public IEnumerable<IRenderable<IGui>> Renderable(Func<Id<NodeTemp>, IRenderable<IGui>> GetNode)
        {
            foreach (var node in Nodes)
                yield return GetNode(node);
            foreach (var group in Groups)
                yield return group;
        }
    }
}
