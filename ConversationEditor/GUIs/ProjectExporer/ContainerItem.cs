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

            #region Modification

            //This makes the instance InsertChildAlphabetically availble to ProjectItem
            protected static void InsertChildAlphabetically(ContainerItem parent, Item child)
            {
                parent.InsertChildAlphabetically(child);
            }

            protected void InsertChildAlphabetically(Item child)
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

            protected virtual void Clear()
            {
                m_subItems.Clear();
            }

            protected bool RemoveChild(Item item)
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
            #endregion

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
                if (!Minimized.Value)
                    return m_subItems.Where(a => a.AllItems(filter).Any()); //Filter out children that wont even report themselves as existing
                else
                    return Enumerable.Empty<Item>();
            }

            public abstract DirectoryInfo Path { get; }
            public override ContainerItem SpawnLocation { get { return this; } }

            public readonly NotifierProperty<bool> Minimized = new NotifierProperty<bool>(false);

            #region Drawing
            public static Pen TreePen = new Pen(ColorScheme.Foreground) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot };
            public override void DrawTree(Graphics g, RectangleF iconRectangle, VisibilityFilter filter)
            {
                //var start = iconRectangle.Center();
                //float treeBranchX = start.X - HEIGHT + 6; //The x coordinate of the point where this node's connector line joins the parents branch line
                //g.DrawLine(ContainerItem.TreePen, start, new PointF(treeBranchX, start.Y));
                //g.DrawLine(ContainerItem.TreePen, treeBranchX, start.Y - HEIGHT, treeBranchX, start.Y + 1); //The +1 ensures the lines connect up nicely

                var start = iconRectangle.Center();
                Func<Item, int> itemsBefore = (child) => Children(filter).TakeWhile(i => i != child).Select(c => c.AllItems(filter).Count()).Sum();

                //Draw vertical line
                if (Children(filter).Any())
                {
                    int itemsBeforeLastChild = itemsBefore(Children(filter).Last());
                    g.DrawLine(ContainerItem.TreePen, start, new PointF(start.X, start.Y + HEIGHT * (itemsBeforeLastChild + 1) + 1));
                }
            }
            protected override void DrawMinimizeIcon(Graphics g, RectangleF minimizeIconRectangle, VisibilityFilter filter)
            {
                if (m_subItems.Any(a => a.AllItems(filter).Any()))
                {
                    g.FillRectangle(ColorScheme.BackgroundBrush, minimizeIconRectangle);
                    g.DrawRectangle(ColorScheme.ForegroundPen, minimizeIconRectangle);
                    g.DrawLine(ColorScheme.ForegroundPen, new PointF(minimizeIconRectangle.Left + 2, minimizeIconRectangle.Y + minimizeIconRectangle.Height / 2), new PointF(minimizeIconRectangle.Right - 2, minimizeIconRectangle.Y + minimizeIconRectangle.Height / 2));
                    if (Minimized.Value)
                        g.DrawLine(ColorScheme.ForegroundPen, new PointF(minimizeIconRectangle.Left + minimizeIconRectangle.Width / 2, minimizeIconRectangle.Top + 2), new PointF(minimizeIconRectangle.Right - minimizeIconRectangle.Width / 2, minimizeIconRectangle.Bottom - 2));
                }
            }
            #endregion
        }
    }
}
