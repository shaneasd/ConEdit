using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using System.IO;
using System.Windows.Forms;
using Utilities;
using Conversation.Serialization;

namespace ConversationEditor
{
    using ConversationNode = ConversationNode<INodeGUI>;
    using System.Drawing;

    public abstract class GraphFile : IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo>, IDisposable
    {
        public ConversationNode GetNode(ID<NodeTemp> id)
        {
            return m_nodesLookup[id];
        }

        protected CallbackList<ConversationNode> m_nodes;
        protected CallbackList<NodeGroup> m_groups;

        protected O1LookupWrapper<ConversationNode, ID<NodeTemp>> m_nodesLookup;
        protected SortedWrapper<ConversationNode> m_nodesOrdered;
        protected SortedWrapper<NodeGroup> m_groupsOrdered;

        protected List<Error> m_errors;

        private INodeFactory<ConversationNode> m_nodeFactory;
        public ConversationNode MakeNode(IEditable e, NodeUIData uiData)
        {
            return m_nodeFactory.MakeNode(e, uiData);
        }

        public GraphFile(IEnumerable<GraphAndUI<NodeUIData>> nodes, List<NodeGroup> groups, List<Error> errors, INodeFactory<ConversationNode> nodeFactory)
        {
            m_nodeFactory = nodeFactory;
            m_nodes = new CallbackList<ConversationNode>(nodes.Select(gnu => MakeNode(gnu.GraphData, gnu.UIData)));
            m_nodesLookup = new O1LookupWrapper<ConversationNode, ID<NodeTemp>>(m_nodes, n => n.Id);
            m_nodesOrdered = new SortedWrapper<ConversationNode>(m_nodes);
            m_groups = new CallbackList<NodeGroup>(groups);
            m_groupsOrdered = new SortedWrapper<NodeGroup>(m_groups);
            m_errors = errors;
        }

        public IEnumerableReversible<ConversationNode> Nodes
        {
            get { return m_nodesOrdered; }
        }

        public IEnumerableReversible<NodeGroup> Groups
        {
            get { return m_groupsOrdered; }
        }

        //TODO: Duplicate the IEditable with a new ID (must be deep copy of parameters)
        public Tuple<IEnumerable<ConversationNode>, IEnumerable<NodeGroup>> DuplicateInto(IEnumerable<GraphAndUI<NodeUIData>> nodeData, IEnumerable<NodeGroup> groups, PointF location, LocalizationEngine localization)
        {
            var nodes = nodeData.Select(gnu => MakeNode(gnu.GraphData, gnu.UIData)).Evaluate();

            if (nodes.Any() || groups.Any())
            {
                List<Action> undoActions = new List<Action>();
                List<Action> redoActions = new List<Action>();

                //Changes to these nodes don't need to be undoable as they're new nodes
                foreach (var node in nodes)
                {
                    foreach (var p in node.Parameters.OfType<LocalizedStringParameter>())
                    {
                        var localize = p as LocalizedStringParameter;
                        var result = localization.DuplicateActions(localize.Value);
                        localize.Value = result.Item1;
                        undoActions.Add(result.Item2.Undo);
                        redoActions.Add(result.Item2.Redo);
                    }

                    var oldID = node.Id;
                    node.ChangeId(ID<NodeTemp>.New());
                    foreach (var group in groups)
                    {
                        if (group.Contents.Contains(oldID))
                        {
                            group.Contents.Remove(oldID);
                            group.Contents.Add(node.Id);
                        }
                    }
                }

                var area = NodeSet.GetArea(nodes.Concat<IRenderable<IGUI>>(groups));
                PointF offset = location.Take(area.Center());
                foreach (var node in nodes)
                {
                    node.Renderer.Offset(offset);
                }
                foreach (var group in groups)
                {
                    group.Renderer.Offset(offset);
                }

                SimpleUndoPair addActions = InnerAddNodes(nodes, groups);

                Action undo = () =>
                    {
                        undoActions.ForEach(a => a());
                        addActions.Undo();
                    };
                Action redo = () =>
                {
                    redoActions.ForEach(a => a());
                    addActions.Redo();
                };

                UndoableFile.Change(new GenericUndoAction(undo, redo, "Pasted"));
            }

            return Tuple.Create(nodes, groups);
        }

        public void Add(IEnumerable<ConversationNode> nodes, IEnumerable<NodeGroup> groups)
        {
            nodes = nodes.Evaluate();
            groups = groups.ToList();

            SimpleUndoPair addActions = InnerAddNodes(nodes, groups);

            bool addedNodes = nodes.Any();
            bool addedGroups = groups.Any();

            if (addedNodes && addedGroups)
                UndoableFile.Change(new GenericUndoAction(addActions.Undo, addActions.Redo, "Added nodes and groups"));
            else if (addedNodes)
                UndoableFile.Change(new GenericUndoAction(addActions.Undo, addActions.Redo, "Added nodes"));
            else if (addedGroups)
                UndoableFile.Change(new GenericUndoAction(addActions.Undo, addActions.Redo, "Added groups"));
            else
                throw new Exception("why would you do this?");
        }

        private SimpleUndoPair InnerAddNodes(IEnumerable<ConversationNode> nodes, IEnumerable<NodeGroup> groups)
        {
            List<Action> undoActions = new List<Action>();
            List<Action> redoActions = new List<Action>();

            //Set up actions for adding/removing the nodes
            foreach (var node in nodes)
            {
                var n = node;
                var actions = n.GetNodeRemoveActions();
                var containingGroups = m_groups.Where(g => g.Contents.Contains(n.Id)).Evaluate();
                redoActions.Add(() =>
                {
                    m_nodes.Add(n);
                    foreach (var group in containingGroups)
                        group.Contents.Add(n.Id);
                    actions.Item1();
                });
                undoActions.Add(() =>
                {
                    m_nodes.Remove(n);
                    foreach (var group in containingGroups)
                        group.Contents.Remove(n.Id);
                    actions.Item2();
                    NodesDeleted.Execute();
                });
            }

            //Set up actions for adding/removing nodes from other groups that are gaining/losing their grouping due to removing/adding new groups
            foreach (var group in groups)
            {
                foreach (var node in group.Contents)
                {
                    var n = node;
                    var old = m_groups.SingleOrDefault(g => g.Contents.Contains(n));
                    if (old != null)
                    {
                        undoActions.Add(() => old.Contents.Add(n));
                        redoActions.Add(() => old.Contents.Remove(n));
                    }
                }
            }

            //Set up actions for adding/removing the groups
            undoActions.Add(() =>
            {
                foreach (var group in groups.Reverse())
                {
                    m_groups.Remove(group);
                }
            });
            redoActions.Add(() =>
            {
                m_groups.AddRange(groups);
            });

            return new SimpleUndoPair
            {
                Undo = () => { foreach (Action action in undoActions) action(); },
                Redo = () => { foreach (Action action in redoActions) action(); },
            };
        }

        public void Remove(IEnumerable<ConversationNode> nodes, IEnumerable<NodeGroup> groups)
        {
            nodes = nodes.ToList();
            groups = groups.ToList();
            bool removeNodes = nodes.Any();
            bool removeGroups = groups.Any();

            List<Action> undoActions = new List<Action>();
            List<Action> redoActions = new List<Action>();

            if (removeNodes)
            {
                foreach (var node in nodes)
                {
                    var n = node;
                    var actions = n.GetNodeRemoveActions();
                    var containingGroups = m_groups.Where(g => g.Contents.Contains(n.Id)).Evaluate();
                    undoActions.Add(() =>
                    {
                        m_nodes.Add(n);
                        actions.Item1(); //Connect after adding the node
                        foreach (var group in containingGroups)
                            group.Contents.Add(n.Id);
                    });
                    redoActions.Add(() =>
                    {
                        actions.Item2(); //Disconnect before removing the node
                        m_nodes.Remove(n);
                        foreach (var group in containingGroups)
                            group.Contents.Remove(n.Id);
                        NodesDeleted.Execute();
                    });
                }
            }
            if (removeGroups)
            {
                List<NodeGroup> toAdd = new List<NodeGroup>(groups);
                undoActions.Add(() =>
                {
                    m_groups.AddRange(toAdd);
                });
                redoActions.Add(() =>
                {
                    m_groups.RemoveRange(toAdd);
                });
            }

            Action undo = () => { foreach (Action action in undoActions) action(); };
            Action redo = () => { foreach (Action action in redoActions) action(); };

            string message;
            if (removeNodes && removeGroups)
                message = "Deleted elements";
            else if (removeNodes)
                message = "Deleted nodes";
            else if (removeGroups)
                message = "Removed groupings";
            else
                throw new Exception("Something went wrong :(");

            UndoableFile.Change(new GenericUndoAction(undo, redo, message));
        }

        public void RemoveLinks(Output o)
        {
            UndoableFile.Change(new GenericUndoAction(o.DisconnectAll(), "Removed links"));
        }

        public abstract ISaveableFileUndoable UndoableFile { get; }
        ISaveableFile ISaveableFileProvider.File { get { return UndoableFile; } }

        public List<Error> Errors
        {
            get { return m_errors; }
        }

        public void ClearErrors()
        {
            List<Error> oldErrors = m_errors;
            GenericUndoAction action = new GenericUndoAction(() => { m_errors = oldErrors; }, () => { m_errors = new List<Error>(); }, "Cleared errors on conversation");
            UndoableFile.Change(action);
        }

        public void BringToFront(IReadonlyNodeSet Selected)
        {
            m_nodesOrdered.BringToFront(Selected.Nodes.Select(GetNode));
            m_groupsOrdered.BringToFront(Selected.Groups);
        }

        public event Action FileModifiedExternally
        {
            add { (this as ISaveableFileProvider).File.FileModifiedExternally += value; }
            remove { (this as ISaveableFileProvider).File.FileModifiedExternally -= value; }
        }

        public event Action FileDeletedExternally
        {
            add { (this as ISaveableFileProvider).File.FileDeletedExternally += value; }
            remove { (this as ISaveableFileProvider).File.FileDeletedExternally -= value; }
        }

        public void Dispose()
        {
            ISaveableFileProvider me = this as ISaveableFileProvider;
            me.File.Dispose();
        }

        Dictionary<Output, TransitionNoduleUIInfo> m_cachedNodeUI = new Dictionary<Output, TransitionNoduleUIInfo>();
        public TransitionNoduleUIInfo UIInfo(Output connection)
        {
            if (!m_cachedNodeUI.ContainsKey(connection))
            {
                var node = m_nodes.Where(n => n.Connectors.Any(c => c.ID == connection.ID && n.Id == connection.Parent.NodeID)).Single();
                var comparable = node.Connectors.Where(c => c.m_definition.Position == connection.m_definition.Position);
                m_cachedNodeUI[connection] = CreateTransitionUIInfo(node, connection.m_definition.Position, comparable.IndexOf(connection), comparable.Count());
            }
            return m_cachedNodeUI[connection];
        }

        public TransitionNoduleUIInfo CreateTransitionUIInfo(ConversationNode node, ConnectorPosition position, int i, int count)
        {
            Func<RectangleF> top = () =>
            {
                float per = node.Renderer.Area.Width / (float)count;
                float y = node.Renderer.Area.Top - 10;
                return new RectangleF(node.Renderer.Area.Left + (int)(per * (i + 0.5f)) - 5, y, 10, 10);
            };

            Func<RectangleF> bottom = () =>
            {
                float per = node.Renderer.Area.Width / (float)count;
                float y = node.Renderer.Area.Bottom;
                return new RectangleF(node.Renderer.Area.Left + (int)(per * (i + 0.5f)) - 5, y, 10, 10);
            };

            Func<RectangleF> left = () =>
            {
                float per = node.Renderer.Area.Height / (float)count;
                float x = node.Renderer.Area.Left - 10;
                return new RectangleF(x, node.Renderer.Area.Top + (int)(per * (i + 0.5f)) - 5, 10, 10);
            };

            Func<RectangleF> right = () =>
            {
                float per = node.Renderer.Area.Height / (float)count;
                float x = node.Renderer.Area.Right;
                return new RectangleF(x, node.Renderer.Area.Top + (int)(per * (i + 0.5f)) - 5, 10, 10);
            };

            return new TransitionNoduleUIInfo(() => position.For(top, bottom, left, right));
        }

        public event Action NodesDeleted;
    }


}
