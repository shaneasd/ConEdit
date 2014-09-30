using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Utilities;
using System.Drawing.Drawing2D;
using System.Reflection;

namespace ConversationEditor
{
    public partial class ProjectExplorer
    {
        public class FileSystemObject
        {
            private Or<DirectoryInfo, ISaveableFile> m_target;
            IProject m_project;

            public FileSystemObject(IProject project, ISaveableFile file)
            {
                m_project = project;
                m_target = new Or<DirectoryInfo, ISaveableFile>(file);
            }

            public FileSystemObject(IProject project, DirectoryInfo folder)
            {
                m_project = project;
                m_target = folder;
            }

            public string Name
            {
                get
                {
                    return m_target.Transformed(a => a.Name, b => b.File.Name);
                }
            }

            public string FullName
            {
                get
                {
                    return m_target.Transformed(a => a.FullName, b => b.File.FullName);
                }
            }

            public event Action SaveStateChanged
            {
                add { m_target.Do(a => { }, b => b.SaveStateChanged += value); }
                remove { m_target.Do(a => { }, b => b.SaveStateChanged -= value); }
            }

            public bool Exists
            {
                get
                {
                    return m_target.Transformed(a => true, b => b.Exists);
                }
            }

            public bool Changed
            {
                get
                {
                    return m_target.Transformed(a => false, b => b.Writable == null || b.Writable.Changed);
                }
            }

            public DirectoryInfo Parent
            {
                get
                {
                    return m_target.Transformed(a => a.Parent, b => b.File.Directory);
                }
            }

            public bool Move(string newPath, Func<bool> replace)
            {
                Func<DirectoryInfo, bool> moveFolder = a =>
                    {
                        if (!Directory.Exists(newPath))
                        {
                            var oldpath = a.FullName;
                            a.MoveTo(newPath);
                            Moved.Execute(oldpath, newPath);
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("A directory already exists at this location");
                            return false;
                        }
                    };
                return m_target.Transformed(moveFolder, b => b.Move(new FileInfo(newPath), replace));
            }

            public event Action<string, string> Moved;

            internal void ParentMoved(string o, string n)
            {
                DirectoryInfo a = new DirectoryInfo(o);
                DirectoryInfo b = new DirectoryInfo(n);
                var path = m_target.Transformed(x => FileSystem.PathToFrom(x, a), x => FileSystem.PathToFrom(x.File, a));

                var newPath = Path.Combine(b.FullName.Only().Concat(path.Skip(1).Select(d => d.Name)).Concat(m_target.Transformed(x => x.Name, x => x.File.Name).Only()).ToArray());
                m_target.Do(x => m_target = x, x => x.GotMoved(new FileInfo(newPath)));
            }

            public bool Writable
            {
                get
                {
                    return m_target.Transformed(d => true, f => !f.File.Exists || (f.File.Attributes & FileAttributes.ReadOnly) != FileAttributes.ReadOnly);
                }
            }
        }

        public abstract class Item
        {
            public static TextureBrush ReadonlyBackgroundBrush;
            static Item()
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream("ConversationEditor.Resources.ReadOnly.png"))
                {
                    //Something about the image makes it unsuitable for the TextureBrush causing an out of memory exception but I'm not sure what
                    using (Image temp = new Bitmap(stream))
                    {
                        Image buffer = new Bitmap(temp.Width, temp.Height);
                        using (var fg = Graphics.FromImage(buffer))
                            fg.DrawImage(temp, 0, 0, temp.Width, temp.Height);
                        ReadonlyBackgroundBrush = new TextureBrush(buffer);
                    }
                }
            }

            public readonly FileSystemObject File;
            protected readonly IProject m_project;
            protected ContainerItem m_parent;

            protected static void ChangeParent(Item item, ContainerItem parent)
            {
                item.m_parent = parent;
            }

            private Func<RectangleF> m_area;
            public RectangleF Area { get { return m_area(); } }

            public struct ConstructorParams
            {
                public readonly Func<RectangleF> Area;
                public readonly IProject Project;
                public readonly FileSystemObject File;
                public readonly ContainerItem Parent;
                public readonly Func<Matrix> ToControlTransform;

                public ConstructorParams(Func<RectangleF> area, IProject project, FileSystemObject file, ContainerItem parent, Func<Matrix> toControlTransform)
                {
                    Area = area;
                    Project = project;
                    File = file;
                    Parent = parent;
                    ToControlTransform = toControlTransform;
                }
            }

            public Item(ConstructorParams parameters)
            {
                m_project = parameters.Project;
                File = parameters.File;
                m_parent = parameters.Parent;
                m_area = parameters.Area;
                ToControlTransform = parameters.ToControlTransform;
            }

            public readonly static Item Null = null;

            public const float HEIGHT = 20;
            protected virtual string PermanentText { get { return File.Name; } }
            public string Text
            {
                get
                {
                    return File.Changed ? PermanentText + " *" : PermanentText;
                }
            }
            public void DrawSelection(Graphics g, RectangleF area, bool selected, bool conversationSelected)
            {
                if (selected)
                {
                    var selectionArea = new RectangleF(area.X + 2, area.Y + 2, area.Width - 4 - 1, area.Height - 4);
                    using (var brush = new SolidBrush(ColorScheme.SelectedConversationListItemPrimaryBackground))
                    {
                        g.FillRectangle(brush, selectionArea);
                    }
                }
                else if (conversationSelected)
                {
                    var selectionArea = new RectangleF(area.X + 2, area.Y + 2, area.Width - 4 - 1, area.Height - 4);
                    using (var brush = new SolidBrush(ColorScheme.SelectedConversationListItemSecondaryBackground))
                    {
                        g.FillRectangle(brush, selectionArea);
                    }
                }
            }

            private void DrawReadOnly(Graphics g)
            {
                g.FillRectangle(ReadonlyBackgroundBrush, Area);
            }

            RectangleF IconRectangle
            {
                get { return new RectangleF(Area.X + Indent + 3, Area.Y + 3, Area.Height - 6, Area.Height - 6); }
            }

            float Indent { get { return m_indentLevel * (Area.Height - 6); } }

            RectangleF TextArea { get { return RectangleF.FromLTRB(5 + Indent + IconRectangle.Width, Area.Top - 1, Area.Right, Area.Bottom); } }

            public RectangleF MinimizedIconRectangle(Graphics g)
            {
                const int minimizeRectangleSize = 8;
                //return new Rectangle((int)(m_area.Location.Plus(textStart).X + textSize.Width + 2), (int)(m_area.Location.Plus(textStart).Y + (textSize.Height - minimizeRectangleSize) / 2), minimizeRectangleSize, minimizeRectangleSize);
                return new RectangleF(Area.X + Indent + 2 - (Area.Height - 6) + minimizeRectangleSize / 2,
                                      Area.Y + 3 + (Area.Height - 6) / 2 - minimizeRectangleSize / 2,
                                      minimizeRectangleSize, minimizeRectangleSize);
            }

            public void Draw(Graphics g, VisibilityFilter filter)
            {
                float indent = Indent;
                var iconRectangle = IconRectangle;

                DrawMinimizeIcon(g, MinimizedIconRectangle(g), filter);

                DrawTree(g, iconRectangle, filter);
                DrawIcon(g, iconRectangle);

                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

                if (m_textBox == null)
                    g.DrawString(Text, SystemFonts.MessageBoxFont, ColorScheme.ForegroundBrush, TextArea.Location.Plus(MyTextBox.BORDER_SIZE, MyTextBox.BORDER_SIZE));
            }

            internal void DrawBackground(Graphics g, VisibilityFilter Visibility)
            {
                if (!File.Writable)
                {
                    DrawReadOnly(g);
                }
            }

            protected virtual void DrawMinimizeIcon(Graphics g, RectangleF minimizeIconRectangle, VisibilityFilter filter) { }

            public int CursorPosition(float x, Graphics g)
            {
                float bestX = float.NegativeInfinity;
                for (int i = 0; i < Text.Length; i++)
                {
                    float width = g.MeasureString(Text.Substring(0, i), SystemFonts.MessageBoxFont).Width;
                    float start = 5 + m_indentLevel * (HEIGHT - 6) + HEIGHT - 6;
                    if (start + width > x)
                    {
                        if (Math.Abs(bestX - x) < Math.Abs(start + width - x))
                        {
                            return i - 1;
                        }
                        else
                        {
                            return i;
                        }
                    }
                    bestX = start + width;
                }
                return Text.Length;
            }

            public abstract void DrawTree(Graphics g, RectangleF iconRectangle, VisibilityFilter filter);
            public abstract void DrawIcon(Graphics g, RectangleF iconRectangle);
            public abstract IEnumerable<Item> AllItems(VisibilityFilter filter);
            public abstract IEnumerable<Item> Children(VisibilityFilter filter);
            protected uint m_indentLevel = 0;
            public void SetIndentLevel(uint indentLevel)
            {
                m_indentLevel = indentLevel;
            }
            public abstract ContainerItem SpawnLocation { get; }

            const int CARET_HEIGHT = 15;

            MyTextBox m_textBox = null;

            public void StartRenaming(int cursorPos, Graphics g, Control control)
            {
                if (m_textBox == null)
                {
                    m_textBox = new MyTextBox(control, () => new RectangleF(TextArea.Location.Plus(ToControlTransform().OffsetX, ToControlTransform().OffsetY), TextArea.Size), MyTextBox.InputFormEnum.FileName);
                    m_textBox.Text = PermanentText;
                    m_textBox.CursorPos = cursorPos;
                    m_textBox.Colors.BackgroundBrush = Brushes.Transparent;
                    m_textBox.Colors.BorderPen = Pens.Transparent;
                    MyTextBox.SetupCallbacks(control, m_textBox);
                    m_textBox.GotFocus();
                }
            }

            public void StopRenaming(bool save)
            {
                if (m_textBox != null)
                {
                    if (m_textBox.Text != PermanentText)
                    {
                        if (save)
                        {
                            var newPath = File.Parent.FullName + Path.DirectorySeparatorChar + m_textBox.Text;
                            File.Move(newPath, () => Replace(newPath));
                        }
                    }

                    m_textBox.Dispose();
                    m_textBox = null;
                }
            }

            public bool Replace(string newPath)
            {
                if (m_project.Conversations.Any(f => f.File.File.FullName == (new FileInfo(newPath)).FullName))
                {
                    MessageBox.Show("A file with that name already exists within the project");
                    return false;
                }
                else
                {
                    return DialogResult.OK == MessageBox.Show("Replace existing file?", "Replace?", MessageBoxButtons.OKCancel);
                }
            }

            Func<Matrix> ToControlTransform;

            public abstract bool CanDelete { get; }
            public abstract bool CanRemove { get; }
            public abstract bool CanSave { get; }

            internal void MoveTo(ContainerItem destination, string path)
            {
                if (File.Move(path, () => Replace(path)))
                {
                    m_parent.RemoveChild(this);
                    destination.InsertChildAlphabetically(this);
                }
            }

            internal virtual bool Select(ref Item m_selectedItem, ref Item m_selectedEditable)
            {
                if (this != m_selectedItem)
                {
                    m_selectedItem = this;
                    return true;
                }
                return false;
            }

            internal virtual string CanSelect() { return null; }
        }

        private class ProjectItem : ContainerItem
        {
            public ProjectItem(Func<RectangleF> area, IProject project, Func<Matrix> toControlTransform)
                : base(new ConstructorParams(area, project, new FileSystemObject(project, project.File), null, toControlTransform))
            {
            }

            public IProject Project { get { return m_project; } }

            public override DirectoryInfo Path
            {
                get { return m_project.Origin; }
            }

            public override void DrawIcon(Graphics g, RectangleF iconRectangle)
            {
                g.DrawImage(ProjectIcon, iconRectangle);
            }

            private class DummyProjectItem : ProjectItem
            {
                public DummyProjectItem() : base(() => new RectangleF(0, 0, 0, 0), DummyProject.Instance, () => null) { }
                protected override string PermanentText { get { return ""; } }
                public override IEnumerable<Item> AllItems(VisibilityFilter filter)
                {
                    return Enumerable.Empty<Item>();
                }
            }

            public new static readonly ProjectItem Null = new DummyProjectItem();

            public override bool CanDelete { get { return false; } }
            public override bool CanRemove { get { return false; } }

            public override bool CanSave { get { return Project.File.Writable != null; } }
        }

        private class FolderItem : ContainerItem
        {
            public FolderItem(Func<RectangleF> area, DirectoryInfo path, IProject project, ContainerItem parent, Func<Matrix> toControlTransform)
                : base(new ConstructorParams(area, project, new FileSystemObject(project, path), parent, toControlTransform))
            {
                if (path == null)
                    throw new ArgumentNullException("path");
                m_path = path;
                File.Moved += (o, n) => { Children(VisibilityFilter.Everything).ForAll(a => a.File.ParentMoved(o, n)); };
            }

            private DirectoryInfo m_path;

            public override void DrawIcon(Graphics g, RectangleF iconRectangle)
            {
                g.DrawImage(FolderIcon, iconRectangle);
            }

            public override DirectoryInfo Path
            {
                get
                {
                    return m_path;
                }
            }

            public override bool CanDelete { get { return false; } }
            public override bool CanRemove { get { return false; } }
            public override bool CanSave { get { return false; } }
        }

        public abstract class ContainerItem : Item
        {
            public ContainerItem(ConstructorParams parameters) : base(parameters) { }

            private List<Item> m_subItems = new List<Item>();

            public void InsertChildAlphabetically(Item child)
            {
                child.SetIndentLevel(m_indentLevel + 1);
                bool inserted = false;
                for (int i = 0; i < m_subItems.Count && !inserted; i++)
                {
                    bool interteeIsContainer = child is ContainerItem;
                    bool candidateIsContainer = m_subItems[i] is ContainerItem;
                    bool nameOrdering = string.Compare(m_subItems[i].Text, child.Text) > 0;

                    if (((interteeIsContainer == candidateIsContainer) && nameOrdering) || (interteeIsContainer && !candidateIsContainer))
                    {
                        m_subItems.Insert(i, child);
                        inserted = true;
                    }
                }
                if (!inserted)
                    m_subItems.Add(child);

                Item.ChangeParent(child, this);
            }

            public override IEnumerable<Item> AllItems(VisibilityFilter filter)
            {
                if (Children(filter).Any() || filter.EmptyFolders)
                {
                    yield return this;

                    foreach (var subitem in Children(filter))
                        foreach (var subitemitem in subitem.AllItems(filter))
                            yield return subitemitem;
                }
            }

            public override IEnumerable<Item> Children(VisibilityFilter filter)
            {
                if (!Minimized)
                    return m_subItems.Where(a => a.AllItems(filter).Any()); //Filter out children that wont even report themselves as existing
                else
                    return Enumerable.Empty<Item>();
            }

            public void Clear()
            {
                m_subItems.Clear();
            }

            public abstract DirectoryInfo Path { get; }
            public override ContainerItem SpawnLocation { get { return this; } }
            private bool m_minimized = false;
            public event Action MinimizedChanged;
            public bool Minimized { get { return m_minimized; } set { m_minimized = value; MinimizedChanged.Execute(); } }

            public override void DrawTree(Graphics g, RectangleF iconRectangle, VisibilityFilter filter)
            {
                using (Pen pen = new Pen(ColorScheme.Foreground) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot })
                {
                    var start = iconRectangle.Center();

                    Func<Item, int> itemsBefore = (child) => Children(filter).TakeWhile(i => i != child).Select(c => c.AllItems(filter).Count()).Sum();

                    //Draw vertical line
                    if (Children(filter).Any())
                    {
                        int itemsBeforeLastChild = itemsBefore(Children(filter).Last());
                        g.DrawLine(pen, start, new PointF(start.X, start.Y + HEIGHT * (itemsBeforeLastChild + 1)));

                        //Draw horizontal line to each immediate child
                        foreach (var child in Children(filter))
                        {
                            var yOffset = (itemsBefore(child) + 1) * HEIGHT;
                            g.DrawLine(pen, new PointF(start.X, start.Y + yOffset), new PointF(start.X + HEIGHT - 6, start.Y + yOffset));
                        }
                    }
                }
            }

            protected override void DrawMinimizeIcon(Graphics g, RectangleF minimizeIconRectangle, VisibilityFilter filter)
            {
                if (m_subItems.Any(a => a.AllItems(filter).Any()))
                {
                    g.FillRectangle(ColorScheme.BackgroundBrush, minimizeIconRectangle);
                    g.DrawRectangle(ColorScheme.ForegroundPen, minimizeIconRectangle);
                    g.DrawLine(ColorScheme.ForegroundPen, new PointF(minimizeIconRectangle.Left + 2, minimizeIconRectangle.Y + minimizeIconRectangle.Height / 2), new PointF(minimizeIconRectangle.Right - 2, minimizeIconRectangle.Y + minimizeIconRectangle.Height / 2));
                    if (Minimized)
                        g.DrawLine(ColorScheme.ForegroundPen, new PointF(minimizeIconRectangle.Left + minimizeIconRectangle.Width / 2, minimizeIconRectangle.Top + 2), new PointF(minimizeIconRectangle.Right - minimizeIconRectangle.Width / 2, minimizeIconRectangle.Bottom - 2));
                }
            }

            internal bool RemoveChild(Item item)
            {
                if (!m_subItems.Remove(item))
                {
                    for (int i = 0; i < m_subItems.Count; i++)
                    {
                        var container = m_subItems[i] as ContainerItem;
                        if (container != null)
                            if (container.RemoveChild(item))
                                return true;
                    }
                    return false;
                }
                return true;
            }

            public override void DrawIcon(Graphics g, RectangleF iconRectangle)
            {
                throw new NotImplementedException();
            }

            public override bool CanDelete
            {
                get { throw new NotImplementedException(); }
            }

            public override bool CanRemove
            {
                get { throw new NotImplementedException(); }
            }
        }

        public abstract class LeafItem : Item
        {
            private readonly ISaveableFileProvider m_item;
            public ISaveableFileProvider Item { get { return m_item; } }
            private Func<VisibilityFilter, bool> m_filter;
            public LeafItem(Func<RectangleF> area, IProject project, ISaveableFile file, ContainerItem parent, Func<VisibilityFilter, bool> filter, ISaveableFileProvider item, Func<Matrix> toControlTransform)
                : base(new ConstructorParams(area, project, new FileSystemObject(project, file), parent, toControlTransform))
            {
                m_filter = filter;
                m_item = item;
            }

            public override IEnumerable<Item> AllItems(VisibilityFilter filter)
            {
                if (m_filter(filter))
                    yield return this;
            }

            public override IEnumerable<Item> Children(VisibilityFilter filter)
            {
                return Enumerable.Empty<Item>();
            }

            public override void DrawTree(Graphics g, RectangleF iconRectangle, VisibilityFilter filter)
            {
                //This is a leaf node so no tree
            }

            public override ContainerItem SpawnLocation
            {
                get { return m_parent; }
            }
        }

        public abstract class LeafItem<T> : LeafItem where T : ISaveableFileProvider
        {
            private readonly T m_item;

            public new T Item { get { return m_item; } }

            public LeafItem(Func<RectangleF> area, IProject project, T item, ContainerItem parent, Func<VisibilityFilter, bool> filter, Func<Matrix> toControlTransform)
                : base(area, project, item.File, parent, filter, item, toControlTransform)
            {
                m_item = item;
            }
        }
    }
}
