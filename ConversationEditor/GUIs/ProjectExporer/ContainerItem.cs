using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using Utilities;
using Utilities.UI;

namespace ConversationEditor
{
    public partial class ProjectExplorer
    {
        public abstract class ContainerItem : Item
        {
            protected ContainerItem(ConstructorParams parameters) : base(parameters) 
            {
            }

            private List<Item> m_subItems = new List<Item>();

            #region Modification

            //This makes the instance InsertChildAlphabetically availble to ProjectItem
            protected static void InsertChildAlphabetically(ContainerItem parent, Item child)
            {
                parent.InsertChildAlphabetically(child);
            }

            protected void InsertChildAlphabetically(Item child)
            {
                child.IndentLevel = IndentLevel + 1;
                bool inserted = false;
                for (int i = 0; i < m_subItems.Count && !inserted; i++)
                {
                    bool interteeIsContainer = child is ContainerItem;
                    bool candidateIsContainer = m_subItems[i] is ContainerItem;
                    bool nameOrdering = string.Compare(m_subItems[i].Text, child.Text, StringComparison.CurrentCulture) > 0;

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
                        if (m_subItems[i] is ContainerItem container)
                            if (container.RemoveChild(item))
                                return true;
                    }
                    return false;
                }
                return true;
            }
            #endregion

            /// <summary>
            /// Applies no filtering to this instance but still filters children
            /// </summary>
            protected IEnumerable<Item> AllItemsWithoutFilteringThis(VisibilityFilter filter)
            {
                yield return this;

                foreach (var subitem in Children(filter))
                    foreach (var subitemitem in subitem.AllItems(filter))
                        yield return subitemitem;
            }

            public override IEnumerable<Item> AllItems(VisibilityFilter filter)
            {
                if (Children(filter).Any() || (filter.Types.EmptyFolders.Value && Text.IndexOf(filter.Text.Value, StringComparison.CurrentCultureIgnoreCase) >= 0))
                {
                    return AllItemsWithoutFilteringThis(filter);
                }
                else
                    return Enumerable.Empty<Item>();
            }

            public override IEnumerable<Item> Children(VisibilityFilter filter)
            {
                if (!Minimized.Value)
                    return m_subItems.Where(a => a.AllItems(filter).Any()); //Filter out children that wont even report themselves as existing
                else
                    return Enumerable.Empty<Item>();
            }

            public abstract DirectoryInfo Path { get; }
            public override ContainerItem SpawnLocation => this;

            public NotifierProperty<bool> Minimized { get; } = new NotifierProperty<bool>(false);

            #region Drawing
            public override void DrawTree(Graphics g, RectangleF iconRectangle, VisibilityFilter filter, IColorScheme scheme)
            {
                var start = iconRectangle.Center();

                float treeBranchX = start.X - ItemHeight + 6; //The x coordinate of the point where this node's connector line joins the parents branch line
                g.DrawLine(scheme.TreePen, start, new PointF(treeBranchX, start.Y));

                Func<Item, int> itemsBefore = (child) => Children(filter).TakeWhile(i => i != child).Select(c => c.AllItems(filter).Count()).Sum();

                //Draw vertical line
                if (Children(filter).Any())
                {
                    int itemsBeforeLastChild = itemsBefore(Children(filter).Last());
                    g.DrawLine(scheme.TreePen, start, new PointF(start.X, start.Y + ItemHeight * (itemsBeforeLastChild + 1) + 1));
                }
            }
            protected override void DrawMinimizeIcon(Graphics g, RectangleF minimizeIconRectangle, VisibilityFilter filter, IColorScheme scheme)
            {
                if (m_subItems.Any(a => a.AllItems(filter).Any()))
                {
                    g.FillRectangle(scheme.BackgroundBrush, minimizeIconRectangle);
                    g.DrawRectangle(scheme.ForegroundPen, minimizeIconRectangle);
                    g.DrawLine(scheme.ForegroundPen, new PointF(minimizeIconRectangle.Left + 2, minimizeIconRectangle.Y + minimizeIconRectangle.Height / 2), new PointF(minimizeIconRectangle.Right - 2, minimizeIconRectangle.Y + minimizeIconRectangle.Height / 2));
                    if (Minimized.Value)
                        g.DrawLine(scheme.ForegroundPen, new PointF(minimizeIconRectangle.Left + minimizeIconRectangle.Width / 2, minimizeIconRectangle.Top + 2), new PointF(minimizeIconRectangle.Right - minimizeIconRectangle.Width / 2, minimizeIconRectangle.Bottom - 2));
                }
            }
            #endregion
        }
    }
}
