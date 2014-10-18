using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using Utilities;

namespace ConversationEditor
{
    public partial class ProjectExplorer
    {
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
        }
    }
}
