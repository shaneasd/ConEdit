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
using ConversationEditor.Controllers;
using System.Reflection;
using Conversation.Serialization;

namespace ConversationEditor
{
    using ConversationNode = ConversationNode<INodeGUI>;

    public interface IGraphEditorControl<TNode> where TNode : IRenderable<IGUI>
    {
        void CopySelection();

        void DuplicateSelection();

        void SelectAll();

        void Paste(Point? point);

        void UngroupSelection();

        void GroupSelection();

        Control AsControl();

        void SelectNode(TNode node);
    }

    public interface IConversationEditorControlData<TNode, TTransitionUI> : ISaveableFileUndoableProvider where TNode : IRenderable<IGUI>
    {
        TNode GetNode(ID<NodeTemp> id);
        Tuple<IEnumerable<TNode>, IEnumerable<NodeGroup>> DuplicateInto(IEnumerable<GraphAndUI<NodeUIData>> nodeData, IEnumerable<NodeGroup> groups, PointF location, LocalizationEngine localization);
        void Add(IEnumerable<TNode> nodes, IEnumerable<NodeGroup> groups);
        bool Remove(IEnumerable<TNode> nodes, IEnumerable<NodeGroup> groups);
        IEnumerableReversible<TNode> Nodes { get; }
        IEnumerableReversible<NodeGroup> Groups { get; }
        void RemoveLinks(Output o);
        /// <summary>
        /// Issues that were detected in deserialization that can be automatically resolved but with possible loss of data
        /// e.g. removing links that point to non-existent nodes
        /// </summary>
        List<Error> Errors { get; }
        void ClearErrors();

        void BringToFront(IReadonlyNodeSet Selected);

        TTransitionUI UIInfo(Output connection);

        event Action NodesDeleted;

        TNode MakeNode(IEditable e, NodeUIData uiData);
    }

    public partial class GraphEditorControl<TNode> : UserControl, IGraphEditorControl<TNode> where TNode : class, IRenderable<IGUI>, IConversationNode, IConfigurable
    {
        public GraphEditorControl()
        {
            InitializeComponent();

            Colors = new ColorScheme();

            BackColor = ColorScheme.FormBackground;
            drawWindow.BackColor = ColorScheme.Background;

            hScrollBar1.Scrolled += Redraw;
            vScrollBar1.Scrolled += Redraw;
            drawWindow.Paint += paintDrawWindow;
            zoomBar.Value = 1.0f;
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            BackColor = ColorScheme.FormBackground;
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
                if (m_conversation == value)
                    return;
                else
                {
                    if (m_conversation != (value ?? DummyConversationEditorControlData<TNode, TransitionNoduleUIInfo>.Instance))
                    {
                        m_conversation.File.Modified -= ResizeDocument;
                        m_conversation.File.Modified -= Redraw;
                        if (value != null)
                        {
                            value.File.Modified += ResizeDocument;
                            value.File.Modified += Redraw;
                            m_conversation.NodesDeleted -= OnNodesDeleted;
                            value.NodesDeleted += OnNodesDeleted;
                            m_conversation = value;
                        }
                        else
                        {
                            m_conversation = DummyConversationEditorControlData<TNode, TransitionNoduleUIInfo>.Instance;
                        }
                        ResizeDocument();
                        Redraw();
                    }
                }
            }
        }

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
            IEnumerable<IRenderable<IGUI>> nodes = CurrentFile.Nodes;
            IEnumerable<IRenderable<IGUI>> groups = CurrentFile.Groups;
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

        internal void SetContext(IDataSource datasource, LocalizationEngine localization, IProject project)
        {
            m_datasource = datasource;
            m_localization = localization;
            m_project = project;
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
        private Func<IEditable, AudioGenerationParameters, ConfigureResult> Edit;
        private Action<TNode> FileReferences;
        private INodeFactory<TNode> m_nodeFactory;
        private CopyPasteController<TNode, TransitionNoduleUIInfo> m_copyPasteController;

        Matrix GetTransform()
        {
            Matrix result = new Matrix();
            result.Scale((float)GraphScale, (float)GraphScale);
            result.Translate(-hScrollBar1.Value, -vScrollBar1.Value);
            return result;
        }

        public bool SnapToGrid { get; set; }
        private bool m_showGrid;
        public bool ShowGrid { get { return m_showGrid; } set { m_showGrid = value; Redraw(); } }

        private uint m_minorGridSpacing;
        public uint MinorGridSpacing { get { return m_minorGridSpacing; } set { m_minorGridSpacing = value; Redraw(); } }

        private uint m_majorGridSpacing;
        public uint MajorGridSpacing { get { return m_majorGridSpacing; } set { m_majorGridSpacing = value; Redraw(); } }

        public bool ShowIDs { get; set; }
        private ColorScheme m_colorScheme;
        public ColorScheme Colors { get { return m_colorScheme; } set { m_colorScheme = value; Redraw(); } }

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
            m_mouseController.SetSelection(nodes.Select(n => n.Id), groups);
        }

        public void SetSelection(IEnumerable<ID<NodeTemp>> nodes, IEnumerable<NodeGroup> groups)
        {
            m_mouseController.SetSelection(nodes, groups);
        }

        public IReadonlyNodeSet Selected
        {
            get
            {
                return m_mouseController.Selected;
            }
        }

        public CopyPasteController<TNode, TransitionNoduleUIInfo> CopyPasteController { get { return m_copyPasteController; } }

        ToolTip m_toolTip = new ToolTip();

        internal void Initialise(Func<IEditable, AudioGenerationParameters, ConfigureResult> editNode, INodeFactory<TNode> nodeFactory, CopyPasteController<TNode, TransitionNoduleUIInfo> copyPasteController, Action<TNode> findReferences)
        {
            Edit = editNode;
            m_nodeFactory = nodeFactory;
            m_copyPasteController = copyPasteController;
            FileReferences = findReferences;
            InitialiseMouseController();
            m_contextMenu = new ContextMenu<TNode>(m_mouseController, DrawWindowToGraphSpace, () => CurrentFile != DummyConversationEditorControlData<TNode, TransitionNoduleUIInfo>.Instance && !(CurrentFile is MissingConversationFile));
            m_contextMenu.AttachTo(drawWindow);

            m_toolTip.SetToolTip(drawWindow, null);
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

        private void drawWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (m_mouseController.Selected.Nodes.Any() || m_mouseController.Selected.Groups.Any())
                {
                    //TODO: This would probably make more sense in MouseController.Delete
                    CurrentFile.Remove(m_mouseController.Selected.Nodes.Select(CurrentFile.GetNode), m_mouseController.Selected.Groups);
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

        Dictionary<Keys, EditableGenerator> m_keyMapping = new Dictionary<Keys, EditableGenerator>();

        private ConfigureResult MyEdit(IEditable data)
        {
            return Edit(data, new AudioGenerationParameters(CurrentFile, m_project));
        }
        private void InitialiseMouseController()
        {
            m_mouseController = new MouseController<TNode>(Redraw, shift => Shift(shift), (p, z) => Zoom(p, z), () => CurrentFile.Nodes, () => CurrentFile.Groups, MyEdit, n => CurrentFile.Remove(n.Only(), Enumerable.Empty<NodeGroup>()), Snap, UIInfo, id => CurrentFile.GetNode(id));
            drawWindow.MouseDown += (a, args) => m_mouseController.MouseDown(DrawWindowToGraphSpace(args.Location), args.Location, args.Button);
            drawWindow.MouseUp += (a, args) => m_mouseController.MouseUp(DrawWindowToGraphSpace(args.Location), args.Location, args.Button);
            drawWindow.MouseMove += (a, args) => m_mouseController.MouseMove(DrawWindowToGraphSpace(args.Location), args.Location);
            drawWindow.MouseDoubleClick += (a, args) => m_mouseController.MouseDoubleClick(DrawWindowToGraphSpace(args.Location), args.Button);
            drawWindow.MouseWheel += (a, args) => m_mouseController.MouseWheel(DrawWindowToGraphSpace(args.Location), args, Control.ModifierKeys);
            drawWindow.MouseCaptureChanged += (a, args) => m_mouseController.MouseCaptureChanged();

            m_mouseController.Changed += (a) => { CurrentFile.UndoableFile.Change(a); ResizeDocument(); };
            m_mouseController.SelectionChanged += Redraw;

            m_mouseController.PlainClick += (p) =>
            {
                if (m_keyMapping.ContainsKey(m_keyHeld))
                    AddNode(m_keyMapping[m_keyHeld], p);
            };

            m_mouseController.SelectionChanged += () => m_conversation.BringToFront(Selected);

            drawWindow.KeyDown += (o, k) =>
            {
                m_mouseController.m_keyHeld = m_keyMapping.ContainsKey(k.KeyCode);
                m_keyHeld = k.KeyCode;
                if (k.KeyCode.IsSet(Keys.ShiftKey))
                    Redraw();
            };
            drawWindow.KeyUp += (o, k) =>
            {
                m_mouseController.m_keyHeld = false;
                if (k.KeyCode.IsSet(Keys.ShiftKey))
                    Redraw();
            };
            drawWindow.LostFocus += (o, e) => m_mouseController.m_keyHeld = false;

            m_mouseController.StateChanged += () =>
            {
                drawWindow.Cursor = m_mouseController.m_state.Cursor;
            };

            m_mouseController.HoverNodeChanged += () =>
            {
                if (m_mouseController.HoverNode != null && ShowIDs)
                {
                    m_toolTip.Active = true;
                    m_toolTip.SetToolTip(drawWindow, m_mouseController.HoverNode.Id.Serialized());
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
            m_keyMapping = new Dictionary<Keys, EditableGenerator>();
            foreach (var node in DataSource.AllNodes())
            {
                string shortcutKeys = "";
                if (ShortcutKey.TryGet(node.Config, ref shortcutKeys))
                {
                    foreach (char key in shortcutKeys)
                    {
                        Keys k;
                        if (!Enum.TryParse("" + key, out k))
                            if (!Enum.TryParse(("" + key).ToUpper(), out k))
                                throw new Exception("Don't understand shortcut " + key);
                        m_keyMapping[k] = node;
                    }
                }
            }
        }

        Keys m_keyHeld = Keys.None;

        public void DrawGrid(PaintEventArgs e)
        {
            if (ShowGrid)
            {
                var mi = GetTransform().Inverse();

                PointF lowerBound = mi.TransformPoint(new PointF(0, 0));
                PointF upperBound = mi.TransformPoint(new PointF(drawWindow.Size.Width, drawWindow.Size.Height));

                using (var pen = new Pen(ColorScheme.MinorGrid))
                    DrawGrid(e, pen, MinorGridSpacing, lowerBound, upperBound);
                using (var pen = new Pen(ColorScheme.Grid))
                    DrawGrid(e, pen, MajorGridSpacing, lowerBound, upperBound);
            }
        }

        private void DrawGrid(PaintEventArgs e, Pen pen, uint GRID_SPACING, PointF lowerBound, PointF upperBound)
        {
            if (GRID_SPACING * GraphScale >= 4)
            {
                List<PointF> pointList = new List<PointF>();

                for (float i = (float)Math.Floor(lowerBound.X / GRID_SPACING) * GRID_SPACING; i < upperBound.X; i += GRID_SPACING)
                {
                    pointList.Add(new PointF(i, lowerBound.Y - GRID_SPACING));
                    pointList.Add(new PointF(i, upperBound.Y + GRID_SPACING));
                }

                for (float i = (float)Math.Floor(lowerBound.Y / GRID_SPACING) * GRID_SPACING; i < upperBound.Y; i += GRID_SPACING)
                {
                    pointList.Add(new PointF(lowerBound.X - GRID_SPACING, i));
                    pointList.Add(new PointF(upperBound.X + GRID_SPACING, i));
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

            using (g.Clip = new Region(new RectangleF(0, 0, DocumentSize.Width * GraphScale, DocumentSize.Height * GraphScale)))
            {
                var originalState = g.Save();

                DrawGrid(e);

                g.Transform = GetTransform();

                if (CurrentFile != null)
                {
                    var orderedUnselectedNodes = CurrentFile.Nodes.Reverse().Where(n => !m_mouseController.Selected.Nodes.Contains(n.Id));
                    var orderedSelectedNodes = CurrentFile.Nodes.Reverse().Where(n => m_mouseController.Selected.Nodes.Contains(n.Id));

                    //Make sure the nodes are the right size so the connectors end up in the right place
                    foreach (var node in CurrentFile.Nodes)
                        node.Renderer.UpdateArea();

                    HashSet<UnordererTuple2<PointF>> unselectedNodeConnections = new HashSet<UnordererTuple2<PointF>>();
                    HashSet<UnordererTuple2<PointF>> selectedNodeConnections = new HashSet<UnordererTuple2<PointF>>();
                    HashSet<UnordererTuple2<PointF>> selectedConnections = new HashSet<UnordererTuple2<PointF>>();

                    foreach (var node in orderedUnselectedNodes)
                    {
                        foreach (var t in node.Connectors)
                        {
                            foreach (var connection in t.Connections)
                            {
                                PointF p1 = UIInfo(t).Area.Center();
                                PointF p2 = UIInfo(connection).Area.Center();
                                var pair = UnordererTuple.Make(p1, p2);
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
                        foreach (var t in node.Connectors)
                        {
                            foreach (var connection in t.Connections)
                            {
                                PointF p1 = UIInfo(t).Area.Center();
                                PointF p2 = UIInfo(connection).Area.Center();
                                var pair = UnordererTuple.Make(p1, p2);
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
                        group.Renderer.Draw(g, m_mouseController.Selected.Groups.Contains(group));
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
                        node.Renderer.Draw(g, m_mouseController.Selected.Nodes.Contains(node.Id));
                        foreach (var t in node.Connectors)
                        {
                            bool selected = m_mouseController.IsSelected(t);
                            UIInfo(t).Draw(g, selected ? ColorScheme.Foreground : Colors.Connectors);
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
                        node.Renderer.Draw(g, m_mouseController.Selected.Nodes.Contains(node.Id));
                        foreach (var t in node.Connectors)
                        {
                            bool selected = m_mouseController.IsSelected(t);
                            UIInfo(t).Draw(g, selected ? ColorScheme.Foreground : Colors.Connectors);
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

                g.DrawRectangle(ColorScheme.ControlBorder, RectangleF.FromLTRB(0, 0, DocumentSize.Width - 1, DocumentSize.Height - 1));

                g.Restore(originalState);

                g.DrawRectangle(ColorScheme.ControlBorder, Rectangle.FromLTRB(0, 0, drawWindow.Width - 1, drawWindow.Height - 1));
            }
        }

        public void AddNode(EditableGenerator node, Point p)
        {
            if (CurrentFile.File.Exists)
            {
                TNode g = CurrentFile.MakeNode(node.Generate(ID<NodeTemp>.New(), new List<EditableGenerator.ParameterData>()), new NodeUIData(p));
                Action addNode = () => { CurrentFile.Add(g.Only(), Enumerable.Empty<NodeGroup>()); };
                g.Configure(MyEdit).Do
                (
                    //TODO: I swapped the add and configure here. Really not confident that this order makes sense. However it's needed for the audio parameters to correctly detect that their associated files are being used.
                    sup => { addNode(); sup.Redo(); }, //Configure the node (doesn't need to be undoable) and add it
                    b => { if (b != ConfigureResult.Cancel) addNode(); } //Add the node if the user didn't cancel (i.e. no editing was required)
                );
            }
        }

        public void RefreshContextMenu(IEnumerable<IMenuActionFactory<TNode>> menuFactories)
        {
            if (m_contextMenu == null)
                return;

            List<MenuAction2<TNode>> custom = new List<MenuAction2<TNode>>();

            foreach (var factory in menuFactories)
            {
                custom.AddRange(factory.GetMenuActions(this));
            }

            m_contextMenu.ResetCustomNodes(custom.ToArray());
        }

        public void GroupSelection()
        {
            if (Selected.Nodes.Any())
            {
                NodeGroup newGroup = NodeGroup.Make(Selected.Nodes.Select(CurrentFile.GetNode));
                CurrentFile.Add(Enumerable.Empty<TNode>(), newGroup.Only());
                SetSelection(Selected.Nodes.Evaluate(), Selected.Groups.Concat(newGroup.Only()).Evaluate());

                Redraw();
            }
        }

        public void UngroupSelection()
        {
            if (Selected.Groups.Any())
            {
                CurrentFile.Remove(Enumerable.Empty<TNode>(), Selected.Groups);
                SetSelection(Selected.Nodes.Evaluate(), Enumerable.Empty<NodeGroup>());

                Redraw();
            }
        }

        public void DuplicateSelection()
        {
            if (Selected.Nodes.Any() || Selected.Groups.Any())
            {
                var duplicates = m_copyPasteController.Duplicate(Selected.Nodes.Select(CurrentFile.GetNode), Selected.Groups, DataSource);
                var nodesAndGroups = CurrentFile.DuplicateInto(duplicates.Item1, duplicates.Item2, duplicates.Item3, m_localization);
                SetSelection(nodesAndGroups.Item1, nodesAndGroups.Item2);
                Redraw();
            }
        }

        public void SelectAll()
        {
            SetSelection(CurrentFile.Nodes.Evaluate(), CurrentFile.Groups);
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

        public void Insert(Point? p, Tuple<IEnumerable<GraphAndUI<NodeUIData>>, IEnumerable<NodeGroup>> additions)
        {
            Point loc = p ?? DrawWindowToGraphSpace(new Point(Width / 2, Height / 2));
            var nodesAndGroups = CurrentFile.DuplicateInto(additions.Item1, additions.Item2, loc, m_localization);
            SetSelection(nodesAndGroups.Item1, nodesAndGroups.Item2);
        }

        public Control AsControl()
        {
            return this;
        }

        private TransitionNoduleUIInfo UIInfo(Output connection)
        {
            return CurrentFile.UIInfo(connection);
        }
    }

    /// <summary>
    /// To get rid of the generic parameter to make the designer happy
    /// </summary>
    public class ConversationEditorControl : GraphEditorControl<ConversationNode>
    {
    }
}
