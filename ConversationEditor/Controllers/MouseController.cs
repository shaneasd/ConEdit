using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using Utilities;
using Conversation;

namespace ConversationEditor
{
    public class MouseController<TNode> where TNode : IRenderable<IGui>, IConversationNode, IConfigurable
    {
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
                            m_controller.m_selection.Add(node.Data.NodeId);
                    }
                    m_parent.SetStateToNothingOrSelectingDirection(null, null);
                }

                public override void Draw(Graphics g, ConnectionDrawer connections)
                {
                    g.FillRectangle(m_parent.m_scheme.Hatch, SelectionRectangle());
                }

                public override void MouseMove(PointF client, Point screen)
                {
                    m_lastClientPos = client;
                    m_parent.ScrollIfRequired(screen);
                }
            }
            public class Dragging : State
            {
                public PointF MoveOrigin { get; }
                public PointF LastClientPos { get; private set; }
                IRenderable<IGui> DragTarget { get; }

                public Dragging(MouseController<TNode> parent, PointF moveOrigin, PointF client, IRenderable<IGui> dragTarget)
                    : base(parent)
                {
                    MoveOrigin = moveOrigin;
                    LastClientPos = client;
                    DragTarget = dragTarget;
                }

                public override void MouseMove(PointF client, Point screen)
                {
                    PointF offset = client.Take(LastClientPos); //Amount the mouse has moved (since we last actually moved the nodes)
                    var selectionOrigin = DragTarget.Renderer.Area.Center();
                    var newPosF = DragTarget is NodeGroup
                                  ? selectionOrigin.Plus(m_parent.SnapGroup(offset))
                                  : m_parent.Snap(selectionOrigin.Plus(offset));
                    offset = newPosF.Take(selectionOrigin); //The actual offset applied

                    foreach (var a in m_parent.m_selection.Renderable(id => m_parent.GetNode(id)))
                        a.Renderer.Offset(offset);
                    LastClientPos = LastClientPos.Plus(offset);
                    m_parent.ScrollIfRequired(screen);
                }

                public override void LeftMouseUp(Point client, Point screen)
                {
                    //ChangingState() handles the actual logic for saving the node movement
                    m_parent.SetStateToNothingOrSelectingDirection(null, null);
                }

                internal override void ChangingState()
                {
                    var offset = MoveOrigin.Take(DragTarget.Renderer.Area.Center());

                    if (!offset.IsEmpty)
                    {
                        m_parent.MoveSelected(offset);
                    }
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
                    if (object.Equals(null, inConnector) || object.Equals(outConnector, null))
                        return;
                    if (!outConnector.Connections.Contains(inConnector))
                    {
                        Action redo = () => { outConnector.ConnectTo(inConnector, false); };
                        Action undo = () => { outConnector.Disconnect(inConnector); };
                        m_parent.Changed.Execute(new GenericUndoAction(undo, redo, "Connected nodes"));
                    }
                }

                public override void MouseMove(PointF client, Point screen)
                {
                    m_lastClientPos = client;
                    m_parent.ScrollIfRequired(screen);
                }

                public override void LeftMouseUp(Point client, Point screen)
                {
                    Output selectTransition = null;
                    if (!Control.ModifierKeys.HasFlag(Keys.Shift))
                    {
                        m_parent.ForClickedOn(client,
                            x => { },
                            x =>
                            {
                                if (x != LinkingTransition)
                                    Connect(LinkingTransition, x);
                                else
                                    selectTransition = LinkingTransition;
                                LinkingTransition = null;
                            },
                            (a, b) => { },
                            x => { },
                            () => { });
                    }
                    else
                    {
                        var x = m_parent.BestConnector(m_parent.UIInfo(LinkingTransition).Area.Value.Center(), client, a => a.CanConnectTo(LinkingTransition, ConnectionConsiderations.None));
                        if (x != LinkingTransition)
                            Connect(LinkingTransition, x);
                        else
                            selectTransition = LinkingTransition;
                        LinkingTransition = null;
                    }
                    m_parent.SetStateToNothingOrSelectingDirection(null, selectTransition);

                    m_parent.RefreshDisplay();
                }

                public override void Draw(Graphics g, ConnectionDrawer connections)
                {
                    //Diagnostic rendering of the isosurface generated by the FancyDistance metric
                    //for (int i = 0; i < 640; i++)
                    //    for (int j = 0; j < 480; j++)
                    //    {
                    //        //var dist = m_parent.FancyDistance(null, m_lastClientPos, new PointF(i, j));
                    //        var dist = FancyDistance(m_parent.UIInfo(LinkingTransition).Area.Center(), m_lastClientPos, new PointF(i, j));
                    //        dist = (float)Math.Sqrt(dist);
                    //        int r = (int)(dist * 5 % 256);
                    //        //if (dist > 255)
                    //        //    r = 255;
                    //        g.FillRectangle(new SolidBrush(Color.FromArgb(r, 0, 0)), Rectangle.FromLTRB(i, j, i + 1, j + 1));
                    //    }
                    if (Control.ModifierKeys.HasFlag(Keys.Shift))
                    {
                        var p0 = m_lastClientPos;
                        Output x = m_parent.BestConnector(m_parent.UIInfo(LinkingTransition).Area.Value.Center(), p0, a => a.CanConnectTo(LinkingTransition, ConnectionConsiderations.None));
                        if (x != null)
                            connections.Add(m_parent.UIInfo(x).Area.Value.Center(), m_parent.UIInfo(LinkingTransition).Area.Value.Center(), true);
                    }
                    else
                    {
                        connections.Add(m_lastClientPos, m_parent.UIInfo(LinkingTransition).Area.Value.Center(), true);
                    }
                }
            }
            public class Scrolling : State
            {
                Point m_lastScreenPos;
                private readonly Output m_selectedTransition;
                public Scrolling(MouseController<TNode> parent, Point lastScreenPos, Output selectedTransition)
                    : base(parent)
                {
                    m_lastScreenPos = lastScreenPos;
                    m_selectedTransition = selectedTransition;
                }

                public override void MouseMove(PointF client, Point screen)
                {
                    var shift = m_lastScreenPos.Take(screen);
                    m_parent.Shift(shift);
                    m_lastScreenPos = screen;
                }

                public override void MiddleMouseUp(Point client, Point screen)
                {
                    m_parent.SetStateToNothingOrSelectingDirection(null, m_selectedTransition);
                }

                public override SimpleUndoPair? Delete()
                {
                    if (SelectedTransition != null)
                    {
                        return SelectedTransition.DisconnectAllActions();
                    }
                    else
                        return null;
                }

                public override Output SelectedTransition { get { return m_selectedTransition; } }
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
                        if (m_parent.RemoveNode(x))
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

                    m_parent.SetStateToNothingOrSelectingDirection(null, null);
                }
            }
            public class Nothing : State
            {
                private readonly Output m_selectedTransition;
                public Nothing(MouseController<TNode> parent, ResizeState? resizeState, Output selectedTransition)
                    : base(parent)
                {
                    ResizeState = resizeState;
                    m_selectedTransition = selectedTransition;
                }
                public ResizeState? ResizeState { get; }

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
                        var newResizeState = MouseController<TNode>.GetResizeOption(totalArea, client);
                        if (newResizeState != ResizeState)
                            m_parent.SetStateToNothingOrSelectingDirection(newResizeState, m_selectedTransition);
                    };
                    Action clearState = () =>
                    {
                        if (null != ResizeState)
                            m_parent.SetStateToNothingOrSelectingDirection(null, m_selectedTransition);
                    };
                    m_parent.ForClickedOn(client, n => { clearState(); }, t => { clearState(); }, (a, b) => { clearState(); }, groupOp, clearState);
                }

                public override SimpleUndoPair? Delete()
                {
                    if (SelectedTransition != null)
                    {
                        return SelectedTransition.DisconnectAllActions();
                    }
                    else
                        return null;
                }

                public override Output SelectedTransition { get { return m_selectedTransition; } }
            }
            public class Resizing : State
            {
                public Resizing(MouseController<TNode> parent, ResizeState resizeState, RectangleF resizeOriginalArea, NodeGroup group, Action<NodeGroup> updateNodesInGroup)
                    : base(parent)
                {
                    ResizeState = resizeState;
                    ResizeOriginalArea = resizeOriginalArea;
                    Group = group;
                    UpdateNodesInGroup = () => updateNodesInGroup(group);
                }
                public ResizeState ResizeState { get; }
                public RectangleF ResizeOriginalArea { get; }
                public NodeGroup Group { get; }
                public Action UpdateNodesInGroup { get; }

                public override void LeftMouseUp(Point client, Point screen)
                {
                    var newArea = Group.Renderer.Area;
                    var oldArea = ResizeOriginalArea;

                    UpdateNodesInGroup();

                    Action undo = () => { Group.Renderer.Area = oldArea; UpdateNodesInGroup(); };
                    Action redo = () => { Group.Renderer.Area = newArea; UpdateNodesInGroup(); };

                    m_parent.Changed.Execute(new GenericUndoAction(undo, redo, "Resized group"));
                    m_parent.SetStateToNothingOrSelectingDirection(null, null);
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
                    m_parent.ScrollIfRequired(screen);
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
                PointF m_lastClientPos;

                public DraggingLinks(MouseController<TNode> parent, Output selectedTransition, PointF client)
                    : base(parent)
                {
                    SelectedTransition = selectedTransition;
                    m_lastClientPos = client;
                }

                public override void Draw(Graphics g, ConnectionDrawer connections)
                {
                    if (Control.ModifierKeys.HasFlag(Keys.Shift))
                    {
                        var p0 = m_lastClientPos;
                        Output x = m_parent.BestConnector(null, p0, a => SelectedTransition.Connections.All(b => a.CanConnectTo(b, ConnectionConsiderations.RedundantConnection)));

                        if (x != null)
                            foreach (Output connection in SelectedTransition.Connections)
                            {
                                connections.Add(m_parent.UIInfo(x).Area.Value.Center(), m_parent.UIInfo(connection).Area.Value.Center(), true);
                            }
                    }
                    else
                    {
                        foreach (Output connection in SelectedTransition.Connections)
                        {
                            connections.Add(Util.Center(m_parent.UIInfo(connection).Area.Value), m_lastClientPos, true);
                        }
                    }
                }

                public override void MouseMove(PointF client, Point screen)
                {
                    m_lastClientPos = client;
                    m_parent.ScrollIfRequired(screen);
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
                                    selectedTransition.ConnectTo(connection, true); //This is an undo action so we don't need to recheck the rules
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
                                    x.ConnectTo(connection, false);
                                }
                            };
                            m_parent.Changed.Execute(new GenericUndoAction(undo, redo, "Moved links"));
                        }
                    };

                    if (Control.ModifierKeys.HasFlag(Keys.Shift))
                    {
                        var best = m_parent.BestConnector(null, client, a => SelectedTransition.Connections.All(c => a.CanConnectTo(c, ConnectionConsiderations.RedundantConnection)));
                        action(best);
                    }
                    m_parent.ForClickedOn(client, x => { }, action, (a, b) => { }, x => { }, () => { });
                    m_parent.RefreshDisplay();
                    m_parent.SetStateToNothingOrSelectingDirection(null, null);
                }

                public override SimpleUndoPair? Delete()
                {
                    if (SelectedTransition != null)
                    {
                        return SelectedTransition.DisconnectAllActions();
                    }
                    else
                        return null;
                }

                public override Output SelectedTransition { get; }
            }
            public class ConnectionSelected : State
            {
                public UnorderedTuple2<Output> SelectedConnection { get; }
                public ConnectionSelected(MouseController<TNode> parent, UnorderedTuple2<Output> selectedConnection)
                    : base(parent)
                {
                    SelectedConnection = selectedConnection;
                }

                public override SimpleUndoPair? Delete()
                {
                    Output a = SelectedConnection.Item1;
                    Output b = SelectedConnection.Item2;
                    return new SimpleUndoPair
                    {
                        Undo = () => { a.ConnectTo(b, true); }, //Reverting to a previous state so we can ignore the connection rules
                        Redo = () => { a.Disconnect(b); }
                    };
                }

                public override void Draw(Graphics g, ConnectionDrawer connections)
                {
                    connections.Add(Util.Center(m_parent.UIInfo(SelectedConnection.Item1).Area.Value), Util.Center(m_parent.UIInfo(SelectedConnection.Item2).Area.Value), true);
                }
            }
            public class SelectingDirection : State
            {
                private Point m_direction;

                public SelectingDirection(MouseController<TNode> parent, Point direction) : base(parent)
                {
                    m_direction = direction;
                }

                public override void LeftMouseUp(Point client, Point screen)
                {
                    m_parent.SelectAllNodesAndGroupsRelativeToPoint(client, m_direction);
                }

                public override Cursor Cursor
                {
                    get
                    {
                        if (m_direction.X < 0)
                        {
                            if (m_direction.Y > 0)
                                return Cursors.PanSW;
                            else if (m_direction.Y < 0)
                                return Cursors.PanNW;
                            else
                                return Cursors.PanWest;
                        }
                        else if (m_direction.X > 0)
                        {
                            if (m_direction.Y > 0)
                                return Cursors.PanSE;
                            else if (m_direction.Y < 0)
                                return Cursors.PanNE;
                            else
                                return Cursors.PanEast;
                        }
                        else
                        {
                            if (m_direction.Y < 0)
                                return Cursors.PanNorth;
                            else
                                return Cursors.PanSouth;
                            //We ignore the (0,0) case as it shouldn't occur
                        }
                    }
                }
            }

            public virtual bool If(Action<Nothing> n) { return false; }

            public virtual SimpleUndoPair? Delete() { return null; }

            public virtual void MouseMove(PointF client, Point screen) { }

            public virtual void LeftMouseUp(Point client, Point screen) { }

            public virtual void Draw(Graphics g, ConnectionDrawer connections) { }

            public State(MouseController<TNode> parent)
            {
                m_parent = parent;
            }

            public virtual void MiddleMouseUp(Point client, Point screen) { }

            public virtual Cursor Cursor { get { return Cursors.Default; } }

            public virtual Output SelectedTransition { get { return null; } }

            internal bool IsSelected(Output connector)
            {
                return SelectedTransition == connector;
            }

            internal virtual void ChangingState()
            {
            }
        }

        public void MoveSelected(PointF offset)
        {
            var currentSelection = m_selection.Clone();
            List<Action> undoList = new List<Action>();
            List<Action> redoList = new List<Action>();
            foreach (var a in currentSelection.Renderable(id => GetNode(id)))
            {
                var aa = a;
                undoList.Add(() => { aa.Renderer.Offset(offset); });
                var currentPos = aa.Renderer.Area.Center();
                redoList.Add(() => { aa.Renderer.MoveTo(currentPos); });
            }

            var groups = m_groups();
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

                    var caught = groups.FirstOrDefault(g => g.Renderer.Area.Contains(GetNode(n).Renderer.Area));
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
            Changed.Execute(new GenericUndoAction(undo, redo, "Dragged " + currentSelection.Count() + " things"));
        }

        private void SelectAllNodesAndGroupsRelativeToPoint(Point p, Point dir)
        {
            var groups = m_groups().Where(g =>
            {
                var groupArea = g.Renderer.Area;
                var groupCenter = groupArea.Center();
                var xError = p.X - groupCenter.X;
                var yError = p.Y - groupCenter.Y;
                return (xError - groupArea.Width / 2) * dir.X <= 0 &&
                       (xError + groupArea.Width / 2) * dir.X <= 0 &&
                       (yError - groupArea.Height / 2) * dir.Y <= 0 &&
                       (yError + groupArea.Height / 2) * dir.Y <= 0;
            }).ToList();
            var nodes = m_nodes().Where(n =>
            {
                if (groups.Any(g => g.Contents.Contains(n.Data.NodeId)))
                    return false;
                var groupArea = n.Renderer.Area;
                var groupCenter = groupArea.Center();
                var xError = p.X - groupCenter.X;
                var yError = p.Y - groupCenter.Y;
                return (xError - groupArea.Width / 2) * dir.X <= 0 &&
                       (xError + groupArea.Width / 2) * dir.X <= 0 &&
                       (yError - groupArea.Height / 2) * dir.Y <= 0 &&
                       (yError + groupArea.Height / 2) * dir.Y <= 0;
            }).ToList();
            SetSelection(nodes.Select(n => n.Data.NodeId), groups);
        }

        public enum ResizeState
        {
            L, R, T, B, TL, TR, BL, BR,
        }

        State m_innerState;
        public State m_state { get { return m_innerState; } set { m_innerState.ChangingState(); m_innerState = value; StateChanged.Execute(); } }
        public event Action StateChanged;
        NodeSet m_selection = new NodeSet();

        public IReadOnlyNodeSet Selected { get { return m_selection; } }
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

        public bool DraggingLinks
        {
            get
            {
                return m_state is State.DraggingLinks;
            }
        }

        public void SetSelection(IEnumerable<Id<NodeTemp>> nodes, IEnumerable<NodeGroup> groups)
        {
            m_selection.Clear();
            foreach (var node in nodes)
                m_selection.Add(node);
            foreach (var group in groups)
                m_selection.Add(group);
        }

        public void Draw(Graphics g, ConnectionDrawer connections)
        {
            m_state.Draw(g, connections);
        }

        /// <param name="from">null if there is no single linking connector (e.g. when dragging several links)</param>
        public static float FancyDistance(PointF? from, PointF p0, PointF test)
        {
            if (from == null)
            {
                //This is the cartesian distance from 'p0' to 'test'. No need to squareroot as A^2 > B^2 implies sqrt(A^2) > sqrt(B^2)
                return test.Take(p0).LengthSquared();
            }
            else
            {
                //express in new orthonormal basis {X, Y} where X is the normalized vector from 'from' to 'p0'
                //and where the origin is at 'from'
                PointF X = (p0.Take(from.Value)).Normalised();
                PointF Y = new PointF(-X.Y, X.X);
                // test = xX + yY
                var translatedTest = test.Take(p0);
                var x = translatedTest.Dot(X);
                var y = translatedTest.Dot(Y);
                //The cartesian distance would be sqrt(x^2+y^2) however we would like to bias the metric such that test points in
                //the 'correct' direction are given a lower score (and thus are more likely to be picked).
                //Thus we scale x down by this empirically selected scaling factor before calculating distance.
                //However we only scale down x if it is not in the wrong direction. If x<0 then it is in the wrong direction and its
                //value is unchanged.
                //Again the square root is not required as we're only interested in comparing distances not taking their difference.
                if (x > 0)
                    x *= 0.5f;
                return x * x + y * y;
            }
        }

        private Output BestConnector(PointF? from, PointF p0, Func<Output, bool> filter)
        {
            var nodes = m_nodes();
            var connectors = nodes.SelectMany(n => n.Data.Connectors);
            var filteredConnectors = connectors.Where(filter);
            if (filteredConnectors.Any())
            {
                var connectorsAndMetric = filteredConnectors.Select(c => new { Connector = c, Distance = FancyDistance(from, p0, (UIInfo(c).Area.Value.Center())) });
                var x = connectorsAndMetric.Best((a, b) => a.Distance < b.Distance).Connector;
                return x;
            }
            else
            {
                return null;
            }
        }

        Action RefreshDisplay;
        Action<Point> Shift;
        Action<PointF?> ScrollIfRequired;
        Action<Point, float> Scale;
        Func<IReadOnlyQuadTree<TNode>> m_nodes; //Accessor to the set of nodes associated with the current file
        Func<IReadOnlyQuadTree<UnorderedTuple2<Output>>> m_connections;
        Func<IEnumerable<NodeGroup>> m_groups; //Accessor to the set of groups associated with the current file
        public event Action<UndoAction> Changed;
        private readonly Func<PointF, PointF> Snap;
        private readonly Func<PointF, PointF> SnapGroup;
        private readonly NodeEditOperation Edit;
        private readonly Func<TNode, bool> RemoveNode;
        private readonly Func<Id<NodeTemp>, TNode> GetNode;
        public event Action<Point> PlainClick;

        void SetStateToNothingOrSelectingDirection(ResizeState? resizeState, Output selectedTransition)
        {
            ScrollIfRequired(null);
            if (m_keyHeld == Keys.Left)
                m_state = new State.SelectingDirection(this, new Point(-1, 0));
            else if (m_keyHeld == Keys.Right)
                m_state = new State.SelectingDirection(this, new Point(1, 0));
            else if (m_keyHeld == Keys.Up)
                m_state = new State.SelectingDirection(this, new Point(0, -1));
            else if (m_keyHeld == Keys.Down)
                m_state = new State.SelectingDirection(this, new Point(0, 1));
            else if (!m_state.If(n => { }))
            {
                m_state = new State.Nothing(this, resizeState, selectedTransition);
            }
        }

        private Keys? m_keyHeld;
        public Keys? KeyHeld
        {
            get { return m_keyHeld; }
            set
            {
                m_keyHeld = value;
                if (m_state is State.Nothing || m_state is State.SelectingDirection)
                    SetStateToNothingOrSelectingDirection(null, m_state.SelectedTransition);
            }
        }
        private readonly Func<Output, bool, TransitionNoduleUIInfo> m_UIInfo;
        private TransitionNoduleUIInfo UIInfo(Output output, bool canFail) { return m_UIInfo(output, canFail); }
        private TransitionNoduleUIInfo UIInfo(Output output) { return m_UIInfo(output, false); }
        IColorScheme m_scheme;

        public MouseController(IColorScheme scheme, Action refreshDisplay, Action<Point> shift, Action<PointF?> scrollIfRequired, Action<Point, float> scale, Func<IReadOnlyQuadTree<TNode>> nodes, Func<IReadOnlyQuadTree<UnorderedTuple2<Output>>> connections, Func<IEnumerable<NodeGroup>> groups, NodeEditOperation edit, Func<TNode, bool> removeNode, Func<PointF, PointF> snap, Func<PointF, PointF> snapGroup, Func<Output, bool, TransitionNoduleUIInfo> uiInfo, Func<Id<NodeTemp>, TNode> getNode)
        {
            m_scheme = scheme;
            m_innerState = new State.Nothing(this, null, null);
            RefreshDisplay = refreshDisplay;
            Shift = shift;
            ScrollIfRequired = scrollIfRequired;
            Scale = scale;
            m_nodes = nodes;
            m_connections = connections;
            m_groups = groups;
            RemoveNode = removeNode;
            Edit = edit;
            Snap = snap;
            SnapGroup = snapGroup;
            m_UIInfo = uiInfo;
            GetNode = getNode;
        }

        /// <summary>
        /// If p is over a Node then execute nodeOp(n) where n is the topmost node overlapping p
        /// else if p is over a connector then execute transitionOp(n) where n is the topmost connector overlapping p
        /// else if p is over a connection then execute connectionOp(n) where n is the topmost connection overlapping p
        /// else if p is over a ground then execute groupOp(n) where n is the topmost group overlapping p
        /// else execute otherwise()
        /// </summary>
        public void ForClickedOn(PointF p, Action<TNode> nodeOp, Action<Output> transitionOp, Action<Output, Output> connectionOp, Action<NodeGroup> groupOp, Action otherwise)
        {
            var nodes = m_nodes();
            //Make the search rectangle large enough to capture nodes which we haven't clicked on but whose connectors we might have
            RectangleF searchRectangle = RectangleF.FromLTRB(p.X - 20, p.Y - 20, p.X + 20, p.Y + 20);//TODO: Making a big assumption here about the nature of connectors
            var nearbyNodes = nodes.FindTouchingRegion(searchRectangle).ToArray();
            TNode clicked = nearbyNodes.FirstOrDefault(n => n.Renderer.Area.Contains(p));

            if (clicked != null)
            {
                nodeOp(clicked);
                return;
            }

            var connector = nearbyNodes.SelectMany(n => n.Data.Connectors).FirstOrDefault(t => UIInfo(t).Area.Value.Contains(p));
            if (connector != null)
            {
                transitionOp(connector);
                return;
            }

            var connections = m_connections();
            foreach (var connection in connections.FindTouchingRegion(searchRectangle))
            {
                Bezier b = LineDrawer.GetBezier(Util.Center(UIInfo(connection.Item1).Area.Value), Util.Center(UIInfo(connection.Item2).Area.Value));
                if (b.WithinDistance(p, 5))
                {
                    connectionOp(connection.Item1, connection.Item2);
                    return;
                }
            }

            var groups = m_groups();
            var group = groups.FirstOrDefault(g => g.Renderer.Area.Contains(p));
            if (group != null)
            {
                groupOp(group);
                return;
            }

            otherwise();
        }

        public static ResizeState? GetResizeOption(RectangleF r, PointF p)
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
                        if (m_selection.Nodes.Contains(clicked.Data.NodeId) && ctrl)
                            m_selection.Remove(clicked.Data.NodeId);
                        else
                        {
                            if (!m_selection.Nodes.Contains(clicked.Data.NodeId))
                                if (!ctrl)
                                    m_selection.Clear();
                            m_selection.Add(clicked.Data.NodeId);
                            m_state = new State.Dragging(this, clicked.Renderer.Area.Center(), client, clicked);
                        }
                    };

                Action<Output> transitionOp = transitionNode =>
                    {
                        if (m_state.SelectedTransition != transitionNode || !m_state.SelectedTransition.Connections.Any())
                        {
                            m_selection.Clear(); //While we're linking nodes there's no selected nodes/groups
                            m_state = new State.Linking(this, transitionNode, client);
                        }
                        else
                        {
                            m_state = new State.DraggingLinks(this, transitionNode, client);
                        }
                    };

                Action<Output, Output> connectionOp = (c1, c2) =>
                {
                    m_selection.Clear();
                    m_state = new State.ConnectionSelected(this, UnorderedTuple.Make(c1, c2));
                };

                Action<NodeGroup> groupOp = group =>
                    {
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
                        if (!ctrl)
                            m_selection.Clear();
                        if (KeyHeld.HasValue)
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
                    m_state = new State.Scrolling(this, screen, m_state.SelectedTransition);
                    Cursor.Current = Cursors.NoMove2D;
                };

                Action<TNode> nodeOp = clicked =>
                {
                    m_state = new State.Killing(this, client);
                };
                ForClickedOn(client, nodeOp, a => { otherwise(); }, (a, b) => { otherwise(); }, g => { }, otherwise);
            }
            //If you deselect on right click you can't select a group and then right click them.
            //You also can't add a 'move selection' action to an undo queue before changing State because the selection will be cleared first
            //else
            //{
            //    m_selection.Clear();
            //}
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
            HashSet<Id<NodeTemp>> toAdd = new HashSet<Id<NodeTemp>>();
            HashSet<Id<NodeTemp>> toRemove = new HashSet<Id<NodeTemp>>();
            foreach (var node in group.Contents.ToList().Where(n => !group.Renderer.Area.Contains(GetNode(n).Renderer.Area)))
            {
                group.Contents.Remove(node);
                toRemove.Add(node);
            }
            foreach (var node in m_nodes().Where(n => group.Renderer.Area.Contains(n.Renderer.Area)))
            {
                group.Contents.Add(node.Data.NodeId);
                toAdd.Add(node.Data.NodeId);
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

            var stateDelete = m_state.Delete();
            if (stateDelete != null)
            {
                redoActions.Add(stateDelete.Value.Redo);
                undoActions.Add(stateDelete.Value.Undo);
            }

            SetStateToNothingOrSelectingDirection(null, null);

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
            SetStateToNothingOrSelectingDirection(null, null);
            return null;
        }

        internal void MouseCaptureChanged()
        {
            if (!(m_state is State.ConnectionSelected))
                SetStateToNothingOrSelectingDirection(null, m_state.SelectedTransition);
        }

        internal bool IsSelected(Output connector)
        {
            return m_state.IsSelected(connector);
        }
    }
}
