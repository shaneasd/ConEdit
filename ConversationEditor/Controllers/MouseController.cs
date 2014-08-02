﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using Utilities;
using Conversation;
using ConversationEditor.Controllers;

namespace ConversationEditor
{
    //TODO: Lost focus handler to reset state

    public class MouseController<TNode> where TNode : IRenderable<IGUI>, IGraphNode, IConfigurable
    {
        private static Brush HATCH = new HatchBrush(HatchStyle.Percent50, ColorScheme.SelectionRectangle);

        public class State
        {
            private MouseController<TNode> m_parent;
            public class Netting : State
            {
                PointF m_leftButtonDownPos;
                private MouseController<TNode> m_controller;
                private PointF m_lastClientPos;
                public Netting(MouseController<TNode> parent, PointF start, MouseController<TNode> controller)
                    : base(parent)
                {
                    m_leftButtonDownPos = start;
                    m_lastClientPos = start;
                    m_controller = controller;
                }

                public RectangleF SelectionRectangle()
                {
                    PointF start = m_leftButtonDownPos;
                    SizeF size = new SizeF(m_lastClientPos.X - m_leftButtonDownPos.X, m_lastClientPos.Y - m_leftButtonDownPos.Y);
                    if (size.Width < 0)
                    {
                        start.X += size.Width;
                        size.Width = -size.Width;
                    }
                    if (size.Height < 0)
                    {
                        start.Y += size.Height;
                        size.Height = -size.Height;
                    }

                    return new RectangleF(start, size);
                }

                public override void LeftMouseUp(Point client, Point screen)
                {
                    foreach (var node in m_controller.m_nodes().Evaluate())
                    {
                        if (RectangleF.Intersect(SelectionRectangle(), node.Renderer.Area) != RectangleF.Empty)
                            m_controller.m_selection.Add(node.Id);
                    }
                    m_parent.m_state = new State.Nothing(m_parent, null);
                }

                public override void Draw(Graphics g, ConnectionDrawer connections)
                {
                    g.FillRectangle(HATCH, SelectionRectangle());
                }

                public override void MouseMove(PointF client, Point screen)
                {
                    m_lastClientPos = client;
                }
            }
            public class Dragging : State
            {
                public PointF m_moveOrigin;
                public PointF m_lastClientPos;
                IRenderable<IGUI> m_dragTarget;

                public Dragging(MouseController<TNode> parent, PointF moveOrigin, PointF client, IRenderable<IGUI> dragTarget)
                    : base(parent)
                {
                    m_moveOrigin = moveOrigin;
                    m_lastClientPos = client;
                    m_dragTarget = dragTarget;
                }

                public override void MouseMove(PointF client, Point screen)
                {
                    PointF offset = client.Take(m_lastClientPos); //Amount the mouse has moved (since we last actually moved the nodes)
                    var selectionOrigin = m_dragTarget.Renderer.Area.Center();
                    var newPosF = m_parent.Snap(selectionOrigin.Plus(offset));
                    offset = newPosF.Take(selectionOrigin); //The actual offset applied

                    foreach (var a in m_parent.m_selection.Renderable(id => m_parent.GetNode(id)))
                        a.Renderer.Offset(offset);
                    m_lastClientPos = m_lastClientPos.Plus(offset);
                }

                public override void LeftMouseUp(Point client, Point screen)
                {
                    var currentSelection = m_parent.m_selection.Clone();

                    List<Action> undoList = new List<Action>();
                    List<Action> redoList = new List<Action>();
                    var offset = m_moveOrigin.Take(m_dragTarget.Renderer.Area.Center());

                    if (!offset.IsEmpty)
                    {
                        foreach (var a in currentSelection.Renderable(id => m_parent.GetNode(id)))
                        {
                            var aa = a;
                            undoList.Add(() => { aa.Renderer.Offset(offset); });
                            var currentPos = aa.Renderer.Area.Center();
                            redoList.Add(() => { aa.Renderer.MoveTo(currentPos); });
                        }

                        var groups = m_parent.m_groups();
                        foreach (var node in currentSelection.Nodes)
                        {
                            var n = node;
                            if (!currentSelection.Groups.Any(g => g.Contents.Contains(n)))
                            {
                                foreach (var group in groups)
                                {
                                    if (group.Contents.Contains(n))
                                    {
                                        undoList.Add(() => { group.Contents.Add(n); });
                                        Action redoOp = () => { group.Contents.Remove(n); };
                                        redoList.Add(redoOp);
                                        redoOp();
                                    }
                                }

                                var caught = groups.FirstOrDefault(g => g.Renderer.Area.Contains(m_parent.GetNode(n).Renderer.Area));
                                if (caught != null)
                                {
                                    undoList.Add(() => { caught.Contents.Remove(n); });
                                    Action redoOp = () => { caught.Contents.Add(n); };
                                    redoList.Add(redoOp);
                                    redoOp();
                                }
                            }
                        }

                        Action undo = () =>
                        {
                            foreach (Action a in undoList)
                                a();
                        };

                        Action redo = () =>
                        {
                            foreach (Action a in redoList)
                                a();
                        };
                        m_parent.Changed.Execute(new GenericUndoAction(undo, redo, "Dragged " + currentSelection.Count() + " things"));
                    }

                    m_parent.m_state = new State.Nothing(m_parent, null);
                }
            }
            public class Linking : State
            {
                private Output LinkingTransition;
                PointF m_lastClientPos;

                public Linking(MouseController<TNode> parent, Output linking, PointF client)
                    : base(parent)
                {
                    LinkingTransition = linking;
                    m_lastClientPos = client;
                }

                private void Connect(Output inConnector, Output outConnector)
                {
                    if (!outConnector.Connections.Contains(inConnector))
                    {
                        Action redo = () => { outConnector.ConnectTo(inConnector); };
                        Action undo = () => { outConnector.Disconnect(inConnector); };
                        m_parent.Changed.Execute(new GenericUndoAction(undo, redo, "Connected nodes"));
                    }
                }

                public override void MouseMove(PointF client, Point screen)
                {
                    m_lastClientPos = client;
                }

                public override void LeftMouseUp(Point client, Point screen)
                {
                    if (!Control.ModifierKeys.HasFlag(Keys.Shift))
                    {
                        m_parent.ForClickedOn(client,
                            x => { },
                            x =>
                            {
                                if (x != LinkingTransition)
                                    Connect(LinkingTransition, x);
                                else
                                    m_parent.SelectedTransition = LinkingTransition;
                                LinkingTransition = null;
                            },
                            (a, b) => { },
                            x => { },
                            () => { });
                    }
                    else
                    {
                        var x = m_parent.BestConnector(client, a => m_parent.CanConnect(a, LinkingTransition));
                        if (x != LinkingTransition)
                            Connect(LinkingTransition, x);
                        else
                            m_parent.SelectedTransition = LinkingTransition;
                        LinkingTransition = null;
                    }
                    m_parent.m_state = new State.Nothing(m_parent, null);
                    m_parent.RefreshDisplay();
                }

                public override void Draw(Graphics g, ConnectionDrawer connections)
                {
                    if (Control.ModifierKeys.HasFlag(Keys.Shift))
                    {
                        var p0 = m_lastClientPos;
                        Output x = m_parent.BestConnector(p0, a => m_parent.CanConnect(a, LinkingTransition));
                        connections.Add(m_parent.UIInfo(x).Area.Center(), m_parent.UIInfo(LinkingTransition).Area.Center(), false);
                    }
                    else
                    {
                        connections.Add(m_lastClientPos, m_parent.UIInfo(LinkingTransition).Area.Center(), false);
                    }
                }
            }
            public class Scrolling : State
            {
                Point m_lastScreenPos;
                public Scrolling(MouseController<TNode> parent, Point lastScreenPos)
                    : base(parent)
                {
                    m_lastScreenPos = lastScreenPos;
                }

                public override void MouseMove(PointF client, Point screen)
                {
                    var shift = m_lastScreenPos.Take(screen);
                    m_parent.Shift(shift);
                    m_lastScreenPos = screen;
                }

                public override void MiddleMouseUp(Point client, Point screen)
                {
                    m_parent.m_state = new State.Nothing(m_parent, null);
                }
            }
            public class Killing : State
            {
                Point m_midButtonDownPos;
                public Killing(MouseController<TNode> parent, Point start)
                    : base(parent)
                {
                    m_midButtonDownPos = start;
                }

                private void Kill(TNode x)
                {
                    if (x.Renderer.Area.Contains(m_midButtonDownPos))
                    {
                        m_parent.RemoveNode(x);
                        m_parent.Deleted();
                    }
                }

                public override void MiddleMouseUp(Point client, Point screen)
                {
                    m_parent.ForClickedOn(client,
                        x =>
                        {
                            Kill(x);
                        },
                        x => { }, (a, b) => { }, x => { }, () => { });

                    m_parent.m_state = new State.Nothing(m_parent, null);
                }
            }
            public class Nothing : State
            {
                public Nothing(MouseController<TNode> parent, ResizeState? resizeState)
                    : base(parent)
                {
                    ResizeState = resizeState;
                }
                public readonly ResizeState? ResizeState;

                public override bool If(Action<Nothing> n)
                {
                    n(this);
                    return true;
                }

                public override Cursor Cursor
                {
                    get
                    {
                        switch (ResizeState)
                        {
                            case MouseController<TNode>.ResizeState.L:
                            case MouseController<TNode>.ResizeState.R:
                                return Cursors.SizeWE;
                            case MouseController<TNode>.ResizeState.T:
                            case MouseController<TNode>.ResizeState.B:
                                return Cursors.SizeNS;
                            case MouseController<TNode>.ResizeState.TL:
                            case MouseController<TNode>.ResizeState.BR:
                                return Cursors.SizeNWSE;
                            case MouseController<TNode>.ResizeState.TR:
                            case MouseController<TNode>.ResizeState.BL:
                                return Cursors.SizeNESW;
                            default:
                                return Cursors.Default;
                        }
                    }
                }

                public override void MouseMove(PointF client, Point screen)
                {
                    Action<NodeGroup> groupOp = group =>
                    {
                        var totalArea = group.Renderer.Area;
                        var newResizeState = m_parent.GetResizeOption(totalArea, client);
                        if (newResizeState != ResizeState)
                            m_parent.m_state = new State.Nothing(m_parent, newResizeState);
                    };
                    Action clearState = () =>
                    {
                        if (null != ResizeState)
                            m_parent.m_state = new State.Nothing(m_parent, null);
                    };
                    m_parent.ForClickedOn(client, n => { clearState(); }, t => { clearState(); }, (a, b) => { clearState(); }, groupOp, clearState);
                }
            }
            public class Resizing : State
            {
                public Resizing(MouseController<TNode> parent, ResizeState resizeState, RectangleF resizeOriginalArea, NodeGroup group, Action<NodeGroup> updateNodesInGroup)
                    : base(parent)
                {
                    ResizeState = resizeState;
                    m_resizeOriginalArea = resizeOriginalArea;
                    m_group = group;
                    UpdateNodesInGroup = () => updateNodesInGroup(group);
                }
                public readonly ResizeState ResizeState;
                public readonly RectangleF m_resizeOriginalArea;
                public readonly NodeGroup m_group;
                public readonly Action UpdateNodesInGroup;

                public override void LeftMouseUp(Point client, Point screen)
                {
                    var newArea = m_group.Renderer.Area;
                    var oldArea = m_resizeOriginalArea;

                    UpdateNodesInGroup();

                    Action undo = () => { m_group.Renderer.Area = oldArea; UpdateNodesInGroup(); };
                    Action redo = () => { m_group.Renderer.Area = newArea; UpdateNodesInGroup(); };

                    m_parent.Changed.Execute(new GenericUndoAction(undo, redo, "Resized group"));
                    m_parent.m_state = new State.Nothing(m_parent, null);
                }

                public override void MouseMove(PointF client, Point screen)
                {
                    var group = m_parent.m_selection.Groups.Single(); //There should be exactly one

                    switch (ResizeState)
                    {
                        case MouseController<TNode>.ResizeState.L:
                            group.Renderer.MoveLeft(client.X);
                            break;
                        case MouseController<TNode>.ResizeState.R:
                            group.Renderer.MoveRight(client.X);
                            break;
                        case MouseController<TNode>.ResizeState.T:
                            group.Renderer.MoveTop(client.Y);
                            break;
                        case MouseController<TNode>.ResizeState.B:
                            group.Renderer.MoveBottom(client.Y);
                            break;
                        case MouseController<TNode>.ResizeState.TL:
                            group.Renderer.MoveLeft(client.X);
                            group.Renderer.MoveTop(client.Y);
                            break;
                        case MouseController<TNode>.ResizeState.TR:
                            group.Renderer.MoveTop(client.Y);
                            group.Renderer.MoveRight(client.X);
                            break;
                        case MouseController<TNode>.ResizeState.BL:
                            group.Renderer.MoveBottom(client.Y);
                            group.Renderer.MoveLeft(client.X);
                            break;
                        case MouseController<TNode>.ResizeState.BR:
                            group.Renderer.MoveBottom(client.Y);
                            group.Renderer.MoveRight(client.X);
                            break;
                    }
                    m_parent.RefreshDisplay();
                }

                public override Cursor Cursor
                {
                    get
                    {
                        switch (ResizeState)
                        {
                            case MouseController<TNode>.ResizeState.L:
                            case MouseController<TNode>.ResizeState.R:
                                return Cursors.SizeWE;
                            case MouseController<TNode>.ResizeState.T:
                            case MouseController<TNode>.ResizeState.B:
                                return Cursors.SizeNS;
                            case MouseController<TNode>.ResizeState.TL:
                            case MouseController<TNode>.ResizeState.BR:
                                return Cursors.SizeNWSE;
                            case MouseController<TNode>.ResizeState.TR:
                            case MouseController<TNode>.ResizeState.BL:
                                return Cursors.SizeNESW;
                            default:
                                return Cursors.Default;
                        }
                    }
                }
            }
            public class DraggingLinks : State
            {
                public readonly Output SelectedTransition;
                PointF m_lastClientPos;

                public DraggingLinks(MouseController<TNode> parent, Output selectedTransition, PointF client)
                    : base(parent)
                {
                    SelectedTransition = selectedTransition;
                    m_lastClientPos = client;
                }

                public override void Draw(Graphics g, ConnectionDrawer connections)
                {
                    foreach (Output connection in SelectedTransition.Connections)
                    {
                        if (Control.ModifierKeys.HasFlag(Keys.Shift))
                        {
                            var p0 = m_lastClientPos;
                            Output x = m_parent.BestConnector(p0, a => m_parent.CanConnect(a, connection));
                            connections.Add(m_parent.UIInfo(x).Area.Center(), m_parent.UIInfo(connection).Area.Center(), true);
                        }
                        else
                        {
                            connections.Add(Util.Center(m_parent.UIInfo(connection).Area), m_lastClientPos, true);
                        }
                    }
                }

                public override void MouseMove(PointF client, Point screen)
                {
                    m_lastClientPos = client;
                }

                public override void LeftMouseUp(Point client, Point screen)
                {
                    Action<Output> action = x =>
                    {
                        if (x != SelectedTransition)
                        {
                            var removeConnections = SelectedTransition.Connections.ToList();
                            var addConnections = removeConnections.Where(c => !x.Connections.Contains(c)).ToList();
                            var selectedTransition = SelectedTransition;
                            Action undo = () =>
                            {
                                foreach (var connection in addConnections)
                                {
                                    x.Disconnect(connection);
                                }
                                foreach (var connection in removeConnections)
                                {
                                    selectedTransition.ConnectTo(connection);
                                }
                            };
                            Action redo = () =>
                            {
                                foreach (var connection in removeConnections)
                                {
                                    selectedTransition.Disconnect(connection);
                                }
                                foreach (var connection in addConnections)
                                {
                                    x.ConnectTo(connection);
                                }
                            };
                            m_parent.Changed.Execute(new GenericUndoAction(undo, redo, "Moved links"));
                        }
                    };

                    if (Control.ModifierKeys.HasFlag(Keys.Shift))
                        action(m_parent.BestConnector(client, a => SelectedTransition.Connections.All(c => m_parent.CanConnect(a, c))));
                    m_parent.ForClickedOn(client, x => { }, action, (a, b) => { }, x => { }, () => { });
                    m_parent.RefreshDisplay();
                    m_parent.m_state = new State.Nothing(m_parent, null);
                }
            }
            public class ConnectionSelected : State
            {
                public readonly UnordererTuple2<Output> SelectedConnection;
                public ConnectionSelected(MouseController<TNode> parent, UnordererTuple2<Output> selectedConnection)
                    : base(parent)
                {
                    SelectedConnection = selectedConnection;
                }

                public override UndoAction Delete()
                {
                    Output a = SelectedConnection.Item1;
                    Output b = SelectedConnection.Item2;
                    Action undo = () => { a.ConnectTo(b); };
                    Action redo = () => { a.Disconnect(b); };
                    return new GenericUndoAction(undo, redo, "Removed connection");
                }

                public override void Draw(Graphics g, ConnectionDrawer connections)
                {
                    connections.Add(Util.Center(m_parent.UIInfo(SelectedConnection.Item1).Area), Util.Center(m_parent.UIInfo(SelectedConnection.Item2).Area), true);
                }
            }

            public virtual bool If(Action<Nothing> n) { return false; }

            public virtual UndoAction Delete() { return null; }

            public virtual void MouseMove(PointF client, Point screen) { }

            public virtual void LeftMouseUp(Point client, Point screen) { }

            public virtual void Draw(Graphics g, ConnectionDrawer connections) { }

            public State(MouseController<TNode> parent)
            {
                m_parent = parent;
            }

            public virtual void MiddleMouseUp(Point client, Point screen) { }

            public virtual Cursor Cursor { get { return Cursors.Default; } }
        }

        public enum ResizeState
        {
            L, R, T, B, TL, TR, BL, BR,
        }

        State m_innerState;
        public State m_state { get { return m_innerState; } set { m_innerState = value; StateChanged.Execute(); } }
        public event Action StateChanged;
        NodeSet m_selection = new NodeSet();

        public IReadonlyNodeSet Selected { get { return m_selection; } }
        public event Action SelectionChanged { add { m_selection.Changed += value; } remove { m_selection.Changed -= value; } }

        private TNode m_hoverNode = default(TNode);
        /// <summary>
        /// The topmost node the mouse is currently on top of
        /// </summary>
        public TNode HoverNode
        {
            get { return m_hoverNode; }
            set
            {
                if (!object.Equals(m_hoverNode, value))
                {
                    m_hoverNode = value;
                    HoverNodeChanged.Execute();
                }
            }
        }
        public event Action HoverNodeChanged;

        public Output SelectedTransition //TODO: Can probably be moved into states
        {
            get;
            private set;
        }

        public bool DraggingLinks
        {
            get
            {
                return m_state is State.DraggingLinks;
            }
        }

        public void SetSelection(IEnumerable<ID<NodeTemp>> nodes, IEnumerable<NodeGroup> groups)
        {
            m_selection.Clear();
            foreach (var node in nodes)
                m_selection.Add(node);
            foreach (var group in groups)
                m_selection.Add(group);
        }

        private bool CanConnect(Output a, Output b)
        {
            return a.Rules.CanConnect(a.m_definition.Id, b.m_definition.Id);
        }

        public void Draw(Graphics g, ConnectionDrawer connections)
        {
            m_state.Draw(g, connections);

        }

        private Output BestConnector(PointF p0, Func<Output, bool> filter)
        {
            var nodes = m_nodes();
            var connectors = nodes.SelectMany(n => n.Connectors);
            var filteredConnectors = connectors.Where(filter);
            var connectorsAndMetric = filteredConnectors.Select(c => new { Connector = c, Distance = (UIInfo(c).Area.Center().Take(p0)).LengthSquared() });
            var x = connectorsAndMetric.Best((a, b) => a.Distance < b.Distance).Connector;
            return x;
        }

        Action RefreshDisplay;
        Action<Point> Shift;
        Action<Point, float> Scale;
        Func<IEnumerable<TNode>> m_nodes; //Accessor to the set of nodes associated with the current file
        Func<IEnumerable<NodeGroup>> m_groups; //Accessor to the set of groups associated with the current file
        public event Action<UndoAction> Changed;
        private readonly Func<PointF, PointF> Snap;
        private readonly Func<IEditable, ConfigureResult> Edit;
        private readonly Action<TNode> RemoveNode;
        private readonly Func<ID<NodeTemp>, TNode> GetNode;
        public event Action<Point> PlainClick;
        public bool m_keyHeld;
        private readonly Func<Output, TransitionNoduleUIInfo> UIInfo;

        public MouseController(Action refreshDisplay, Action<Point> shift, Action<Point, float> scale, Func<IEnumerable<TNode>> nodes, Func<IEnumerable<NodeGroup>> groups, Func<IEditable, ConfigureResult> edit, Action<TNode> removeNode, Func<PointF, PointF> snap, Func<Output, TransitionNoduleUIInfo> uiInfo, Func<ID<NodeTemp>, TNode> getNode)
        {
            m_innerState = new State.Nothing(this, null);
            RefreshDisplay = refreshDisplay;
            Shift = shift;
            Scale = scale;
            m_nodes = nodes;
            m_groups = groups;
            RemoveNode = removeNode;
            Edit = edit;
            Snap = snap;
            UIInfo = uiInfo;
            GetNode = getNode;
        }

        /// <summary>
        /// If p is over a Node then execute nodeOp(n) where n is the topmost node overlapping p
        /// else if p is over a transition in node then execute transitionInOp(n) where n is the topmost transition in node overlapping p
        /// else if p is over a transition out node then execute transitionsOutOp(n) where n is the topmost transition out node overlapping p
        /// else execute otherwise()
        /// </summary>
        public void ForClickedOn(PointF p, Action<TNode> nodeOp, Action<Output> transitionOp, Action<Output, Output> connectionOp, Action<NodeGroup> groupOp, Action otherwise)
        {
            var nodes = m_nodes();
            TNode clicked = nodes.FirstOrDefault(n => n.Renderer.Area.Contains(p));
            if (clicked != null)
            {
                nodeOp(clicked);
                return;
            }

            var connector = nodes.SelectMany(n => n.Connectors).FirstOrDefault(t => UIInfo(t).Area.Contains(p));
            if (connector != null)
            {
                transitionOp(connector);
                return;
            }

            var groups = m_groups();
            var group = groups.FirstOrDefault(g => g.Renderer.Area.Contains(p));
            if (group != null)
            {
                groupOp(group);
                return;
            }

            var connectors = nodes.SelectMany(n => n.Connectors);
            HashSet<UnordererTuple2<Output>> connections = new HashSet<UnordererTuple2<Output>>(connectors.SelectMany(o => o.Connections.Select(c => UnordererTuple.Make(o, c))));
            foreach (var connection in connections)
            {
                Bezier b = LineDrawer.GetBezier(Util.Center(UIInfo(connection.Item1).Area), Util.Center(UIInfo(connection.Item2).Area));
                if (b.WithinDistance(p, 5))
                {
                    connectionOp(connection.Item1, connection.Item2);
                    return;
                }
            }

            otherwise();
        }

        public ResizeState? GetResizeOption(RectangleF r, PointF p)
        {
            var grow = new SizeF(1, 1);
            var leftArea = RectangleF.FromLTRB(r.Left, r.Top, r.Left + 5, r.Bottom);
            var rightArea = RectangleF.FromLTRB(r.Right - 5, r.Top, r.Right, r.Bottom);
            var topArea = RectangleF.FromLTRB(r.Left, r.Top, r.Right, r.Top + 5);
            var bottomArea = RectangleF.FromLTRB(r.Left, r.Bottom - 5, r.Right, r.Bottom);
            var topLeftArea = RectangleF.Intersect(topArea, leftArea);
            var topRightArea = RectangleF.Intersect(topArea, rightArea);
            var bottomLeftArea = RectangleF.Intersect(bottomArea, leftArea);
            var bottomRightArea = RectangleF.Intersect(bottomArea, rightArea);

            topLeftArea.Inflate(grow);
            topRightArea.Inflate(grow);
            bottomLeftArea.Inflate(grow);
            bottomRightArea.Inflate(grow);

            if (topLeftArea.Contains(p))
            {
                return ResizeState.TL;
            }
            else if (topRightArea.Contains(p))
            {
                return ResizeState.TR;
            }
            else if (bottomLeftArea.Contains(p))
            {
                return ResizeState.BL;
            }
            else if (bottomRightArea.Contains(p))
            {
                return ResizeState.BR;
            }
            else if (topArea.Contains(p))
            {
                return ResizeState.T;
            }
            else if (bottomArea.Contains(p))
            {
                return ResizeState.B;
            }
            else if (leftArea.Contains(p))
            {
                return ResizeState.L;
            }
            else if (rightArea.Contains(p))
            {
                return ResizeState.R;
            }
            else
                return null;
        }

        public void MouseDown(Point client, Point screen, MouseButtons button)
        {
            var ctrl = Control.ModifierKeys.HasFlag(Keys.Control);
            if (button == MouseButtons.Left)
            {
                Action<TNode> nodeOp = clicked =>
                    {
                        SelectedTransition = null;
                        if (m_selection.Nodes.Contains(clicked.Id) && ctrl)
                            m_selection.Remove(clicked.Id);
                        else
                        {
                            if (!m_selection.Nodes.Contains(clicked.Id))
                                if (!ctrl)
                                    m_selection.Clear();
                            m_selection.Add(clicked.Id);
                            m_state = new State.Dragging(this, clicked.Renderer.Area.Center(), client, clicked);
                        }
                    };

                Action<Output> transitionOp = transitionNode =>
                    {
                        if (SelectedTransition != transitionNode)
                        {
                            m_selection.Clear(); //TODO: What's this all about?
                            m_state = new State.Linking(this, transitionNode, client);
                        }
                        else
                        {
                            m_state = new State.DraggingLinks(this, transitionNode, client);
                        }
                    };

                Action<Output, Output> connectionOp = (c1, c2) =>
                {
                    SelectedTransition = null;
                    m_selection.Clear();
                    m_state = new State.ConnectionSelected(this, UnordererTuple.Make(c1, c2));
                };

                Action<NodeGroup> groupOp = group =>
                    {
                        SelectedTransition = null;
                        if (m_selection.Groups.Contains(group) && ctrl)
                        {
                            m_selection.Remove(group);
                        }
                        else
                        {
                            var totalArea = group.Renderer.Area;

                            if (!m_selection.Groups.Contains(group))
                                if (!ctrl)
                                    m_selection.Clear();
                            m_selection.Add(group);
                            foreach (var node in group.Contents)
                                m_selection.Add(node);

                            var resizeOption = GetResizeOption(totalArea, client);
                            if (resizeOption != null)
                            {
                                m_state = new State.Resizing(this, resizeOption.Value, group.Renderer.Area, group, UpdateNodesInGroup);
                            }
                            else
                            {
                                m_state = new State.Dragging(this, group.Renderer.Area.Center(), client, group);
                            }
                        }
                    };

                Action otherwise = () =>
                    {
                        SelectedTransition = null;
                        if (!ctrl)
                            m_selection.Clear();
                        if (m_keyHeld)
                            PlainClick.Execute(client);
                        else
                            m_state = new State.Netting(this, client, this);
                    };

                ForClickedOn(client, nodeOp, transitionOp, connectionOp, groupOp, otherwise);
            }
            else if (button == MouseButtons.Middle)
            {
                Action otherwise = () =>
                {
                    m_state = new State.Scrolling(this, screen);
                    Cursor.Current = Cursors.NoMove2D;
                };

                Action<TNode> nodeOp = clicked =>
                {
                    m_state = new State.Killing(this, client);
                };
                ForClickedOn(client, nodeOp, a => { otherwise(); }, (a, b) => { otherwise(); }, g => { }, otherwise);
            }
            else
            {
                m_selection.Clear();
            }
        }

        public void MouseUp(Point client, Point screen, MouseButtons button)
        {
            if (button == MouseButtons.Left)
            {
                m_state.LeftMouseUp(client, screen);
            }
            else if (button == MouseButtons.Middle)
            {
                m_state.MiddleMouseUp(client, screen);
            }
        }

        //Update the contents of the group so that all nodes physically inside the group are logically inside the group and the complement
        private void UpdateNodesInGroup(NodeGroup group)
        {
            HashSet<ID<NodeTemp>> toAdd = new HashSet<ID<NodeTemp>>();
            HashSet<ID<NodeTemp>> toRemove = new HashSet<ID<NodeTemp>>();
            foreach (var node in group.Contents.ToList().Where(n => !group.Renderer.Area.Contains(GetNode(n).Renderer.Area)))
            {
                group.Contents.Remove(node);
                toRemove.Add(node);
            }
            foreach (var node in m_nodes().Where(n => group.Renderer.Area.Contains(n.Renderer.Area)))
            {
                group.Contents.Add(node.Id);
                toAdd.Add(node.Id);
            }

            foreach (var node in toAdd)
                m_selection.Add(node);
            foreach (var node in toRemove)
                m_selection.Remove(node);

            RefreshDisplay();
        }

        public void MouseMove(PointF client, Point screen)
        {
            ForClickedOn(client, n => { HoverNode = n; }, t => { HoverNode = default(TNode); }, (a, b) => { HoverNode = default(TNode); }, g => { HoverNode = default(TNode); }, () => { HoverNode = default(TNode); });

            m_state.MouseMove(client, screen);

            RefreshDisplay();
        }

        public void MouseDoubleClick(Point p, MouseButtons button)
        {
            if (button == MouseButtons.Left)
            {
                var nodes = m_nodes();
                var clicked = nodes.FirstOrDefault(n => n.Renderer.Area.Contains(p));
                if (clicked != null)
                {
                    var result = clicked.Configure(Edit);
                    result.Do(sup =>
                    {
                        Action undo = () => { sup.Undo(); RefreshDisplay(); };
                        Action redo = () => { sup.Redo(); RefreshDisplay(); };
                        Changed.Execute(new GenericUndoAction(undo, redo, "Edited node"));
                    }, crno => { });
                }
            }
        }

        internal void MouseWheel(Point point, MouseEventArgs args, Keys modifiers)
        {
            if (args.Delta != 0)
            {
                if (modifiers.HasFlag(Keys.Control))
                {
                    Scale(point, (float)Math.Pow(1.1f, args.Delta / 120));
                }
                else
                {
                    const double WHEEL_SCALE = 0.25;
                    Shift(new Point(0, (int)(-args.Delta * WHEEL_SCALE)));
                }
            }
        }

        public SimpleUndoPair? Delete()
        {
            List<Action> redoActions = new List<Action>();
            List<Action> undoActions = new List<Action>();

            //TODO: Move this into state delete?
            if (SelectedTransition != null)
            {
                var actions = SelectedTransition.DisconnectAll();
                redoActions.Add(actions.Redo);
                undoActions.Add(actions.Undo);
            }
            var stateDelete = m_state.Delete();
            if (stateDelete != null)
            {
                redoActions.Add(stateDelete.Redo);
                undoActions.Add(stateDelete.Undo);
            }

            if (redoActions.Any())
                return new SimpleUndoPair() { Undo = () => { foreach (var undo in undoActions) undo(); }, Redo = () => { foreach (var redo in redoActions) redo(); } };
            else
                return null;
        }

        /// <summary>
        /// Reacts to a generic deletion operation by ensuring it's in a neutral state and clearing selections
        /// </summary>
        public UndoAction Deleted()
        {
            m_selection.Clear();
            SelectedTransition = null;
            m_state = new State.Nothing(this, null);
            return null;
        }
    }
}
