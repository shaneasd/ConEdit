using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utilities;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGui>;
using System.Drawing.Drawing2D;
using Utilities.UI;
using Conversation;

namespace ConversationEditor
{
    public partial class ProjectExplorer : UserControl
    {
        public static Bitmap ProjectIcon { get; }
        public static Bitmap FolderIcon { get; }

        Dictionary<DirectoryInfoHashed, ContainerItem> m_mapping = new Dictionary<DirectoryInfoHashed, ContainerItem>(10000);
        private ContextMenuStrip m_contextMenu;
        private SharedContext m_context;

        IColorScheme m_scheme = ColorScheme.Default; //So the designer has something to work with
        public IColorScheme Scheme
        {
            get { return m_scheme; }
            set
            {
                m_scheme = value;
                greyScrollBar1.ColorScheme = value;
                drawWindow1.ColorScheme = value;
                drawWindow2.ColorScheme = value;
                drawWindow3.ColorScheme = value;
                m_contextMenu.Renderer = value.ContextMenu;

                foreach (var button in m_buttons)
                {
                    button.SelectionPen = Scheme.ForegroundPen;
                    button.HighlightBackground = Scheme.ForegroundBrush;
                }
            }
        }

        static ProjectExplorer()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("ConversationEditor.Resources.Project.png"))
                ProjectIcon = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("ConversationEditor.Resources.Folder.png"))
                FolderIcon = new Bitmap(stream);
        }

        SuppressibleAction m_updateScrollbar;

        public ProjectExplorer()
        {
            m_suppressibleItemSelected = new SuppressibleAction(() => ItemSelected.Execute());

            InitializeComponent();

            UpdateSelectedLocalizer = new SuppressibleAction(UnsuppressibleUpdateSelectedLocalizer);

            m_updateScrollbar = new SuppressibleAction(() =>
            {
                greyScrollBar1.Minimum = 0;
                var listSize = m_root.AllItems(Visibility).Count() * Item.HEIGHT;
                greyScrollBar1.Maximum = (listSize - drawWindow1.Height).Clamp(0, float.MaxValue);
                greyScrollBar1.PercentageCovered = drawWindow1.Height / listSize;
            });

            m_contextMenu = this.contextMenuStrip1;
            drawWindow1.Resize += (a, b) => this.InvalidateImage();

            greyScrollBar1.Scrolled += () => drawWindow1.Invalidate(true);
            drawWindow1.Resize += (a, b) => m_updateScrollbar.TryExecute();

            drawWindow1.MouseWheel += (a, e) => greyScrollBar1.MouseWheeled(e);

            int offset = 0;
            Func<Image, HighlightableImageButton> makeButton = icon =>
                {
                    var thisoffset = offset;
                    var result = HighlightableImageButton.Create(() => new RectangleF(thisoffset, 0, icon.Width + 4, icon.Height + 4), Pens.Aqua, Brushes.LightGreen, icon);
                    offset += icon.Width + 4 + 2;
                    return result;
                };

            foreach (var definition in ProjectElementDefinition.Definitions)
            {
                var d = definition;
                var button = makeButton(definition.Icon);
                button.Highlighted = true;
                d.RegisterFilterChangedCallback(m_visibility, b => button.Highlighted = b);
                button.ValueChanged += () => { d.Update(button.Highlighted, ref m_visibility); InvalidateImage(); };

                m_buttons.Add(button);
            }

            var folderFilterButton = makeButton(FolderIcon);
            folderFilterButton.Highlighted = true;
            folderFilterButton.ValueChanged += () => { m_visibility.Types.EmptyFolders.Value = folderFilterButton.Highlighted; InvalidateImage(); };
            m_buttons.Add(folderFilterButton);

            m_visibility.Types.Audio.Changed.Register(b => { m_config.Audio.Value = b.To; m_updateScrollbar.TryExecute(); });
            m_visibility.Types.Conversations.Changed.Register(b => { m_config.Conversations.Value = b.To; m_updateScrollbar.TryExecute(); });
            m_visibility.Types.Domains.Changed.Register(b => { m_config.Domains.Value = b.To; m_updateScrollbar.TryExecute(); });
            m_visibility.Types.EmptyFolders.Changed.Register(b => { m_config.Folders.Value = b.To; m_updateScrollbar.TryExecute(); });
            m_visibility.Types.Localizations.Changed.Register(b => { m_config.Localizations.Value = b.To; m_updateScrollbar.TryExecute(); });

            m_filterTextBox = new MyTextBox(drawWindow3, () => drawWindow3.DisplayRectangle, MyTextBox.InputFormEnum.Text, s => null);
            MyTextBox.SetupCallbacks(drawWindow3, m_filterTextBox);
            m_filterTextBox.TextChanged += (oldtext) => filterTextChanged();
        }

        private void filterTextChanged()
        {
            m_visibility.Text.Value = m_filterTextBox.Text;
            m_updateScrollbar.TryExecute();
            InvalidateImage();
        }

        MyTextBox m_filterTextBox;

        private void UnsuppressibleUpdateSelectedLocalizer()
        {
            var localizationItems = m_root.AllItems(VisibilityFilter.Just(Localizations: true)).OfType<RealLeafItem<ILocalizationFile, ILocalizationFile>>();
            if (localizationItems.Any())
            {
                m_selectedLocalizers = localizationItems.Where(f => m_context.CurrentLocalization.Value?.Sources?.Values?.Contains(f.Item.Id) ?? false).ToList();
                InvalidateImage();
            }
        }

        SuppressibleAction UpdateSelectedLocalizer;

        public void Initialize(SharedContext context, FileFilterConfig config)
        {
            m_context = context;
            m_context.CurrentLocalization.Changed.Register(this, (a, change) => UpdateSelectedLocalizer.TryExecute());
            m_config = config;
            m_visibility.Types.Audio.Value = config.Audio.Value;
            m_visibility.Types.Conversations.Value = config.Conversations.Value;
            m_visibility.Types.Domains.Value = config.Domains.Value;
            m_visibility.Types.EmptyFolders.Value = config.Folders.Value;
            m_visibility.Types.Localizations.Value = config.Localizations.Value;
        }

        public void InvalidateImage()
        {
            m_drawn.Clear();

            Action invalidate = () => Invalidate(true);
            if (InvokeRequired)
                Invoke(invalidate);
            else
                invalidate();
        }

        List<HighlightableImageButton> m_buttons = new List<HighlightableImageButton>();

        public class VisibilityFilter
        {
            public class TypesSet
            {
                public NotifierProperty<bool> Conversations = new NotifierProperty<bool>(true);
                public NotifierProperty<bool> Domains = new NotifierProperty<bool>(true);
                public NotifierProperty<bool> Localizations = new NotifierProperty<bool>(true);
                public NotifierProperty<bool> Audio = new NotifierProperty<bool>(true);
                public NotifierProperty<bool> EmptyFolders = new NotifierProperty<bool>(true);
            }
            public TypesSet Types = new TypesSet();
            public NotifierProperty<string> Text = new NotifierProperty<string>("");

            private VisibilityFilter() { }

            public static VisibilityFilter Everything = new VisibilityFilter();
            public static VisibilityFilter Just(bool Conversations = false, bool Domains = false, bool Localizations = false, bool Audio = false, bool EmptyFolders = false)
            {
                VisibilityFilter result = new VisibilityFilter();
                result.Types.Conversations.Value = Conversations;
                result.Types.Domains.Value = Domains;
                result.Types.Localizations.Value = Localizations;
                result.Types.Audio.Value = Audio;
                result.Types.EmptyFolders.Value = EmptyFolders;
                return result;
            }
        }
        private VisibilityFilter m_visibility = VisibilityFilter.Everything;
        private VisibilityFilter Visibility
        {
            get
            {
                return m_visibility;
            }
        }

        ProjectItem m_root = ProjectItem.Null;

        Item m_selectedItem = Item.Null; //The most highlighted thing. Clicking again will rename.
        Item m_selectedEditable = null; //The thing that's being viewed in the main editor
        IList<RealLeafItem<ILocalizationFile, ILocalizationFile>> m_selectedLocalizers = new List<RealLeafItem<ILocalizationFile, ILocalizationFile>>(); //Localization files to render as the files comprising the currently active localization set

        /// <summary>
        /// The item being dragged within the list
        /// </summary>
        Item DragItem
        {
            get;
            set;
        }

        #region http://tech.pro/tutorial/732/csharp-tutorial-how-to-use-custom-cursors

        public static Cursor CreateCursor(Bitmap bmp, int xHotSpot, int yHotSpot)
        {
            IntPtr ptr = bmp.GetHicon();
            IconInfo tmp = new IconInfo();
            NativeMethods.GetIconInfo(ptr, ref tmp);
            tmp.xHotspot = xHotSpot;
            tmp.yHotspot = yHotSpot;
            tmp.fIcon = false;
            ptr = NativeMethods.CreateIconIndirect(ref tmp);
            return new Cursor(ptr);
        }
        #endregion

        private static OpenFileDialog MakeOpenFileDialog(string DefaultExt, string Filter, bool Multiselect, bool ValidateNames)
        {
            var result = new OpenFileDialog();
            try
            {
                result.DefaultExt = DefaultExt;
                result.Filter = Filter;
                result.Multiselect = Multiselect;
                result.ValidateNames = ValidateNames;
            }
            catch
            {
                result.Dispose();
                throw;
            }
            return result;
        }

        OpenFileDialog m_ofdConversation = MakeOpenFileDialog(DefaultExt: "xml", Filter: "Conversations|*.xml", Multiselect: true, ValidateNames: false);
        OpenFileDialog m_ofdDomain = MakeOpenFileDialog(DefaultExt: "dom", Filter: "Domains|*.dom|xml|*.xml", Multiselect: true, ValidateNames: false);
        OpenFileDialog m_ofdLocalization = MakeOpenFileDialog(DefaultExt: "loc", Filter: "Localizations|*.loc|xml|*.xml", Multiselect: true, ValidateNames: false);
        OpenFileDialog m_ofdAudio = MakeOpenFileDialog(DefaultExt: "ogg", Filter: "Ogg Vorbis|*.ogg|All files (*.*)|*.*", Multiselect: true, ValidateNames: false);

        public SuppressibleAction m_suppressibleItemSelected;
        public event Action ItemSelected;
        public IConversationFile SelectedConversation
        {
            get
            {
                return (m_selectedEditable is LeafItem<IConversationFile>) ? (m_selectedEditable as LeafItem<IConversationFile>).Item : null;
            }
        }
        public IDomainFile CurrentDomainFile { get { return (m_selectedEditable is LeafItem<IDomainFile>) ? (m_selectedEditable as LeafItem<IDomainFile>).Item : null; } }
        public bool ProjectSelected { get { return m_selectedEditable is ProjectItem; } }

        private static float IndexToY(int i)
        {
            return i * Item.HEIGHT;
        }

        private int YToIndex(float y)
        {
            return (int)((y + greyScrollBar1.Value) / Item.HEIGHT);
            //return (int)((y) / Item.HEIGHT);
        }

        /// <summary>
        /// Set the scrollbar's value such that the specified item is visible
        /// </summary>
        private void ScrollForVisibility(Item item)
        {
            Func<float, float> YToIndexFloat = y => ((y + greyScrollBar1.Value) / Item.HEIGHT);

            float topIndex = YToIndexFloat(0);
            float bottomIndex = YToIndexFloat(drawWindow1.Height);

            if (IndexOf(item) < topIndex)
            {
                greyScrollBar1.Value += (IndexOf(item) - topIndex) * Item.HEIGHT;
            }
            else if (IndexOf(item) + 1 > bottomIndex)
            {
                greyScrollBar1.Value += (IndexOf(item) + 1 - bottomIndex) * Item.HEIGHT;
            }
        }

        private RectangleF RectangleForIndex(int i)
        {
            return new RectangleF(0, IndexToY(i), Width, Item.HEIGHT);
        }

        private RectangleF RectangleForItem(Item item)
        {
            return RectangleForIndex(IndexOf(item));
        }

        private Matrix TransformToRenderSurface //Currently the image is rendered unscaled so only the position will be honored
        {
            get
            {
                return new Matrix(1, 0, 0, 1, 0, -(int)greyScrollBar1.Value);
            }
        }

        HashSet<Item> m_drawn = new HashSet<Item>();

        private void drawWindow1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            {
                var allItems = m_root.AllItems(Visibility).ToArray();
                int i = 0;
                List<Tuple<Item, int, RectangleF>> itemsToDraw = new List<Tuple<Item, int, RectangleF>>();
                foreach (var item in allItems)
                {
                    if (!m_drawn.Contains(item))
                    {
                        var y0 = IndexToY(i);
                        y0 -= greyScrollBar1.Value;
                        var y1 = y0 + Item.HEIGHT;
                        if (y1 > 0 && y0 < drawWindow1.Height)
                        {
                            var rect = new RectangleF((int)TransformToRenderSurface.OffsetX, (int)TransformToRenderSurface.OffsetY + IndexToY(i), Width, Item.HEIGHT);
                            itemsToDraw.Add(Tuple.Create(item, i, rect));
                        }
                    }
                    i++;
                }

                foreach (var item in itemsToDraw)
                {
                    item.Item1.DrawBackground(g, Visibility, item.Item3);
                }
                foreach (var item in itemsToDraw)
                {
                    bool isSelectedItem = object.ReferenceEquals(m_selectedItem, item.Item1);
                    bool isSelectedEditable = object.ReferenceEquals(m_selectedEditable, item.Item1);
                    bool isSelectedLocalizer = m_selectedLocalizers.Contains(item.Item1);
                    if (isSelectedItem || isSelectedEditable || isSelectedLocalizer)
                        item.Item1.DrawSelection(g, item.Item3, isSelectedItem, isSelectedLocalizer || isSelectedEditable, m_scheme);
                }

                //Make sure all ancestors' vertical lines are drawn
                if (itemsToDraw.Any())
                {
                    var firstDrawn = itemsToDraw.First();
                    int indent = firstDrawn.Item1.IndentLevel;
                    for (int j = firstDrawn.Item2; j >= 0; j--)
                    {
                        if (allItems[j].IndentLevel < indent)
                        {
                            indent--;
                            var rect = new RectangleF((int)TransformToRenderSurface.OffsetX, (int)TransformToRenderSurface.OffsetY + IndexToY(j), Width, Item.HEIGHT);
                            allItems[j].DrawTree(g, allItems[j].CalculateIconRectangle(rect), Visibility, m_scheme);
                        }
                    }
                }

                foreach (var item in itemsToDraw)
                {
                    item.Item1.Draw(g, Visibility, item.Item3, m_scheme);
                }
                using (var renderer = new Arthur.NativeTextRenderer(g))
                {
                    foreach (var item in itemsToDraw)
                    {
                        item.Item1.DrawText(renderer, Visibility, item.Item3, m_scheme); //Draw the text after everything else as we want to keep our NativeTextRenderer around and it blocks other graphics operations
                    }
                }
            }

            e.Graphics.DrawRectangle(Scheme.ControlBorder, new Rectangle(new Point(0, 0), new Size(drawWindow1.Width - 1, drawWindow1.Height - 1)));
        }

        private int IndexOf(Item item)
        {
            return m_root.AllItems(Visibility).IndexOf(item);
        }

        public void Select<T>(T item) where T : ISaveableFileProvider
        {
            Item newSelected = ContainingItem(item);
            SelectItem(newSelected);
        }

        void SelectItem(Item item)
        {
            if (item == null)
            {
                m_selectedItem = Item.Null;
                //m_selectedItem = null;
                m_selectedEditable = null;
            }
            else
            {
                string canSelect = item.CanSelect();
                if (canSelect == null)
                {
                    if (item.Select(ref m_selectedItem, ref m_selectedEditable))
                    {
                        ScrollForVisibility(item);
                        m_suppressibleItemSelected.TryExecute();
                        InvalidateImage();
                    }
                }
                else
                {
                    MessageBox.Show(canSelect);
                }
            }

            if (m_renamingItem != null)
            {
                m_renamingItem.StopRenaming(true);
                m_renamingItem = null;
            }

            InvalidateImage();
        }

        private void Clicked(Item item, MouseEventArgs e)
        {
            var container = item as ContainerItem;
            if (container != null)
            {
                if (container.MinimizedIconRectangle(RectangleForItem(item)).Contains(e.Location.Plus(new PointF(0, greyScrollBar1.Value).Round())))
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        container.Minimized.Value = !container.Minimized.Value;
                        InvalidateImage();
                    }
                    return;
                }
            }

            if (m_selectedItem != item)
            {
                SelectItem(item);
            }
            else if (e.Button == MouseButtons.Left)
            {
                using (Graphics g = CreateGraphics())
                {
                    if (item.File.Exists)
                    {
                        int cursorPos = item.CursorPosition(e.X, g);
                        item.StartRenaming(cursorPos, drawWindow1);
                        m_renamingItem = item;
                        InvalidateImage();
                    }
                }
            }
            if (e.Button == MouseButtons.Right)
            {
                m_contextItem = item;
                m_cleanContextMenu();
                m_cleanContextMenu = () => { };
                saveToolStripMenuItem.Visible = m_selectedItem != null && m_selectedItem.CanSave;
                removeToolStripMenuItem.Visible = m_selectedItem != null && m_selectedItem.CanRemove;
                deleteToolStripMenuItem.Visible = m_selectedItem != null && m_selectedItem.CanDelete;
                playMenuItem.Visible = m_contextItem is RealLeafItem<AudioFile, IAudioFile>;
                setUpLocalizationsToolStripMenuItem.Visible = m_contextItem is ProjectItem;
                m_contextMenu.Show(PointToScreen(e.Location));

                {
                    var con = m_contextItem as ConversationItem;
                    if (con != null)
                    {
                        foreach (var aa in m_contextMenuItemsFactory.ConversationContextMenuItems((type, textId) => Tuple.Create(m_context.CurrentProject.Value.Localizer.Localize(type, textId), m_context.CurrentProject.Value.Localizer.LocalizationTime(type, textId))))
                        {
                            var a = aa;
                            var i = new ToolStripMenuItem(a.Name);
                            i.Click += (x, y) => a.Execute(con.Item, m_context.ErrorCheckerUtils());
                            m_contextMenu.Items.Insert(m_contextMenu.Items.IndexOf(importConversationToolStripMenuItem), i);
                            var temp = m_cleanContextMenu;
                            m_cleanContextMenu = () => { temp(); m_contextMenu.Items.Remove(i); };
                        }
                    }
                }

                {
                    var dom = m_contextItem as DomainItem;
                    if (dom != null)
                    {
                        foreach (var aa in m_contextMenuItemsFactory.DomainContextMenuItems)
                        {
                            var a = aa;
                            var i = new ToolStripMenuItem(a.Name);
                            i.Click += (x, y) => a.Execute(dom.Item);
                            m_contextMenu.Items.Insert(m_contextMenu.Items.IndexOf(importDomainToolStripMenuItem), i);
                            var temp = m_cleanContextMenu;
                            m_cleanContextMenu = () => { temp(); m_contextMenu.Items.Remove(i); };
                        }
                    }
                }

                {
                    var loc = m_contextItem as RealLeafItem<ILocalizationFile, ILocalizationFile>;
                    if (loc != null)
                    {
                        foreach (var aa in m_contextMenuItemsFactory.LocalizationContextMenuItems)
                        {
                            var a = aa;
                            var i = new ToolStripMenuItem(a.Name);
                            i.Click += (x, y) => a.Execute(loc.Item);
                            m_contextMenu.Items.Insert(m_contextMenu.Items.IndexOf(separatorBetweenLocalizationAndDomain), i);
                            var temp = m_cleanContextMenu;
                            m_cleanContextMenu = () => { temp(); m_contextMenu.Items.Remove(i); };
                        }
                    }
                }

                {
                    var folder = m_contextItem as FolderItem;
                    if (folder != null)
                    {
                        foreach (var aa in m_contextMenuItemsFactory.FolderContextMenuItems((type, textId) => Tuple.Create(m_context.CurrentProject.Value.Localizer.Localize(type, textId), m_context.CurrentProject.Value.Localizer.LocalizationTime(type, textId))))
                        {
                            var a = aa;
                            var i = new ToolStripMenuItem(a.Name);
                            i.Click += (x, y) =>
                            {
                                IEnumerable<ConversationItem> conversationItems = folder.AllItems(VisibilityFilter.Just(Conversations: true)).OfType<ConversationItem>();
                                var conversations = conversationItems.Select(z => z.Item);
                                a.Execute(conversations);
                            };
                            m_contextMenu.Items.Insert(m_contextMenu.Items.IndexOf(separatorBetweenLocalizationAndDomain), i);
                            var temp = m_cleanContextMenu;
                            m_cleanContextMenu = () => { temp(); m_contextMenu.Items.Remove(i); };
                        }
                    }
                }
            }
        }

        Action m_cleanContextMenu = () => { };

        public IProjectExplorerContextMenuItemsFactory m_contextMenuItemsFactory;

        Item m_renamingItem = null;
        private Item m_contextItem;
        private FileFilterConfig m_config;

        Item GetItem(Point location)
        {
            int index = YToIndex(location.Y);
            if (m_root.AllItems(Visibility).Count() > index)
            {
                return m_root.AllItems(Visibility).ElementAt(index);
            }
            return null;
        }

        private void drawWindow1_MouseClick(object sender, MouseEventArgs e)
        {
            var item = GetItem(e.Location);
            if (item != null)
                Clicked(item, e);
        }

        public bool ShouldReplaceFile(string newPath)
        {
            if (m_root.Project.Conversations.Any(f => f.File.File.FullName == (new FileInfo(newPath)).FullName))
            {
                MessageBox.Show("A file with that name already exists within the project");
                return false;
            }
            else
            {
                return DialogResult.OK == MessageBox.Show("Replace existing file?", "Replace?", MessageBoxButtons.OKCancel);
            }
        }

        private bool RenameItem(FileSystemObject item, string to)
        {
            string from = item.FullName;
            if (m_root.RenameElement(item, to, ShouldReplaceFile))
            {
                InvalidateImage();
                return true;
            }
            return false;
        }

        private ProjectItem MakeNewProjectItem(IProject p)
        {
            var result = new ProjectItem(() => RectangleForItem(m_root), p, () => TransformToRenderSurface, RenameItem);
            try
            {
                result.File.SaveStateChanged += InvalidateImage;
            }
            catch
            {
                result.Dispose();
                throw;
            }
            return result;
        }

        internal void SetProject(IProject p, Project.TConfig config)
        {
            m_root = p.File.Exists ? MakeNewProjectItem(p) : ProjectItem.Null;
            m_ofdConversation.InitialDirectory = p.Origin.FullName;
            m_ofdDomain.InitialDirectory = p.Origin.FullName;
            m_ofdLocalization.InitialDirectory = p.Origin.FullName;
            m_ofdAudio.InitialDirectory = p.Origin.FullName;
            UpdateList(config);
        }

        private void UpdateList(Project.TConfig config)
        {
            using (m_suppressibleItemSelected.SuppressCallback())
            using (m_updateScrollbar.SuppressCallback())
            {
                foreach (var item in m_root.Children(VisibilityFilter.Everything))
                {
                    item.File.SaveStateChanged -= InvalidateImage;
                }

                m_mapping.Clear();
                m_root.ClearProject();
                m_mapping[new DirectoryInfoHashed(m_root.Path)] = m_root;

                if (m_root.Project.File.Exists) //If it's a dummy project then A) we don't need to bother and B) getting the parent of the project file will fail
                {
                    foreach (var directory in m_root.File.Parent.EnumerateDirectories("*", SearchOption.AllDirectories))
                    {
                        FindOrMakeDirectory(directory);
                    }
                }

                m_root.Project.AudioFiles.Removed += Remove;
                m_root.Project.AudioFiles.Reloaded += (from, to) => { Remove(from); Add(to); };
                m_root.Project.AudioFiles.Added += Add;
                Stopwatch addingAudioTime = Stopwatch.StartNew();
                foreach (IAudioFile aud in m_root.Project.AudioFiles)
                {
                    Add(aud);
                }
                addingAudioTime.Stop();

                m_root.Project.Conversations.Removed += Remove;
                m_root.Project.Conversations.Reloaded += (from, to) => { Remove(from); Add(to); };
                m_root.Project.Conversations.Added += Add;
                Stopwatch addingConversationsTime = Stopwatch.StartNew();
                foreach (var con in m_root.Project.Conversations)
                {
                    Add(con);
                }

                addingConversationsTime.Stop();

                m_root.Project.DomainFiles.Removed += Remove;
                m_root.Project.DomainFiles.Reloaded += (from, to) => { Remove(from); Add(to); };
                m_root.Project.DomainFiles.Added += Add;
                foreach (var domainFile in m_root.Project.DomainFiles)
                {
                    Add(domainFile);
                }

                var lastCon = config.LastEdited != null ? m_root.Project.Conversations.FirstOrDefault(c => c.Id == config.LastEdited) : null;
                if (lastCon != null)
                    Select(lastCon);
                else
                {
                    var lastDom = config.LastEdited != null ? m_root.Project.DomainFiles.FirstOrDefault(c => c.Id == config.LastEdited) : null;
                    if (lastDom != null)
                        Select(lastCon);
                }


                m_root.Project.LocalizationFiles.Removed += Remove;
                m_root.Project.LocalizationFiles.Reloaded += (from, to) => { Remove(from); Add(to); };
                m_root.Project.LocalizationFiles.Added += Add;
                using (UpdateSelectedLocalizer.SuppressCallback())
                {
                    foreach (var localizationFile in m_root.Project.LocalizationFiles)
                    {
                        Add(localizationFile);
                    }
                }

                if (m_root.Project.File.Exists) //If it's not a real project then don't try and enumerate directories
                {
                    var dirs = m_root.Project.File.File.Directory.EnumerateDirectories("*", SearchOption.AllDirectories);
                    foreach (var dir in dirs)
                    {
                        FindOrMakeParent(new FileInfo(Path.Combine(dir.FullName, "a.fake.file")));
                    }
                }

                SelectItem(null);
                UpdateSelectedLocalizer.TryExecute();

                m_updateScrollbar.TryExecute();
            }

            InvalidateImage();
        }

        private static bool SameItem<T>(T a, T b) where T : ISaveableFileProvider
        {
            return a.File.File.FullName == b.File.File.FullName;
        }

        private void Add<T>(T element) where T : ISaveableFileProvider
        {
            if (!m_root.Contains(element))
            {
                ContainerItem parent = FindOrMakeParent(element.File.File);
                Item result = null;
                if (element.File.Exists)
                {
                    result = m_root.MakeElement(() => RectangleForItem(result), element, parent, () => TransformToRenderSurface);
                    //result = definition.MakeElement(() => RectangleForItem(result), element, m_root.Project, parent, () => TransformToRenderSurface);
                }
                else
                {
                    result = m_root.MakeMissingElement(() => RectangleForItem(result), element, parent, () => TransformToRenderSurface);
                    //result = definition.MakeMissingElement(() => RectangleForItem(result), element, m_root.Project, parent, () => TransformToRenderSurface);
                }
                result.File.SaveStateChanged += InvalidateImage;

                if (m_selectedItem == null)
                {
                    m_selectedItem = result;
                    m_selectedEditable = result;
                    m_suppressibleItemSelected.TryExecute();
                }

                if (element is ILocalizationFile)
                {
                    UpdateSelectedLocalizer.TryExecute(); //If this is a localizer being added then it wasn't in the list when the selection changed callback was triggered
                }
                m_updateScrollbar.TryExecute();
                InvalidateImage();
            }
        }

        private void Remove<T>(T element) where T : ISaveableFileProvider
        {
            //m_root.m_contents.Remove(element.File.File.FullName);
            Item item = m_root.AllItems(VisibilityFilter.Everything).OfType<LeafItem<T>>().SingleOrDefault(i => SameItem(i.Item, element));
            if (item != null)
            {
                item.File.SaveStateChanged -= InvalidateImage;
                m_root.RemoveProjectChild(item, element.File.File.FullName);
                if (item == m_selectedItem)
                {
                    m_selectedItem = Item.Null;
                    //m_selectedItem = null;
                    m_selectedEditable = null;
                    m_suppressibleItemSelected.TryExecute();
                }
                InvalidateImage();
            }
        }

        private ContainerItem FindOrMakeParent(FileInfo file)
        {
            return FindOrMakeDirectory(file.Directory);
            //var path = FileSystem.PathToFrom(file, m_root.Path);
            //ContainerItem parent = m_root;
            //foreach (var folder in path.Skip(1))
            //{
            //    DirectoryInfoHashed key = new DirectoryInfoHashed(folder);
            //    bool add = !m_mapping.ContainsKey(key);
            //    if (!add)
            //        parent = m_mapping[key];
            //    if (add)
            //    {
            //        FolderItem child = null;
            //        child = new FolderItem(() => RectangleForItem(child), folder, m_root.Project, parent, () => TransformToRenderSurface);
            //        child.MinimizedChanged += () => m_updateScrollbar.TryExecute();
            //        parent.InsertChildAlphabetically(child);
            //        parent = child;
            //        m_mapping[key] = parent;
            //    }
            //}
            //return parent;
        }

        private ContainerItem FindOrMakeDirectory(DirectoryInfo dir)
        {
            var path = FileSystem.PathToFrom(dir, m_root.Path);
            ContainerItem parent = m_root;
            foreach (var folder in path.Skip(1))
            {
                DirectoryInfoHashed key = new DirectoryInfoHashed(folder);
                bool add = !m_mapping.ContainsKey(key);
                if (!add)
                    parent = m_mapping[key];
                if (add)
                {
                    FolderItem child = null;
                    child = new FolderItem(() => RectangleForItem(child), folder, m_root.Project, parent, () => TransformToRenderSurface, RenameItem);
                    child.Minimized.Changed.Register(this, (a, b) => m_updateScrollbar.TryExecute());
                    m_root.InsertProjectChildAlphabetically(parent, child);
                    parent = child;
                    m_mapping[key] = parent;
                }
            }
            return parent;
        }

        //private IEnumerable<LeafItem<T>> LeafItems<T>() where T : ISaveableFileProvider
        //{
        //    return m_root.AllItems(VisibilityFilter.Everything).OfType<LeafItem<T>>();
        //}

        private LeafItem ContainingItem(ISaveableFileProvider item)
        {
            return m_root.AllItems(VisibilityFilter.Everything).OfType<LeafItem>().FirstOrDefault(i => object.Equals(i.Item, item));
        }

        private void drawWindow1_Leave(object sender, EventArgs e)
        {
            if (m_renamingItem != null)
            {
                m_renamingItem.StopRenaming(true);
                m_renamingItem = null;
            }

            DragItem = null;
        }

        private void drawWindow1_MouseDown(object sender, MouseEventArgs e)
        {
            //Don't try and drag if we're renaming something
            if (m_renamingItem != null)
                return;

            var item = GetItem(e.Location);
            if (e.Button != System.Windows.Forms.MouseButtons.Left)
                DragItem = null;
            else if (item == null)
                DragItem = null;
            else if (item.File == null)
                DragItem = null;
            else if (!item.File.Exists)
                DragItem = null;
            else
            {
                if (e.X > item.MinimizedIconRectangle(RectangleForItem(item)).Right)
                    DragItem = item;
                else
                    DragItem = null;
            }
        }

        private void drawWindow1_MouseUp(object sender, MouseEventArgs e)
        {
            var item = GetItem(e.Location);
            if (DragItem != null)
            {
                var destination = item.SpawnLocation;

                if (destination != DragItem) //Can't drag an item into itself
                {
                    var name = DragItem.File.Name;
                    var path = new FileInfo(Path.Combine(destination.Path.FullName, name));
                    if (path.FullName != DragItem.File.FullName) //Don't bother dragging an item back to where it was
                    {
                        if (!m_root.Contains(path.FullName)) //Can't move a file to a folder that already contains an item by that name
                        {
                            if (!m_root.MoveElement(DragItem, destination, path.FullName, ShouldReplaceFile))
                                MessageBox.Show("Cannot move file to this location. Possibly a file with this name already exists there.");
                        }
                        else
                        {
                            MessageBox.Show("The project already contains an element with this name in this location");
                        }
                    }
                    InvalidateImage();
                }
                DragItem = null;
            }
        }

        private void newConversationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_root.Project.CanModifyConversations)
            {
                IConversationFile conversation = m_root.Project.Conversations.New(m_contextItem.SpawnLocation.Path);
                Add(conversation);
                Select(conversation);
            }
            else
            {
                MessageBox.Show("Changes cannot be made to conversations while there are unsaved domain changes");
            }
        }

        private void importConversationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_root.Project.CanModifyConversations)
            {
                if (m_ofdConversation.ShowDialog() == DialogResult.OK)
                {
                    var loaded = LoadValidImportFiles(m_ofdConversation, m_root.Project.Conversations);
                    if (loaded.Any())
                    {
                        foreach (var l in loaded)
                            Add(l);
                        Select(loaded.Last());
                    }
                }
            }
            else
            {
                MessageBox.Show("Changes cannot be made to conversations while there are unsaved domain changes");
            }
        }

        private void newLocalizationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ILocalizationFile localizer = m_root.Project.LocalizationFiles.New(m_contextItem.SpawnLocation.Path);
            Select(localizer);
        }

        private void importLocalizationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_ofdLocalization.ShowDialog() == DialogResult.OK)
            {
                var loaded = LoadValidImportFiles(m_ofdLocalization, m_root.Project.LocalizationFiles);
                if (loaded.Any())
                {
                    Select(loaded.Last());
                }
            }
        }

        private IEnumerable<TInterface> LoadValidImportFiles<TConcrete, TInterface>(OpenFileDialog ofd, IProjectElementList<TConcrete, TInterface> list) where TConcrete : TInterface
        {
            var badpaths = ofd.FileNames.Where(p => !list.FileLocationOk(p)).ToArray();
            if (badpaths.Any())
                MessageBox.Show("The following files could not be imported as they are not as they do not exist within the same folder structure as the project: \n" + string.Join("\n", badpaths));
            IEnumerable<DocumentPath> goodpaths = ofd.FileNames.Where(p => list.FileLocationOk(p)).Select(path => DocumentPath.FromPath(path, m_context.CurrentProject.Value.Origin));
            var loaded = list.Load(goodpaths);
            return loaded;
        }

        private void newDomainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_root.Project.CanModifyDomain)
            {
                IDomainFile domain = m_root.Project.DomainFiles.New(m_contextItem.SpawnLocation.Path);
                Select(domain);
            }
            else
            {
                MessageBox.Show("Changes cannot be made to domain while there are unsaved conversation changes");
            }
        }

        private void importDomainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_root.Project.CanModifyDomain)
            {
                if (m_ofdDomain.ShowDialog() == DialogResult.OK)
                {
                    var loaded = LoadValidImportFiles(m_ofdDomain, m_root.Project.DomainFiles);
                    if (loaded.Any())
                    {
                        Select(loaded.Last());
                    }
                }
            }
            else
            {
                MessageBox.Show("Changes cannot be made to domain while there are unsaved conversation changes");
            }
        }

        private void importAudioToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_ofdAudio.ShowDialog() == DialogResult.OK)
            {
                LoadValidImportFiles(m_ofdAudio, m_root.Project.AudioFiles);
            }
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var conversationItem = m_contextItem as LeafItem<IConversationFile>;
            var localizationItem = m_contextItem as LeafItem<ILocalizationFile>;
            var domainItem = m_contextItem as LeafItem<IDomainFile>;
            var audioItem = m_contextItem as LeafItem<IAudioFile>;

            if (conversationItem != null)
                m_root.Project.Conversations.Remove(conversationItem.Item, false);
            else if (localizationItem != null)
                m_root.Project.LocalizationFiles.Remove(localizationItem.Item, false);
            else if (domainItem != null)
            {
                if (m_root.Project.CanModifyDomain)
                {
                    m_root.Project.DomainFiles.Remove(domainItem.Item, false);
                }
                else
                {
                    MessageBox.Show("Changes cannot be made to domain while there are unsaved conversation changes");
                }
            }
            else if (audioItem != null)
                m_root.Project.AudioFiles.Remove(audioItem.Item, false);
            else
                throw new InternalLogicException("dfvhbsd345kbak");
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var conversationItem = m_contextItem as LeafItem<IConversationFile>;
            var localizationItem = m_contextItem as LeafItem<ILocalizationFile>;
            var domainItem = m_contextItem as LeafItem<IDomainFile>;
            var audioItem = m_contextItem as LeafItem<IAudioFile>;
            if (conversationItem != null)
                m_root.Project.Conversations.Delete(conversationItem.Item);
            else if (localizationItem != null)
                m_root.Project.LocalizationFiles.Delete(localizationItem.Item);
            else if (domainItem != null)
            {
                if (m_root.Project.CanModifyDomain)
                {
                    m_root.Project.DomainFiles.Delete(domainItem.Item);
                }
                else
                {
                    MessageBox.Show("Changes cannot be made to domain while there are unsaved conversation changes");
                }
            }
            else if (audioItem != null)
            {
                m_root.Project.AudioFiles.Delete(audioItem.Item);
            }
            else
                throw new InternalLogicException("fdvbsdh4k53bka");
        }

        private void playMenuItem_Click(object sender, EventArgs e)
        {
            var audioItem = m_contextItem as LeafItem<IAudioFile>;
            if (audioItem.File.Exists)
                audioItem.Item.Play();
        }

        private void ExploreBestFolder(DirectoryInfo folder)
        {
            if (folder.Exists)
                Process.Start("explorer", "/root,\"" + folder.FullName + "\"");
            else
                ExploreBestFolder(folder.Parent);
        }

        private void showInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var leaf = m_contextItem as LeafItem;
            var folder = m_contextItem as FolderItem;
            var project = m_contextItem as ProjectItem;
            if (leaf != null)
            {
                if (leaf.File.Exists)
                    Process.Start("explorer", "/select,\"" + leaf.File.FullName + "\"");
                else
                    ExploreBestFolder(leaf.File.Parent);
            }
            else if (project != null)
            {
                Process.Start("explorer", "/select,\"" + project.File.FullName + "\"");
            }
            else if (folder != null)
                ExploreBestFolder(folder.Path);
        }

        private void drawWindow2_Paint(object sender, PaintEventArgs e)
        {
            drawWindow2.Height = ProjectElementDefinition.Definitions.Select(d => d.Icon.Height).Max() + 4 + 4;

            m_buttons.ForEach(b => b.Paint(e.Graphics));
        }

        private void drawWindow2_MouseClick(object sender, MouseEventArgs e)
        {
            foreach (var button in m_buttons)
            {
                if (button.Area.Contains(e.Location))
                    button.MouseClick(e);
            }
            InvalidateImage();
        }

        private void drawWindow1_MouseMove(object sender, MouseEventArgs e)
        {
            if (DragItem != null)
            {
                const int SIZE = 16;
                using (Bitmap bmp = new Bitmap(SIZE, SIZE))
                {
                    using (var g = Graphics.FromImage(bmp))
                        DragItem.DrawIcon(g, new RectangleF(0, 0, SIZE, SIZE));
                    Cursor c = CreateCursor(bmp, 0, 0);
                    Cursor = c;
                }
            }
            else
                Cursor = DefaultCursor;
        }

        private void newFolderToolStripMenuItem_Click(object sender, EventArgs _)
        {
            var parent = m_contextItem.SpawnLocation;
            DirectoryInfo newDir = null;
            for (int i = 0; newDir == null; i++)
            {
                string newPath = Path.Combine(parent.Path.FullName, "New Folder " + i);
                if (!Directory.Exists(newPath))
                {
                    newDir = new DirectoryInfo(newPath);
                    try
                    {
                        newDir.Create();
                    }
                    catch (IOException e)
                    {
                        newDir = null;
                        Console.Out.WriteLine(e.Message);
                        Console.Out.WriteLine(e.StackTrace);
                        MessageBox.Show("Failed to create directory");
                    }
                }
            }

            if (newDir != null)
            {
                ProjectExplorer.FolderItem item = null;
                item = new ProjectExplorer.FolderItem(() => RectangleForItem(item), newDir, m_root.Project, parent, () => TransformToRenderSurface, RenameItem);
                m_root.InsertProjectChildAlphabetically(parent, item);
                m_updateScrollbar.TryExecute();
                InvalidateImage();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var conversationItem = m_contextItem as LeafItem<IConversationFile>;
            var localizationItem = m_contextItem as LeafItem<ILocalizationFile>;
            var domainItem = m_contextItem as LeafItem<IDomainFile>;
            //Audio files are not saveable

            if (conversationItem != null)
                conversationItem.Item.File.Writable.Save();
            else if (localizationItem != null)
                localizationItem.Item.File.Writable.Save();
            else if (domainItem != null)
                domainItem.Item.File.Writable.Save();
            else
                throw new InternalLogicException("attempted to save a file that isn't saveable from the project explorer menu");
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                UpdateSelectedLocalizer.Dispose();
                m_updateScrollbar.Dispose();
                m_suppressibleItemSelected.Dispose();
            }

            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void importIntoLocalizationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = m_contextItem as RealLeafItem<ILocalizationFile, ILocalizationFile>;

            if (m_ofdLocalization.ShowDialog() == DialogResult.OK)
            {
                item.Item.ImportInto(m_ofdLocalization.FileNames, m_context.CurrentProject.Value.Origin);
            }
        }

        private void setUpLocalizationsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetUpLocalizationsForm.SetupLocalizations(m_context.CurrentProject.Value);
        }
    }
}
