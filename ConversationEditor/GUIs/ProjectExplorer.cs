﻿using System;
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
using ConversationNode = Conversation.ConversationNode<Conversation.INodeGUI>;
using System.Drawing.Drawing2D;

namespace ConversationEditor
{
    public partial class ProjectExplorer : UserControl
    {
        public static Bitmap ProjectIcon;
        public static Bitmap FolderIcon;

        Dictionary<DirectoryInfo, ContainerItem> m_mapping = new Dictionary<DirectoryInfo, ContainerItem>(new DirectoryEqualityComparer());
        private ContextMenuStrip m_contextMenu;

        static ProjectExplorer()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("ConversationEditor.Resources.Project.png"))
                ProjectIcon = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("ConversationEditor.Resources.Folder.png"))
                FolderIcon = new Bitmap(stream);
        }

        public ProjectExplorer()
        {
            InitializeComponent();

            drawWindow1.BackColor = ColorScheme.Background;
            m_contextMenu = this.contextMenuStrip1;
            m_contextMenu.Renderer = ColorScheme.ContextMenu;
            drawWindow1.Resize += (a, b) => this.InvalidateImage();

            greyScrollBar1.Scrolled += () => drawWindow1.Invalidate(true);
            drawWindow1.Resize += (a, b) => UpdateScrollBar();

            int offset = 0;
            Func<Image, HighlightableImageButton> makeButton = icon =>
                {
                    var thisoffset = offset;
                    var result = HighlightableImageButton.Create(() => new RectangleF(thisoffset, 0, icon.Width + 4, icon.Height + 4), ColorScheme.ForegroundPen, ColorScheme.ForegroundBrush, icon);
                    offset += icon.Width + 4 + 2;
                    return result;
                };

            foreach (var definition in ProjectElementDefinition.Definitions)
            {
                var d = definition;
                var button = makeButton(definition.Icon);
                button.Highlighted = true;
                button.ValueChanged += () => { d.Update(button.Highlighted, ref m_visibility); InvalidateImage(); };
                m_buttons.Add(button);
            }

            var folderFilterButton = makeButton(FolderIcon);
            folderFilterButton.Highlighted = true;
            folderFilterButton.ValueChanged += () => { m_visibility.EmptyFolders = folderFilterButton.Highlighted; InvalidateImage(); };
            m_buttons.Add(folderFilterButton);
        }

        public void InvalidateImage()
        {
            lock (m_imageLock)
            {
                m_image = null;
            }
            Action invalidate = () => Invalidate(true);
            if (InvokeRequired)
                Invoke(invalidate);
            else
                invalidate();
        }

        List<HighlightableImageButton> m_buttons = new List<HighlightableImageButton>();

        public struct VisibilityFilter
        {
            public bool Conversations;
            public bool Domains;
            public bool Localizations;
            public bool Audio;
            public bool EmptyFolders;

            public static VisibilityFilter Everything = new VisibilityFilter { Conversations = true, Domains = true, Localizations = true, Audio = true, EmptyFolders = true };
        }
        private VisibilityFilter m_visibility = new VisibilityFilter() { Conversations = true, Audio = true, Domains = true, EmptyFolders = true, Localizations = true };
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
        RealLeafItem<ILocalizationFile, ILocalizationFile> m_selectedLocalizer;
        /// <summary>
        /// The item being dragged within the list
        /// </summary>
        Item DragItem
        {
            get;
            set;
        }

        #region http://tech.pro/tutorial/732/csharp-tutorial-how-to-use-custom-cursors

        public struct IconInfo
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);

        [DllImport("user32.dll")]
        public static extern IntPtr CreateIconIndirect(ref IconInfo icon);

        public static Cursor CreateCursor(Bitmap bmp, int xHotSpot, int yHotSpot)
        {
            IntPtr ptr = bmp.GetHicon();
            IconInfo tmp = new IconInfo();
            GetIconInfo(ptr, ref tmp);
            tmp.xHotspot = xHotSpot;
            tmp.yHotspot = yHotSpot;
            tmp.fIcon = false;
            ptr = CreateIconIndirect(ref tmp);
            return new Cursor(ptr);
        }
        #endregion

        OpenFileDialog m_ofdConversation = new OpenFileDialog() { DefaultExt = "xml", Filter = "Conversations|*.xml", Multiselect = true, ValidateNames = false };
        OpenFileDialog m_ofdDomain = new OpenFileDialog() { DefaultExt = "dom", Filter = "Domains|*.dom|xml|*.xml", Multiselect = true, ValidateNames = false };
        OpenFileDialog m_ofdLocalization = new OpenFileDialog() { DefaultExt = "loc", Filter = "Localizations|*.loc|xml|*.xml", Multiselect = true, ValidateNames = false };
        OpenFileDialog m_ofdAudio = new OpenFileDialog() { DefaultExt = "ogg", Filter = "Ogg Vorbis|*.ogg|All files (*.*)|*.*", Multiselect = true, ValidateNames = false };

        public event Action ItemSelected;
        public IConversationFile SelectedConversation
        {
            get
            {
                return (m_selectedEditable is LeafItem<IConversationFile>) ? (m_selectedEditable as LeafItem<IConversationFile>).Item : null;
            }
        }
        public event Action LocalizerSelected;
        public ILocalizationFile CurrentLocalizer { get { return m_selectedLocalizer.Item; } }
        public IDomainFile CurrentDomainFile { get { return (m_selectedEditable is LeafItem<IDomainFile>) ? (m_selectedEditable as LeafItem<IDomainFile>).Item : null; } }
        public bool ProjectSelected { get { return m_selectedEditable is ProjectItem; } }

        private float IndexToY(int i)
        {
            return i * Item.HEIGHT;
        }

        private int YToIndex(float y)
        {
            Refresh();
            return (int)((y + greyScrollBar1.Value) / Item.HEIGHT);
            //return (int)((y) / Item.HEIGHT);
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

        object m_imageLock = new object();
        Image m_image = null;
        private void drawWindow1_Paint(object sender, PaintEventArgs e)
        {
            bool redraw = false;
            Image image;
            lock (m_imageLock)
            {
                if (m_image == null)
                {
                    m_image = new Bitmap(drawWindow1.Width, (int)RectangleForIndex(m_root.AllItems(Visibility).Count()).Bottom);
                    redraw = true;
                }
                image = m_image;
            }

            if (redraw)
            {
                using (Graphics g = Graphics.FromImage(image))
                {
                    foreach (var item in m_root.AllItems(Visibility))
                    {
                        item.DrawBackground(g, Visibility);
                    }
                    foreach (var item in (new[] { m_selectedItem, m_selectedLocalizer, m_selectedEditable }).Distinct().Where(a => a != null))
                    {
                        if (IndexOf(item) >= 0)
                            item.DrawSelection(g, RectangleForIndex(IndexOf(item)), m_selectedItem == item, m_selectedLocalizer == item || m_selectedEditable == item);
                    }
                    foreach (var item in m_root.AllItems(Visibility))
                    {
                        item.Draw(g, Visibility);
                    }
                }
            }

            e.Graphics.DrawImage(image, new Point((int)TransformToRenderSurface.OffsetX, (int)TransformToRenderSurface.OffsetY));
            e.Graphics.DrawRectangle(ColorScheme.ControlBorder, new Rectangle(new Point(0, 0), new Size(drawWindow1.Width - 1, drawWindow1.Height - 1)));
        }

        private int IndexOf(Item item)
        {
            return m_root.AllItems(Visibility).IndexOf(item);
        }

        public void Select<T>(T item) where T : ISaveableFileProvider
        {
            Item newSelected = ContainingItem2(item);
            SelectItem(newSelected);
        }

        void SelectItem(Item item)
        {
            if (item == null)
            {
                m_selectedItem = null;
            }
            else
            {
                string canSelect = item.CanSelect();
                if (canSelect == null)
                {
                    if (item.Select(ref m_selectedItem, ref m_selectedEditable))
                    {
                        ItemSelected.Execute();
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
                using (Graphics g = CreateGraphics())
                {
                    if (container.MinimizedIconRectangle(g).Contains(e.Location))
                    {
                        if (e.Button == MouseButtons.Left)
                        {
                            container.Minimized = !container.Minimized;
                            InvalidateImage();
                        }
                        return;
                    }
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
                        item.StartRenaming(cursorPos, g, drawWindow1);
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
                saveToolStripMenuItem.Visible = m_selectedItem.CanSave;
                removeToolStripMenuItem.Visible = m_selectedItem.CanRemove;
                deleteToolStripMenuItem.Visible = m_selectedItem.CanDelete;
                makeCurrentLocalizationMenuItem.Visible = m_contextItem is RealLeafItem<ILocalizationFile, ILocalizationFile>;
                playMenuItem.Visible = m_contextItem is RealLeafItem<AudioFile, IAudioFile>;
                m_contextMenu.Show(PointToScreen(e.Location));

                //import
                //import
                //makecurrent
                //play
                //foreach (var a in m_contextMenuItemsFactory.ConversationContextMenuItems(m_selectedLocalizer.Item.Localize)) //TODO: Fix localization
                foreach (var a in m_contextMenuItemsFactory.ConversationContextMenuItems(a => ""))
                {
                    var con = m_contextItem as ConversationItem;
                    var i = new ToolStripMenuItem(a.Name);
                    i.Click += (x, y) => a.Execute(con.Item);
                    m_contextMenu.Items.Insert(m_contextMenu.Items.IndexOf(importConversationToolStripMenuItem), i);
                    var temp = m_cleanContextMenu;
                    m_cleanContextMenu = () => { temp(); m_contextMenu.Items.Remove(i); };
                }
            }
        }

        Action m_cleanContextMenu = () => { };

        public IProjectExplorerContextMenuItemsFactory m_contextMenuItemsFactory;

        Item m_renamingItem = null;
        private Item m_contextItem;

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

        class DirectoryEqualityComparer : IEqualityComparer<DirectoryInfo>
        {
            public bool Equals(DirectoryInfo x, DirectoryInfo y)
            {
                return x.FullName == y.FullName;
            }

            public int GetHashCode(DirectoryInfo obj)
            {
                return obj.FullName.GetHashCode();
            }
        }

        private ProjectItem MakeNewProjectItem(IProject p)
        {
            var result = new ProjectItem(() => RectangleForItem(m_root), p, () => TransformToRenderSurface);
            result.File.SaveStateChanged += InvalidateImage;
            return result;
        }

        internal void SetProject(IProject p)
        {
            m_root = p.File.Exists ? MakeNewProjectItem(p) : ProjectItem.Null;
            m_ofdConversation.InitialDirectory = p.Origin.FullName;
            m_ofdDomain.InitialDirectory = p.Origin.FullName;
            m_ofdLocalization.InitialDirectory = p.Origin.FullName;
            m_ofdAudio.InitialDirectory = p.Origin.FullName;
            UpdateList();
        }

        private void UpdateList()
        {
            foreach (var item in m_root.Children(VisibilityFilter.Everything))
            {
                item.File.SaveStateChanged -= InvalidateImage;
            }

            m_mapping.Clear();
            m_root.Clear();
            m_mapping[m_root.Path] = m_root;

            m_root.Project.AudioFiles.Removed += Remove;
            m_root.Project.AudioFiles.Reloaded += (from, to) => { Remove(from); Add(to); };
            m_root.Project.AudioFiles.Added += Add;
            foreach (IAudioFile aud in m_root.Project.AudioFiles)
            {
                Add(aud);
            }

            m_root.Project.Conversations.Removed += Remove;
            m_root.Project.Conversations.Reloaded += (from, to) => { Remove(from); Add(to); };
            m_root.Project.Conversations.Added += Add;
            foreach (var con in m_root.Project.Conversations)
            {
                Add(con);
            }

            m_root.Project.DomainFiles.Removed += Remove;
            m_root.Project.DomainFiles.Reloaded += (from, to) => { Remove(from); Add(to); };
            m_root.Project.DomainFiles.Added += Add;
            foreach (var domainFile in m_root.Project.DomainFiles)
            {
                Add(domainFile);
            }

            m_root.Project.LocalizationFiles.Removed += Remove;
            m_root.Project.LocalizationFiles.Reloaded += (from, to) => { Remove(from); Add(to); };
            m_root.Project.LocalizationFiles.Added += Add;
            foreach (var localizationFile in m_root.Project.LocalizationFiles)
            {
                Add(localizationFile);
            }

            if (m_root.Project.File.Exists) //If it's not a real project then don't try and enumerate directories
            {
                var dirs = m_root.Project.File.File.Directory.EnumerateDirectories("*", SearchOption.AllDirectories);
                foreach (var dir in dirs)
                {
                    FindOrMakeParent(new FileInfo(Path.Combine(dir.FullName, "a.fake.file")));
                }
            }

            //TODO: select a valid localizer

            SelectItem(null);

            UpdateScrollBar();

            InvalidateImage();
        }

        private bool SameItem<T>(T a, T b) where T : ISaveableFileProvider
        {
            return a.File.File.FullName == b.File.File.FullName;
        }

        private void Add<T>(T element) where T : ISaveableFileProvider
        {
            var definition = ProjectElementDefinition.Get<T>();
            Item result = m_root.AllItems(VisibilityFilter.Everything).OfType<LeafItem<T>>().SingleOrDefault(i => SameItem(i.Item, element));
            if (result == null)
            {
                ContainerItem parent = FindOrMakeParent(element.File.File);
                if (element.File.Exists)
                    result = definition.MakeElement(() => RectangleForItem(result), element, m_root.Project, parent, () => TransformToRenderSurface);
                else
                    result = definition.MakeMissingElement(() => RectangleForItem(result), element, m_root.Project, parent, () => TransformToRenderSurface);
                result.File.SaveStateChanged += InvalidateImage;

                if (m_selectedItem == null)
                {
                    m_selectedItem = result;
                    m_selectedEditable = result;
                    ItemSelected.Execute();
                }

                UpdateScrollBar();
                InvalidateImage();
            }
        }

        private void Remove<T>(T element) where T : ISaveableFileProvider
        {
            Item item = m_root.AllItems(VisibilityFilter.Everything).OfType<LeafItem<T>>().SingleOrDefault(i => SameItem(i.Item, element));
            item.File.SaveStateChanged -= InvalidateImage;
            m_root.RemoveChild(item);
            if (item == m_selectedItem)
            {
                m_selectedItem = null;
                m_selectedEditable = null;
                ItemSelected.Execute();
            }
            InvalidateImage();
        }

        private void UpdateScrollBar()
        {
            greyScrollBar1.Minimum = 0;
            var listSize = m_root.AllItems(Visibility).Count() * Item.HEIGHT;
            greyScrollBar1.Maximum = (listSize - drawWindow1.Height).Clamp(0, float.MaxValue);
            greyScrollBar1.PercentageCovered = drawWindow1.Height / listSize;
        }

        private ContainerItem FindOrMakeParent(FileInfo file)
        {
            var path = FileSystem.PathToFrom(file, m_root.Path);
            ContainerItem parent = m_root;
            foreach (var folder in path.Skip(1))
            {
                if (m_mapping.ContainsKey(folder))
                    parent = m_mapping[folder];
                else
                {
                    FolderItem child = null;
                    child = new FolderItem(() => RectangleForItem(child), folder, m_root.Project, parent, () => TransformToRenderSurface);
                    child.MinimizedChanged += UpdateScrollBar;
                    parent.InsertChildAlphabetically(child);
                    parent = child;
                    m_mapping[folder] = parent;
                }
            }
            return parent;
        }

        private IEnumerable<LeafItem<T>> LeafItems<T>() where T : ISaveableFileProvider
        {
            return m_root.AllItems(VisibilityFilter.Everything).OfType<LeafItem<T>>();
        }

        private LeafItem<T> ContainingItem<T>(T item) where T : ISaveableFileProvider
        {
            return LeafItems<T>().FirstOrDefault(i => object.Equals(i.Item, item));
        }

        private LeafItem ContainingItem2(ISaveableFileProvider item)
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
                using (Graphics g = drawWindow1.CreateGraphics())
                {
                    if (e.X > item.MinimizedIconRectangle(g).Right)
                        DragItem = item;
                    else
                        DragItem = null;
                }
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
                    if (path.FullName != DragItem.File.FullName)
                    {
                        DragItem.MoveTo(destination, path.FullName);
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
            var goodpaths = ofd.FileNames.Where(p => list.FileLocationOk(p)).Select(path => new FileInfo(path));
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
                var loaded = LoadValidImportFiles(m_ofdAudio, m_root.Project.AudioFiles);
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
                throw new Exception("dfvhbsd345kbak");
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
                throw new Exception("fdvbsdh4k53bka");
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

        internal void SelectLocalization(ILocalizationFile loc)
        {
            var localizationItems = m_root.AllItems(VisibilityFilter.Everything).OfType<RealLeafItem<ILocalizationFile, ILocalizationFile>>();
            if (!localizationItems.Any())
                return; //This can happen for dummy projects
            var match = localizationItems.FirstOrDefault(f => f.Item.Equals(loc)) ?? localizationItems.First();
            SelectLocalization(match);
        }

        private void SelectLocalization(RealLeafItem<ILocalizationFile, ILocalizationFile> loc)
        {
            var oldSelected = m_selectedLocalizer;
            m_selectedLocalizer = loc;
            if (m_selectedLocalizer != oldSelected)
                LocalizerSelected.Execute();
            InvalidateImage();
        }

        private void makeCurrentLocalizationMenuItemClicked(object sender, EventArgs e)
        {
            SelectLocalization(m_contextItem as RealLeafItem<ILocalizationFile, ILocalizationFile>);
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
                Bitmap bmp = new Bitmap(SIZE, SIZE);
                using (var g = Graphics.FromImage(bmp))
                    DragItem.DrawIcon(g, new RectangleF(0, 0, SIZE, SIZE));
                Cursor c = CreateCursor(bmp, 0, 0);
                Cursor = c;
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
                item = new ProjectExplorer.FolderItem(() => RectangleForItem(item), newDir, m_root.Project, parent, () => TransformToRenderSurface);
                parent.InsertChildAlphabetically(item);
                UpdateScrollBar();
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
                throw new Exception("attempted to save a file that isn't saveable from the project explorer menu");
        }
    }
}
