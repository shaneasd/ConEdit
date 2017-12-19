using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Conversation;
using Utilities;
using System.Diagnostics;
using System.Reflection;
using Conversation.Serialization;
using Utilities.UI;

namespace ConversationEditor
{
    using ConversationNode = ConversationNode<INodeGui>;
    using System.Globalization;
    using System.Collections.ObjectModel;

    internal partial class GraphEditorControl<TNode> : UserControl, IGraphEditorControl<TNode> where TNode : class, IRenderable<IGui>, IConversationNode, IConfigurable
    {
        public GraphEditorControl()
        {
            InitializeComponent();

            Colors = ColorScheme.Default;

            hScrollBar1.Scrolled += Redraw;
            vScrollBar1.Scrolled += Redraw;
            drawWindow.Paint += paintDrawWindow;
            zoomBar.Value = 1.0f;

            m_autoScrollTimer.Tick += (a, b) => ScrollIfRequired();
        }

        Timer m_autoScrollTimer = new Timer();

        protected override void OnBackColorChanged(EventArgs e)
        {
            BackColor = m_colorScheme.FormBackground;
        }

        IConversationEditorControlData<TNode, TransitionNoduleUIInfo> m_conversation = DummyConversationEditorControlData<TNode, TransitionNoduleUIInfo>.Instance;
        private IProject m_project;

        private void Redraw()
        {
            drawWindow.Invalidate(true);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public IConversationEditorControlData<TNode, TransitionNoduleUIInfo> CurrentFile
        {
            get { return m_conversation ?? DummyConversationEditorControlData<TNode, TransitionNoduleUIInfo>.Instance; }
            set
            {
                if (m_conversation != (value ?? DummyConversationEditorControlData<TNode, TransitionNoduleUIInfo>.Instance))
                {
                    m_conversation.File.Modified -= ResizeDocument;
                    m_conversation.File.Modified -= Redraw;
                    m_conversation.NodesDeleted -= OnNodesDeleted;
                    m_conversation.NodeAdded -= OnNodeAdded;
                    m_conversation.NodeRemoved -= OnNodeRemoved;

                    if (value != null)
                    {
                        value.File.Modified += ResizeDocument;
                        value.File.Modified += Redraw;
                        value.NodesDeleted += OnNodesDeleted;
                        value.NodeAdded += OnNodeAdded;
                        value.NodeRemoved += OnNodeRemoved;

                        m_conversation = value;
                    }
                    else
                    {
                        m_conversation = DummyConversationEditorControlData<TNode, TransitionNoduleUIInfo>.Instance;
                    }

                    OnNodesReset();
                    ResizeDocument();
                    Redraw();
                }
            }
        }

        private void NodeAreaChanged(WeakReference<TNode> nodeRef, Changed<RectangleF> c)
        {
            TNode n;
            if (nodeRef.TryGetTarget(out n))
            {
                bool removed = SpatiallyOrderedNodes.Remove(n, c.From);
                if (!removed)
                {
                    throw new InvalidOperationException("Something went wrong removing a node from the map in NodeAreaChanged");
                }
                SpatiallyOrderedNodes.Add(n, c.To);

                foreach (var connector in n.Data.Connectors)
                {
                    foreach (var connection in connector.Connections)
                    {
                        DisconnectionUpdate(connector, connection, c.From);
                    }
                }

                foreach (var connector in n.Data.Connectors)
                {
                    foreach (var connection in connector.Connections)
                    {
                        ConnectionUpdate(connection, connector, true);
                    }
                }
            }
        }

        public void OnNodesReset()
        {
            SpatiallyOrderedNodes = new QuadTree<TNode>(RectangleF.FromLTRB(0, 0, 2048, 2048));
            SpatiallyOrderedConnections = new QuadTree<Tuple<UnorderedTuple2<Output>, RectangleF>>(RectangleF.FromLTRB(0, 0, 2048, 2048));
            foreach (var list in m_connectionDeregisterActions.Values)
                foreach (var deregister in list)
                    deregister();
            m_connectionDeregisterActions.Clear();

            foreach (var node in CurrentFile.Nodes)
            {
                node.Renderer.UpdateArea();
            }

            foreach (var node in CurrentFile.Nodes)
            {
                SpatiallyOrderedNodes.Add(node, node.Renderer.Area);
                AddNodeMovedCallbacks(node);

                StoreConnections(node, true);
            }
        }

        private void AddNodeMovedCallbacks(TNode node)
        {
            WeakReference<TNode> nodeRef = new WeakReference<TNode>(node);
            Action<Changed<RectangleF>> areaChanged = c => NodeAreaChanged(nodeRef, c);
            if (m_nodeMovedCallbacks.ContainsKey(node))
                node.Renderer.AreaChanged -= m_nodeMovedCallbacks[node];
            node.Renderer.AreaChanged += areaChanged;
            m_nodeMovedCallbacks[node] = areaChanged;
            node.RendererChanging += () =>
            {
                node.Renderer.AreaChanged -= areaChanged;
                areaChanged(Changed.Create(node.Renderer.Area, Rectangle.Empty));
            };
            node.RendererChanged += () =>
            {
                node.Renderer.AreaChanged += areaChanged;
                areaChanged(Changed.Create(Rectangle.Empty, node.Renderer.Area));
            };
        }

        private void StoreConnections(TNode node, bool register)
        {
            if (!register)
            {
                if (m_connectionDeregisterActions.ContainsKey(node))
                    foreach (var deregister in m_connectionDeregisterActions[node])
                        deregister();
                return;
            }

            foreach (var connector in node.Data.Connectors)
            {
                var connectorTemp = connector;
                Action<Output, bool> connected = (connection, mustExist) =>
                {
                    ConnectionUpdate(connection, connectorTemp, mustExist);
                };
                connectorTemp.Connected += c => connected(c, true);
                connectorTemp.Disconnected += connection =>
                {
                    DisconnectionUpdate(connection, connectorTemp, UIInfo(connection).Area.Value);
                };

                foreach (var connection in connectorTemp.Connections)
                {
                    connected(connection, false);
                }

                Action deregister = UIInfo(connectorTemp).Area.Changed.Register(change =>
                {
                    foreach (var connection in connectorTemp.Connections)
                    {
                        var other = UIInfo(connection);
                        //The nature of the bezier splines means they will never reach outside the bounding rectangle which includes their endpoints
                        RectangleF fromBounds = RectangleF.Union(change.From, other.Area.Value);

                        var pair = UnorderedTuple.Make(connectorTemp, connection);
                        bool removed = SpatiallyOrderedConnections.Remove(Tuple.Create(pair, fromBounds), fromBounds);
                        if (!removed)
                        {
                            throw new InvalidOperationException("Something went wrong removing a connector from the map in deregister");
                        }

                        RectangleF toBounds = RectangleF.Union(change.To, other.Area.Value);
                        SpatiallyOrderedConnections.Add(Tuple.Create(pair, toBounds), toBounds);
                    }
                });
                if (!m_connectionDeregisterActions.ContainsKey(node))
                    m_connectionDeregisterActions[node] = new List<Action>();
                m_connectionDeregisterActions[node].Add(deregister);
            }
        }

        private void ConnectionUpdate(Output connection, Output connectorTemp, bool mustExist)
        {
            var ui1 = UIInfo(connectorTemp, false);
            var ui2 = UIInfo(connection, !mustExist);
            if (ui2 != null)
            {
                //The nature of the bezier splines means they will never reach outside the bounding rectangle which includes their endpoints
                RectangleF bounds = RectangleF.Union(ui1.Area.Value, ui2.Area.Value);

                var pair = UnorderedTuple.Make(connectorTemp, connection);
                bool exists = SpatiallyOrderedConnections.FindTouchingRegion(bounds).Contains(Tuple.Create(pair, bounds));
                if (!exists)
                {
                    SpatiallyOrderedConnections.Add(Tuple.Create(pair, bounds), bounds);
                }
                else
                {
                }
            }
        }

        private void DisconnectionUpdate(Output connection, Output connectorTemp, RectangleF connectorPosition)
        {
            var ui1 = UIInfo(connectorTemp);
            //The nature of the bezier splines means they will never reach outside the bounding rectangle which includes their endpoints
            RectangleF bounds = RectangleF.Union(ui1.Area.Value, connectorPosition);

            var pair = UnorderedTuple.Make(connectorTemp, connection);
            bool exists = SpatiallyOrderedConnections.FindTouchingRegion(bounds).Contains(Tuple.Create(pair, bounds));
            if (exists)
            {
                bool removed = SpatiallyOrderedConnections.Remove(Tuple.Create(pair, bounds), bounds);
                if (!removed)
                {
                    throw new InvalidOperationException("Something went wrong removing a connector from the map in connectorTemp.Disconnected");
                }
            }
        }

        public void OnNodeAdded(TNode node)
        {
            SpatiallyOrderedNodes.Add(node, node.Renderer.Area);
            AddNodeMovedCallbacks(node);
            StoreConnections(node, true);
        }

        Dictionary<TNode, Action<Changed<RectangleF>>> m_nodeMovedCallbacks = new Dictionary<TNode, Action<Changed<RectangleF>>>();

        public void OnNodeRemoved(TNode node)
        {
            StoreConnections(node, false);
            bool removed = SpatiallyOrderedNodes.Remove(node, node.Renderer.Area);
            if (!removed)
            {
                throw new InvalidOperationException("Something went wrong removing a node from the map in OnNodeRemoved");
            }
            node.Renderer.AreaChanged -= m_nodeMovedCallbacks[node];
            m_nodeMovedCallbacks.Remove(node);
        }

        QuadTree<TNode> SpatiallyOrderedNodes = new QuadTree<TNode>(RectangleF.FromLTRB(0, 0, 2048, 2048));
        QuadTree<Tuple<UnorderedTuple2<Output>, RectangleF>> SpatiallyOrderedConnections = new QuadTree<Tuple<UnorderedTuple2<Output>, RectangleF>>(RectangleF.FromLTRB(0, 0, 2048, 2048));
        Dictionary<TNode, List<Action>> m_connectionDeregisterActions = new Dictionary<TNode, List<Action>>();

        private void OnNodesDeleted()
        {
            m_mouseController.Deleted();
        }

        SizeF m_documentSize;
        SizeF DocumentSize
        {
            get { return m_documentSize; }
            set
            {
                m_documentSize = value;
                UpdateScrollbars();
            }
        }

        public void ResizeDocument()
        {
            //TODO: We can use the quad tree to more efficiently calculate (or cache) bounds
            IEnumerable<IRenderable<IGui>> nodes = CurrentFile.Nodes;
            IEnumerable<IRenderable<IGui>> groups = CurrentFile.Groups;
            RectangleF area = nodes.Concat(groups).Aggregate(RectangleF.Empty, (r, n) => RectangleF.Union(r, n.Renderer.Area));
            area = CurrentFile.Nodes.Aggregate(area, (r, n) => RectangleF.Union(r, n.Renderer.Area));
            area.Inflate(200, 200);
            DocumentSize = area.Size;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateScrollbars();
        }

        private void UpdateScrollbars()
        {
            hScrollBar1.PercentageCovered = drawWindow.Width / DocumentSize.Width / GraphScale;
            hScrollBar1.Maximum = Math.Max(0.0f, DocumentSize.Width - drawWindow.Width / GraphScale);
            vScrollBar1.PercentageCovered = drawWindow.Height / DocumentSize.Height / GraphScale;
            vScrollBar1.Maximum = Math.Max(0.0f, DocumentSize.Height - drawWindow.Height / GraphScale);

            var zoomBarValue = zoomBar.Value;
            zoomBar.Minimum = Math.Min(1.0f, Math.Max(0.1f, Math.Min(drawWindow.Size.Width / DocumentSize.Width, drawWindow.Size.Height / DocumentSize.Height)));
            zoomBar.Value = zoomBarValue;
        }

        IDataSource m_datasource = DummyDataSource.Instance;
        public IDataSource DataSource { get { return m_datasource; } }

        LocalizationEngine m_localization;

        internal void SetContext(IDataSource datasource, LocalizationEngine localization, IProject project, SharedContext context)
        {
            m_datasource = datasource;
            m_localization = localization;
            m_project = project;
            m_context = context;
        }

        public ContextMenu<TNode> m_contextMenu;
        public float GraphScale
        {
            get { return zoomBar.Value; }
            set
            {
                //if (drawWindow.Size.Width / Scale > DocumentSize.Width && drawWindow.Size.Height / Scale > DocumentSize.Height && value < Scale)
                //    return;
                zoomBar.Value = value;
            }
        }

        private void zoomBar_Scrolled()
        {
            UpdateScrollbars();
            Redraw();
        }

        private MouseController<TNode> m_mouseController;
        private Func<IConversationNodeData, AudioGenerationParameters, ConfigureResult> Edit;
        private CopyPasteController<TNode, TransitionNoduleUIInfo> m_copyPasteController;

        Matrix GetTransform()
        {
            Matrix result = new Matrix();
            try
            {
                result.Scale((float)GraphScale, (float)GraphScale);
                result.Translate(-hScrollBar1.Value, -vScrollBar1.Value);
            }
            catch
            {
                result.Dispose();
                throw;
            }
            return result;
        }

        public bool SnapToGrid { get; set; }
        private bool m_showGrid;
        public bool ShowGrid { get { return m_showGrid; } set { m_showGrid = value; Redraw(); } }

        private int m_minorGridSpacing;
        public int MinorGridSpacing { get { return m_minorGridSpacing; } set { m_minorGridSpacing = value; Redraw(); } }

        private int m_majorGridSpacing;
        public int MajorGridSpacing { get { return m_majorGridSpacing; } set { m_majorGridSpacing = value; Redraw(); } }

        public bool ShowIds { get; set; }
        private IColorScheme m_colorScheme;
        public IColorScheme Colors
        {
            get { return m_colorScheme; }
            set
            {
                m_colorScheme = value;
                BackColor = value.FormBackground;
                drawWindow.ColorScheme = value;
                hScrollBar1.ColorScheme = value;
                vScrollBar1.ColorScheme = value;
                zoomBar.ColorScheme = value;
                Redraw();
            }
        }

        /// <summary>
        /// Convert from screen space to graph space
        /// </summary>
        public Point ScreenToGraph(Point p)
        {
            return DrawWindowToGraphSpace(drawWindow.PointToClient(p));
        }

        public Point DrawWindowToGraphSpace(Point p)
        {
            Matrix transform = GetTransform();
            transform.Invert();
            return transform.TransformPoint(p);
        }

        public void SelectNode(TNode node)
        {
            //Shift(Point shift)
            hScrollBar1.Value = (node.Renderer.Area.Center().X - drawWindow.Width / 2.0f).Clamp(hScrollBar1.Minimum, hScrollBar1.Maximum);
            vScrollBar1.Value = (node.Renderer.Area.Center().Y - drawWindow.Height / 2.0f).Clamp(vScrollBar1.Minimum, vScrollBar1.Maximum);

            SetSelection(node.Only(), Enumerable.Empty<NodeGroup>());

            Redraw();
        }

        public void SetSelection(IEnumerable<TNode> nodes, IEnumerable<NodeGroup> groups)
        {
            m_mouseController.SetSelection(nodes.Select(n => n.Data.NodeId), groups);
        }

        public void SetSelection(IEnumerable<Id<NodeTemp>> nodes, IEnumerable<NodeGroup> groups)
        {
            m_mouseController.SetSelection(nodes, groups);
        }

        public IReadOnlyNodeSet Selected
        {
            get
            {
                return m_mouseController.Selected;
            }
        }

        public CopyPasteController<TNode, TransitionNoduleUIInfo> CopyPasteController { get { return m_copyPasteController; } }

        ToolTip m_toolTip = new ToolTip();

        Action<IEnumerable<IErrorListElement>> Log;

        internal void Initialise(Func<IConversationNodeData, AudioGenerationParameters, ConfigureResult> editNode, CopyPasteController<TNode, TransitionNoduleUIInfo> copyPasteController, Action<IEnumerable<IErrorListElement>> log)
        {
            Edit = editNode;
            m_copyPasteController = copyPasteController;
            Log = log;
            InitialiseMouseController();
            m_contextMenu = new ContextMenu<TNode>(Colors, m_mouseController, DrawWindowToGraphSpace, () => CurrentFile != DummyConversationEditorControlData<TNode, TransitionNoduleUIInfo>.Instance && !(CurrentFile is MissingConversationFile));
            m_contextMenu.AttachTo(drawWindow);

            m_toolTip.SetToolTip(drawWindow, null);
            m_toolTip.Popup += (object sender, PopupEventArgs e) => { this.ToolTipPopup(sender, e); };
            m_toolTip.Draw += (object sender, DrawToolTipEventArgs e) => { this.DrawToolTip(sender, e); };
        }

        public PointF Snap(PointF p)
        {
            if (SnapToGrid ^ Control.ModifierKeys.HasFlag(Keys.Shift))
            {
                float x = p.X / MinorGridSpacing;
                float y = p.Y / MinorGridSpacing;
                x = (float)Math.Round(x);
                y = (float)Math.Round(y);
                p.X = x * MinorGridSpacing;
                p.Y = y * MinorGridSpacing;
                return p;
            }
            else
            {
                return p;
            }
        }

        public PointF SnapGroup(PointF p)
        {
            if (SnapToGrid ^ Control.ModifierKeys.HasFlag(Keys.Shift))
            {
                float x = p.X / MinorGridSpacing;
                float y = p.Y / MinorGridSpacing;
                x = (float)Math.Round(x);
                y = (float)Math.Round(y);
                p.X = x * MinorGridSpacing;
                p.Y = y * MinorGridSpacing;
                return p;
            }
            else
            {
                return p;
            }
        }

        private void drawWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (m_mouseController.Selected.Nodes.Any() || m_mouseController.Selected.Groups.Any())
                {
                    //TODO: This would probably make more sense in MouseController.Delete
                    CurrentFile.Remove(m_mouseController.Selected.Nodes.Select(CurrentFile.GetNode), m_mouseController.Selected.Groups, m_localization);
                }
                else
                {
                    var delete = m_mouseController.Delete();
                    if (delete != null)
                    {
                        GenericUndoAction action = new GenericUndoAction(() => { delete.Value.Undo(); Redraw(); },
                                                                         () => { delete.Value.Redo(); Redraw(); },
                                                                         "Deleted");
                        CurrentFile.UndoableFile.Change(action);
                    }
                }
            }
            else if (e.KeyCode == Keys.Tab && e.Control)
            {
                int index = m_context.CurrentProject.Value.Localizer.LocalizationSets.IndexOf(m_context.CurrentLocalization.Value);
                if (index >= 0)
                {
                    m_context.CurrentLocalization.Value = m_context.CurrentProject.Value.Localizer.LocalizationSets.InfiniteRepeat().ElementAt(index + 1);
                    Redraw();
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_MOUSEHWHEEL = 0x20E;
            if (m.Msg == WM_MOUSEHWHEEL)
            {
                short delta = (short)(m.WParam.ToInt32() >> 16);
                const double WHEEL_SCALE = 0.25;
                Shift(new Point((int)(delta * WHEEL_SCALE), 0));
            }
            base.WndProc(ref m);
        }

        internal void Shift(Point shift)
        {
            hScrollBar1.Value = Utilities.Util.Clamp(hScrollBar1.Value + shift.X, hScrollBar1.Minimum, hScrollBar1.Maximum);
            vScrollBar1.Value = Utilities.Util.Clamp(vScrollBar1.Value + shift.Y, vScrollBar1.Minimum, vScrollBar1.Maximum);
        }

        Point m_autoScrollShift;
        private void ScrollIfRequired()
        {
            Shift(m_autoScrollShift);
        }

        internal void ScrollIfRequired(PointF? screenOrNull)
        {
            if (!screenOrNull.HasValue)
            {
                m_autoScrollShift = Point.Empty;
                m_autoScrollTimer.Stop();
                return;
            }

            var screen = screenOrNull.Value;
            float scrollLeft = Math.Max(0, 30 - Math.Abs(drawWindow.Left - screen.X));
            float scrollRight = Math.Max(0, 30 - Math.Abs(drawWindow.Right - screen.X));
            float scrollUp = Math.Max(0, 30 - Math.Abs(drawWindow.Top - screen.Y));
            float scrollDown = Math.Max(0, 30 - Math.Abs(drawWindow.Bottom - screen.Y));

            int x = (int)Math.Round(scrollRight - scrollLeft);
            int y = (int)Math.Round(scrollDown - scrollUp);
            if (x != 0 || y != 0)
            {
                m_autoScrollShift = new Point(x, y);
                m_autoScrollTimer.Start();
            }
            else
            {
                m_autoScrollShift = Point.Empty;
                m_autoScrollTimer.Stop();
            }

        }

        Dictionary<Keys, INodeDataGenerator> m_keyMapping = new Dictionary<Keys, INodeDataGenerator>();

        private ConfigureResult MyEdit(IConversationNodeData data)
        {
            return Edit(data, new AudioGenerationParameters(CurrentFile.File.File, m_project.File.File));
        }

        PopupEventHandler ToolTipPopup = (a, b) => { };
        DrawToolTipEventHandler DrawToolTip = (a, b) => { };

        private void InitialiseMouseController()
        {
            m_mouseController = new MouseController<TNode>(Colors, Redraw, shift => Shift(shift), (screen) => ScrollIfRequired(screen), (p, z) => Zoom(p, z), () => new ZOrderedQuadTree<TNode>(SpatiallyOrderedNodes, CurrentFile.RelativePosition), () => new Fake<UnorderedTuple2<Output>>(SpatiallyOrderedConnections.Select(a => a.Item1)), () => CurrentFile.Groups, MyEdit, n => CurrentFile.Remove(n.Only(), Enumerable.Empty<NodeGroup>(), m_localization), Snap, SnapGroup, UIInfo, id => CurrentFile.GetNode(id));
            drawWindow.MouseDown += (a, args) => m_mouseController.MouseDown(DrawWindowToGraphSpace(args.Location), args.Location, args.Button);
            drawWindow.MouseUp += (a, args) => m_mouseController.MouseUp(DrawWindowToGraphSpace(args.Location), args.Location, args.Button);
            drawWindow.MouseMove += (a, args) =>
            {
                m_mouseController.MouseMove(DrawWindowToGraphSpace(args.Location), args.Location);
            };
            drawWindow.MouseDoubleClick += (a, args) => m_mouseController.MouseDoubleClick(DrawWindowToGraphSpace(args.Location), args.Button);
            drawWindow.MouseWheel += (a, args) => m_mouseController.MouseWheel(DrawWindowToGraphSpace(args.Location), args, Control.ModifierKeys);
            drawWindow.MouseCaptureChanged += (a, args) => m_mouseController.MouseCaptureChanged();

            m_mouseController.Changed += (a) => { CurrentFile.UndoableFile.Change(a); ResizeDocument(); };
            m_mouseController.SelectionChanged += Redraw;

            m_mouseController.PlainClick += (p) =>
            {
                if (m_keyMapping[m_keyHeld] != null)
                    AddNode(m_keyMapping[m_keyHeld], p);
            };

            m_mouseController.SelectionChanged += () => m_conversation.BringToFront(Selected);

            drawWindow.KeyDown += (o, k) =>
            {
                m_mouseController.KeyHeld = m_keyMapping.ContainsKey(k.KeyCode) ? k.KeyCode : (Keys?)null;
                m_keyHeld = k.KeyCode;
                if (k.KeyCode.IsSet(Keys.ShiftKey))
                    Redraw();
            };
            drawWindow.KeyUp += (o, k) =>
            {
                m_mouseController.KeyHeld = null;
                if (k.KeyCode.IsSet(Keys.ShiftKey))
                    Redraw();
            };
            drawWindow.LostFocus += (o, e) => m_mouseController.KeyHeld = null;

            m_mouseController.StateChanged += () =>
            {
                drawWindow.Cursor = m_mouseController.m_state.Cursor;
            };

            m_mouseController.HoverNodeChanged += () =>
            {
                if (m_mouseController.HoverNode != null)
                {
                    if (ShowIds)
                    {
                        m_toolTip.Active = true;
                        m_toolTip.SetToolTip(drawWindow, m_mouseController.HoverNode.Data.NodeId.Serialized());
                        m_toolTip.OwnerDraw = false;
                    }
                    else
                    {
                        m_toolTip.OwnerDraw = true;
                        var node = m_mouseController.HoverNode as ConversationNode<INodeGui>;
                        EditableUI renderer = new EditableUI(node, new PointF(0, 0), m_localization.Localize);
                        renderer.UpdateArea();
                        ToolTipPopup = (sender, e) => { e.ToolTipSize = renderer.Area.Size.ToSize(); };
                        DrawToolTip = (sender, e) =>
                        {
                            using (var m = new Matrix())
                            {
                                m.Translate(renderer.Area.Width / 2, renderer.Area.Height / 2);
                                e.Graphics.Transform = m;
                            }
                            renderer.Draw(e.Graphics, false, m_colorScheme);
                        };
                        m_toolTip.SetToolTip(drawWindow, m_mouseController.HoverNode.Data.NodeId.Serialized());
                        m_toolTip.Active = true;
                    }
                }
                else
                {
                    m_toolTip.Active = false;
                    m_toolTip.SetToolTip(drawWindow, null);
                }
            };
        }

        private void Zoom(Point graphPoint, float z)
        {
            Matrix originalTransform = GetTransform();
            var originalWindowPoint = originalTransform.TransformPoint(graphPoint);

            GraphScale *= z;

            //Adjust translation so the part of the graph under the mouse pointer remains stationary
            Matrix zoomedTransform = GetTransform();
            var newWindowPoint = zoomedTransform.TransformPoint(graphPoint);
            hScrollBar1.Value += (newWindowPoint.X - originalWindowPoint.X) / GraphScale;
            vScrollBar1.Value += (newWindowPoint.Y - originalWindowPoint.Y) / GraphScale;
        }

        public void UpdateKeyMappings()
        {
            m_keyMapping = new Dictionary<Keys, INodeDataGenerator>();
            foreach (var node in DataSource.AllNodes())
            {
                string shortcutKeys;
                if (ShortcutKey.TryGet(node.Config, out shortcutKeys))
                {
                    foreach (char key in shortcutKeys)
                    {
                        Keys k;
                        if (Enum.TryParse("" + key, out k) || (Enum.TryParse(("" + key).ToUpper(CultureInfo.InvariantCulture), out k)))
                            m_keyMapping[k] = node;
                    }
                }

                //Mouse controller is going to want m_keyHeld set when these are held but it will handle them itself
                m_keyMapping[Keys.Left] = null;
                m_keyMapping[Keys.Right] = null;
                m_keyMapping[Keys.Up] = null;
                m_keyMapping[Keys.Down] = null;
            }
        }

        Keys m_keyHeld = Keys.None;
        private SharedContext m_context;

        public void DrawGrid(PaintEventArgs e)
        {
            if (ShowGrid)
            {
                var mi = GetTransform().Inverse();

                PointF lowerBound = mi.TransformPoint(new PointF(0, 0));
                PointF upperBound = mi.TransformPoint(new PointF(drawWindow.Size.Width, drawWindow.Size.Height));

                using (var pen = new Pen(m_colorScheme.MinorGrid))
                    DrawGrid(e, pen, MinorGridSpacing, lowerBound, upperBound);
                using (var pen = new Pen(m_colorScheme.Grid))
                    DrawGrid(e, pen, MajorGridSpacing, lowerBound, upperBound);
            }
        }

        private void DrawGrid(PaintEventArgs e, Pen pen, int gridSpacing, PointF lowerBound, PointF upperBound)
        {
            if (gridSpacing * GraphScale >= 4)
            {
                List<PointF> pointList = new List<PointF>();

                for (float i = (float)Math.Floor(lowerBound.X / gridSpacing) * gridSpacing; i < upperBound.X; i += gridSpacing)
                {
                    pointList.Add(new PointF(i, lowerBound.Y - gridSpacing));
                    pointList.Add(new PointF(i, upperBound.Y + gridSpacing));
                }

                for (float i = (float)Math.Floor(lowerBound.Y / gridSpacing) * gridSpacing; i < upperBound.Y; i += gridSpacing)
                {
                    pointList.Add(new PointF(lowerBound.X - gridSpacing, i));
                    pointList.Add(new PointF(upperBound.X + gridSpacing, i));
                }

                PointF[] points = pointList.ToArray();
                Matrix transform = GetTransform();
                transform.TransformPoints(points); //Apply the transform ourselves so the lines don't get stretched
                for (int i = 0; i < points.Length; i += 2)
                {
                    e.Graphics.DrawLine(pen, points[i], points[i + 1]);
                }
            }
        }

        private void paintDrawWindow(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            Matrix transform = GetTransform();

            var tl = transform.Inverse().TransformPoint(new PointF(0, 0));
            var br = transform.Inverse().TransformPoint(new PointF(Width, Height));
            RectangleF bounds = RectangleF.FromLTRB(tl.X, tl.Y, br.X, br.Y);

            using (g.Clip = new Region(new RectangleF(0, 0, DocumentSize.Width * GraphScale, DocumentSize.Height * GraphScale)))
            {
                var originalState = g.Save();

                DrawGrid(e);

                g.Transform = transform;

                if (CurrentFile != null)
                {
                    //var orderedUnselectedNodes = CurrentFile.Nodes.Reverse().Where(n => !m_mouseController.Selected.Nodes.Contains(n.Data.NodeId));
                    //var orderedSelectedNodes = CurrentFile.Nodes.Reverse().Where(n => m_mouseController.Selected.Nodes.Contains(n.Data.NodeId));

                    var tree = new ZOrderedQuadTree<TNode>(SpatiallyOrderedNodes, CurrentFile.RelativePosition);
                    var nodes = tree.FindTouchingRegion(bounds).Reverse();
                    var orderedUnselectedNodes = nodes.Where(n => !m_mouseController.Selected.Nodes.Contains(n.Data.NodeId));
                    var orderedSelectedNodes = nodes.Where(n => m_mouseController.Selected.Nodes.Contains(n.Data.NodeId));

                    //Make sure the nodes are the right size so the connectors end up in the right place
                    foreach (var node in CurrentFile.Nodes)
                        node.Renderer.UpdateArea();

                    HashSet<UnorderedTuple2<PointF>> unselectedNodeConnections = new HashSet<UnorderedTuple2<PointF>>();
                    HashSet<UnorderedTuple2<PointF>> selectedNodeConnections = new HashSet<UnorderedTuple2<PointF>>();
                    HashSet<UnorderedTuple2<PointF>> selectedConnections = new HashSet<UnorderedTuple2<PointF>>();

                    foreach (var node in orderedUnselectedNodes)
                    {
                        foreach (var t in node.Data.Connectors)
                        {
                            foreach (var connection in t.Connections)
                            {
                                PointF p1 = UIInfo(t).Area.Value.Center();
                                PointF p2 = UIInfo(connection).Area.Value.Center();
                                var pair = UnorderedTuple.Make(p1, p2);
                                bool selected = m_mouseController.IsSelected(connection) || m_mouseController.IsSelected(t);

                                if (selected)
                                {
                                    unselectedNodeConnections.Remove(pair);
                                    if (!m_mouseController.DraggingLinks) //If we're dragging this connection around then let the mouse controller render it
                                        selectedConnections.Add(pair);
                                }
                                else
                                    unselectedNodeConnections.Add(pair);
                            }
                        }
                    }

                    foreach (var node in orderedSelectedNodes)
                    {
                        foreach (var t in node.Data.Connectors)
                        {
                            foreach (var connection in t.Connections)
                            {
                                PointF p1 = UIInfo(t).Area.Value.Center();
                                PointF p2 = UIInfo(connection).Area.Value.Center();
                                var pair = UnorderedTuple.Make(p1, p2);
                                bool selected = m_mouseController.IsSelected(connection) || m_mouseController.IsSelected(t);

                                unselectedNodeConnections.Remove(pair);

                                if (selected)
                                {
                                    selectedNodeConnections.Remove(pair);
                                    if (!m_mouseController.DraggingLinks) //If we're dragging this connection around then let the mouse controller render it
                                        selectedConnections.Add(pair);
                                }
                                else
                                    selectedNodeConnections.Add(pair);
                            }
                        }
                    }

                    //Draw all the groups
                    foreach (NodeGroup group in CurrentFile.Groups.Reverse())
                    {
                        group.Renderer.Draw(g, m_mouseController.Selected.Groups.Contains(group), m_colorScheme);
                    }

                    //Draw all the connections for unselected nodes
                    using (ConnectionDrawer connections = new ConnectionDrawer(this.Colors))
                    {
                        foreach (var connection in unselectedNodeConnections)
                            connections.Add(connection.Item1, connection.Item2, false);
                        connections.Draw(g);
                    }

                    //Draw all the unselected nodes
                    foreach (TNode node in orderedUnselectedNodes)
                    {
                        node.Renderer.Draw(g, m_mouseController.Selected.Nodes.Contains(node.Data.NodeId), m_colorScheme);
                        foreach (var t in node.Data.Connectors)
                        {
                            bool selected = m_mouseController.IsSelected(t);
                            UIInfo(t).Draw(g, selected ? Colors.Foreground : Colors.Connectors);
                        }
                    }

                    //Draw all the connections for selected nodes
                    using (ConnectionDrawer connections = new ConnectionDrawer(this.Colors))
                    {
                        foreach (var connection in selectedNodeConnections)
                            connections.Add(connection.Item1, connection.Item2, false);
                        connections.Draw(g);
                    }

                    //Draw all the selected nodes
                    foreach (TNode node in orderedSelectedNodes)
                    {
                        node.Renderer.Draw(g, m_mouseController.Selected.Nodes.Contains(node.Data.NodeId), m_colorScheme);
                        foreach (var t in node.Data.Connectors)
                        {
                            bool selected = m_mouseController.IsSelected(t);
                            UIInfo(t).Draw(g, selected ? Colors.Foreground : Colors.Connectors);
                        }
                    }

                    //Draw all the selected connections
                    using (ConnectionDrawer connections = new ConnectionDrawer(this.Colors))
                    {
                        foreach (var connection in selectedConnections)
                            connections.Add(connection.Item1, connection.Item2, true);
                        connections.Draw(g);
                    }

                    //Draw any dynamic connections, etc
                    if (m_mouseController != null)
                    {
                        using (var connections = new ConnectionDrawer(this.Colors))
                        {
                            m_mouseController.Draw(g, connections);
                            connections.Draw(g);
                        }
                    }
                }

                g.DrawRectangle(Colors.ControlBorder, RectangleF.FromLTRB(0, 0, DocumentSize.Width - 1, DocumentSize.Height - 1));

                g.Restore(originalState);

                g.DrawRectangle(Colors.ControlBorder, Rectangle.FromLTRB(0, 0, drawWindow.Width - 1, drawWindow.Height - 1));
            }
        }

        public void AddNode(INodeDataGenerator node, Point p)
        {
            if (CurrentFile.File.Exists)
            {
                TNode g = CurrentFile.MakeNode(node.Generate(Id<NodeTemp>.New(), new List<NodeDataGeneratorParameterData>(), CurrentFile), new NodeUIData(p));
                Action addNode = () => { CurrentFile.Add(g.Only(), Enumerable.Empty<NodeGroup>(), m_localization); };
                g.Configure(MyEdit).Do
                (
                    //Configure the node and then add it. We need to configure the node before adding so that it is in a valid state
                    //when added. Failure to do so results in localized string parameters having a null value meaning their undo
                    //behavior can't be set up correctly.
                    simpleUndoPair => { simpleUndoPair.Redo(); addNode(); },
                    resultNotOk =>
                    {
                        if (resultNotOk == ConfigureResult.Cancel)
                        {
                            //Merge the parameter into a junk source so it doesn't count towards the real source
                            foreach (var parameter in g.Data.Parameters.OfType<IDynamicEnumParameter>())
                                parameter.MergeInto(new DynamicEnumParameter.Source());
                        }
                        else
                            addNode();  //Add the node if the user didn't cancel (i.e. no editing was required)
                    }
                );
            }
        }

        public void RefreshContextMenu(IEnumerable<IMenuActionFactory<TNode>> menuFactories)
        {
            if (m_contextMenu == null)
                return;

            List<MenuAction<TNode>> custom = new List<MenuAction<TNode>>();

            foreach (var factory in menuFactories)
            {
                custom.AddRange(factory.GetMenuActions(this, m_project, Log, m_localization));
            }

            m_contextMenu.ResetCustomNodes(custom.ToArray());
        }

        public void GroupSelection()
        {
            if (Selected.Nodes.Any())
            {
                NodeGroup newGroup = NodeGroup.Make(Selected.Nodes.Select(CurrentFile.GetNode));
                CurrentFile.Add(Enumerable.Empty<TNode>(), newGroup.Only(), m_localization);
                SetSelection(Selected.Nodes.Evaluate(), Selected.Groups.Concat(newGroup.Only()).Evaluate());

                Redraw();
            }
        }

        public void UngroupSelection()
        {
            if (Selected.Groups.Any())
            {
                CurrentFile.Remove(Enumerable.Empty<TNode>(), Selected.Groups, m_localization);
                SetSelection(Selected.Nodes.Evaluate(), Enumerable.Empty<NodeGroup>());

                Redraw();
            }
        }

        public void DuplicateSelection()
        {
            if (Selected.Nodes.Any() || Selected.Groups.Any())
            {
                var duplicates = m_copyPasteController.Duplicate(Selected.Nodes.Select(CurrentFile.GetNode), Selected.Groups, DataSource);
                var nodesAndGroups = CurrentFile.DuplicateInto(duplicates.Item1, duplicates.Item2, duplicates.Item4, duplicates.Item3, m_localization);
                SetSelection(nodesAndGroups.Item1, nodesAndGroups.Item2);
                Redraw();
            }
        }

        public void SelectAll()
        {
            //Can't pass in the lazy collections as selecting these nodes brings them to the front
            //which changes the order of the elements corrupting the enumerator
            SetSelection(CurrentFile.Nodes.Evaluate(), CurrentFile.Groups.Evaluate());
        }

        public void CopySelection()
        {
            m_copyPasteController.Copy(Selected.Nodes.Select(CurrentFile.GetNode), Selected.Groups);
        }

        public void Paste(Point? p)
        {
            var additions = m_copyPasteController.Paste(DataSource);

            Insert(p, additions);
        }

        public void Insert(Point? p, Tuple<IEnumerable<GraphAndUI<NodeUIData>>, IEnumerable<NodeGroup>, object> additions)
        {
            Point loc = p ?? DrawWindowToGraphSpace(new Point(Width / 2, Height / 2));
            var nodesAndGroups = CurrentFile.DuplicateInto(additions.Item1, additions.Item2, additions.Item3, loc, m_localization);
            SetSelection(nodesAndGroups.Item1, nodesAndGroups.Item2);
        }

        public Control AsControl()
        {
            return this;
        }

        private TransitionNoduleUIInfo UIInfo(Output connection, bool canFail = false)
        {
            return CurrentFile.UIInfo(connection, canFail);
        }
    }

    /// <summary>
    /// To get rid of the generic parameter to make the designer happy
    /// </summary>
    internal class ConversationEditorControl : GraphEditorControl<ConversationNode>
    { }
}
