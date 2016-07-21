﻿using System;
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
    using ConversationNode = ConversationNode<INodeGui>;
    using System.Drawing;
    using System.Collections.ObjectModel;
    using System.Diagnostics.Contracts;
    using System.Diagnostics;

    internal abstract class GraphFile : Disposable, IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo>, IDisposable
    {
        public ConversationNode GetNode(Id<NodeTemp> id)
        {
            return m_nodesLookup[id];
        }

        protected CallbackList<ConversationNode> m_nodes;
        protected CallbackList<NodeGroup> m_groups;

        protected O1LookupWrapper<ConversationNode, Id<NodeTemp>> m_nodesLookup;
        protected SortedWrapper<ConversationNode> m_nodesOrdered;
        protected SortedWrapper<NodeGroup> m_groupsOrdered;

        protected ReadOnlyCollection<LoadError> m_errors;
        Dictionary<Output, TransitionNoduleUIInfo> m_cachedNodeUI = new Dictionary<Output, TransitionNoduleUIInfo>();

        private INodeFactory<ConversationNode> m_nodeFactory;
        private Func<ISaveableFileProvider, IEnumerable<Parameter>, Audio> m_generateAudio;
        private Func<IDynamicEnumParameter, DynamicEnumParameter.Source> m_getDocumentSource;
        private IAudioLibrary m_audioProvider;

        public ConversationNode MakeNode(IEditable e, NodeUIData uiData)
        {
            return m_nodeFactory.MakeNode(e, uiData);
        }

        protected GraphFile(IEnumerable<GraphAndUI<NodeUIData>> nodes, List<NodeGroup> groups, ReadOnlyCollection<LoadError> errors, INodeFactory<ConversationNode> nodeFactory,
            Func<ISaveableFileProvider, IEnumerable<Parameter>, Audio> generateAudio, Func<IDynamicEnumParameter, object, DynamicEnumParameter.Source> getDocumentSource, IAudioLibrary audioProvider)
        {
            Contract.Assert(getDocumentSource != null);
            m_nodeFactory = nodeFactory;
            m_generateAudio = generateAudio;
            m_getDocumentSource = a => getDocumentSource(a, this);
            m_audioProvider = audioProvider;
            m_nodes = new CallbackList<ConversationNode>(nodes.Select(gnu => MakeNode(gnu.GraphData, gnu.UIData)));
            m_nodesLookup = new O1LookupWrapper<ConversationNode, Id<NodeTemp>>(m_nodes, n => n.Id);
            m_nodesOrdered = new SortedWrapper<ConversationNode>(m_nodes);
            m_groups = new CallbackList<NodeGroup>(groups);
            m_groupsOrdered = new SortedWrapper<NodeGroup>(m_groups);
            m_errors = errors;

            IEnumerable<IDynamicEnumParameter> localDynamicEnumerationParameters = m_nodes.SelectMany(n => n.Parameters.OfType<IDynamicEnumParameter>());
            foreach (var ldep in localDynamicEnumerationParameters)
            {
                ldep.MergeInto(m_getDocumentSource(ldep));
            }

            m_nodes.Inserting += M_nodes_Inserting;
            m_nodes.Inserted += M_nodes_Inserted;
            m_nodes.Removing += M_nodes_Removing;
            m_nodes.Clearing += M_nodes_Clearing;
        }

        private void M_nodes_Clearing()
        {
            foreach (var parameter in m_nodes.SelectMany(n => n.Parameters).OfType<IDynamicEnumParameter>())
            {
                //Give it a junk source so it's value stops counting towards the real source
                parameter.MergeInto(new DynamicEnumParameter.Source());
            }
            foreach (var node in m_nodes)
                NodeRemoved.Execute(node);
        }

        private void M_nodes_Removing(ConversationNode<INodeGui> node)
        {
            foreach (var parameter in node.Parameters.OfType<IDynamicEnumParameter>())
            {
                //Give it a junk source so it's value stops counting towards the real source
                parameter.MergeInto(new DynamicEnumParameter.Source());
            }
            NodeRemoved.Execute(node);
        }

        private void M_nodes_Inserting(ConversationNode<INodeGui> node)
        {
            foreach (var parameter in node.Parameters.OfType<IDynamicEnumParameter>())
            {
                parameter.MergeInto(m_getDocumentSource(parameter));
            }
        }

        private void M_nodes_Inserted(ConversationNode<INodeGui> node)
        {
            NodeAdded.Execute(node);
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
        public Tuple<IEnumerable<ConversationNode>, IEnumerable<NodeGroup>> DuplicateInto(IEnumerable<GraphAndUI<NodeUIData>> nodeData, IEnumerable<NodeGroup> groups, object documentID, PointF location, ILocalizationEngine localization)
        {
            var nodes = nodeData.Select(gnu => MakeNode(gnu.GraphData, gnu.UIData)).Evaluate();

            IEnumerable<IDynamicEnumParameter> localDynamicEnumerationParameters = nodes.SelectMany(n => n.Parameters.OfType<IDynamicEnumParameter>().Where(p => p.Local));
            foreach (var ldep in localDynamicEnumerationParameters)
            {
                ldep.MergeInto(m_getDocumentSource(ldep));
            }

            if (nodes.Any() || groups.Any())
            {
                List<Action> undoActions = new List<Action>();
                List<Action> redoActions = new List<Action>();

                //Changes to these nodes don't need to be undoable as they're new nodes
                foreach (var node in nodes)
                {
                    foreach (var p in node.Parameters.OfType<LocalizedStringParameter>())
                    {
                        var result = localization.DuplicateActions(p.Value);
                        p.Value = result.Item1;
                        undoActions.Add(result.Item2.Undo);
                        redoActions.Add(result.Item2.Redo);
                    }

                    foreach (var p in node.Parameters.OfType<IAudioParameter>())
                    {
                        //No need to update audio usage as this will occur when the node is added/removed
                        var audio = m_generateAudio(this, node.Parameters);
                        var actions = p.SetValueAction(audio);
                        undoActions.Add(actions.Value.Undo);
                        redoActions.Add(actions.Value.Redo);
                    }

                    foreach (var p in node.Parameters.OfType<AudioParameter>())
                    {
                        p.Value = new Audio(Guid.NewGuid().ToString());
                    }

                    var oldID = node.Id;
                    node.ChangeId(Id<NodeTemp>.New());
                    foreach (var group in groups)
                    {
                        if (group.Contents.Contains(oldID))
                        {
                            group.Contents.Remove(oldID);
                            group.Contents.Add(node.Id);
                        }
                    }
                }

                var area = NodeSet.GetArea(nodes.Concat<IRenderable<IGui>>(groups));
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
                throw new InternalLogicException("why would you do this?");
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
                    m_audioProvider.UpdateUsage(n);
                    foreach (var group in containingGroups)
                        group.Contents.Add(n.Id);
                    actions.Undo();
                });
                undoActions.Add(() =>
                {
                    if (CanRemoveFromData(n, PromptNodeDeletion))
                    {
                        m_nodes.Remove(n);
                    }
                    foreach (var group in containingGroups)
                        group.Contents.Remove(n.Id);
                    actions.Redo();
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
                Undo = () => { using (m_audioProvider.SuppressUpdates()) foreach (Action action in undoActions) action(); },
                Redo = () => { using (m_audioProvider.SuppressUpdates()) foreach (Action action in redoActions) action(); },
            };
        }

        public bool Remove(IEnumerable<ConversationNode> nodes, IEnumerable<NodeGroup> groups)
        {
            nodes = nodes.ToList();
            groups = groups.ToList();
            bool removeNodes = nodes.Any();
            bool removeGroups = groups.Any();

            List<Action> undoActions = new List<Action>();
            List<Action> redoActions = new List<Action>();

            if (nodes.Any(n => !CanRemoveFromData(n, () => false)))
            {
                if (!PromptNodeDeletion())
                    return false;
            }

            if (removeNodes)
            {
                //Make sure all the nodes are added before trying to link them
                foreach (var node in nodes)
                {
                    var n = node;
                    undoActions.Add(() => { m_nodes.Add(n); });
                }

                foreach (var node in nodes)
                {
                    var n = node;
                    var actions = n.GetNodeRemoveActions();
                    var containingGroups = m_groups.Where(g => g.Contents.Contains(n.Id)).Evaluate();
                    undoActions.Add(() =>
                    {
                        actions.Undo(); //Connect after adding the node
                        foreach (var group in containingGroups)
                            group.Contents.Add(n.Id);
                        m_audioProvider.UpdateUsage(n);
                    });
                    redoActions.Add(() =>
                    {
                        actions.Redo(); //Disconnect before removing the node
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

            Action undo = () => { using (m_audioProvider.SuppressUpdates()) foreach (Action action in undoActions) action(); };
            Action redo = () => { using (m_audioProvider.SuppressUpdates()) foreach (Action action in redoActions) action(); };

            string message;
            if (removeNodes && removeGroups)
                message = "Deleted elements";
            else if (removeNodes)
                message = "Deleted nodes";
            else if (removeGroups)
                message = "Removed groupings";
            else
                throw new InternalLogicException("Something went wrong :(");

            UndoableFile.Change(new GenericUndoAction(undo, redo, message));

            return true;
        }

        public void RemoveLinks(Output o)
        {
            UndoableFile.Change(new GenericUndoAction(o.DisconnectAll(), "Removed links"));
        }

        public abstract ISaveableFileUndoable UndoableFile { get; }
        ISaveableFile ISaveableFileProvider.File { get { return UndoableFile; } }

        public ReadOnlyCollection<LoadError> Errors
        {
            get { return m_errors; }
        }

        public void ClearErrors()
        {
            ReadOnlyCollection<LoadError> oldErrors = m_errors;
            GenericUndoAction action = new GenericUndoAction(() => { m_errors = oldErrors; }, () => { m_errors = new ReadOnlyCollection<LoadError>(new LoadError[0]); }, "Cleared errors on conversation");
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ISaveableFileProvider me = this as ISaveableFileProvider;
                me.File.Dispose();
            }
        }

        public TransitionNoduleUIInfo UIInfo(Output connection, bool canFail)
        {
            if (!m_cachedNodeUI.ContainsKey(connection))
            {
                var node = m_nodes.Where(n => n.Connectors.Any(c => c.Id == connection.Id && n.Id == connection.Parent.NodeId)).SingleOrDefault();
                if (node == null && canFail)
                    return null;
                var comparable = node.Connectors.Where(c => c.m_definition.Position == connection.m_definition.Position);
                m_cachedNodeUI[connection] = CreateTransitionUIInfo(node, connection.m_definition.Position, comparable.IndexOf(connection), comparable.Count());
            }
            return m_cachedNodeUI[connection];
        }

        private static TransitionNoduleUIInfo CreateTransitionUIInfo(ConversationNode node, ConnectorPosition position, int i, int count)
        {
            Func<RectangleF, RectangleF> top = (area) =>
            {
                float per = area.Width / (float)count;
                float y = area.Top - 10;
                return new RectangleF(area.Left + (int)(per * (i + 0.5f)) - 5, y, 10, 10);
            };

            Func<RectangleF, RectangleF> bottom = (area) =>
            {
                float per = area.Width / (float)count;
                float y = area.Bottom;
                return new RectangleF(area.Left + (int)(per * (i + 0.5f)) - 5, y, 10, 10);
            };

            Func<RectangleF, RectangleF> left = (area) =>
            {
                float per = area.Height / (float)count;
                float x = area.Left - 10;
                return new RectangleF(x, area.Top + (int)(per * (i + 0.5f)) - 5, 10, 10);
            };

            Func<RectangleF, RectangleF> right = (area) =>
            {
                float per = area.Height / (float)count;
                float x = area.Right;
                return new RectangleF(x, area.Top + (int)(per * (i + 0.5f)) - 5, 10, 10);
            };

            var result = new TransitionNoduleUIInfo(position.For(() => top(node.Renderer.Area), () => bottom(node.Renderer.Area), () => left(node.Renderer.Area), () => right(node.Renderer.Area)));
            Action<Changed<RectangleF>> areaChanged = c =>
            {
                result.Area.Value = position.For(() => top(node.Renderer.Area), () => bottom(node.Renderer.Area), () => left(node.Renderer.Area), () => right(node.Renderer.Area));
            };
            node.Renderer.AreaChanged += areaChanged;
            node.RendererChanging += () => node.Renderer.AreaChanged -= areaChanged;
            node.RendererChanged += () => node.Renderer.AreaChanged += areaChanged;
            return result;
        }

        public event Action NodesDeleted;

        public event Action<ConversationNode> NodeAdded;
        public event Action<ConversationNode> NodeRemoved;

        public static bool PromptNodeDeletion()
        {
            var result = MessageBox.Show("Removing this node will result in a domain which does not support the currently loaded conversations", "Ok to remove node?", MessageBoxButtons.OKCancel);
            return result == DialogResult.OK;
        }

        public static bool PromptFileRemoved()
        {
            var result = MessageBox.Show("Removing this file will result in a domain which does not support the currently loaded conversations", "Ok to remove file?", MessageBoxButtons.OKCancel);
            return result == DialogResult.OK;
        }

        protected virtual void RemoveFromData(ConversationNode node)
        {
            //Do nothing;
        }

        protected virtual bool CanRemoveFromData(ConversationNode node, Func<bool> prompt)
        {
            return true;
        }

        public int RelativePosition(ConversationNode ofNode, ConversationNode relativeTo)
        {
            return m_nodesOrdered.RelativePosition(ofNode, relativeTo);
        }
    }
}
