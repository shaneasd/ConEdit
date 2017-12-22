using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ConversationEditor
{
    partial class ProjectExplorer
    {
        public delegate bool ItemFilter(VisibilityFilter.TypesSet filterData);

        public abstract class LeafItem : Item
        {
            private readonly ISaveableFileProvider m_item;
            public ISaveableFileProvider Item { get { return m_item; } }
            private ItemFilter m_filter;
            protected LeafItem(Func<RectangleF> area, IProject project, ISaveableFile file, ContainerItem parent, ItemFilter filter, ISaveableFileProvider item, Func<Matrix> toControlTransform, Func<FileSystemObject, string, bool> rename)
                : base(new ConstructorParams(area, project, new FileSystemObject(file), parent, toControlTransform, rename))
            {
                m_filter = filter;
                m_item = item;
            }

            public override IEnumerable<Item> AllItems(VisibilityFilter filter)
            {
                if (m_filter(filter.Types) && Text.IndexOf(filter.Text.Value,StringComparison.CurrentCultureIgnoreCase) >= 0)
                    yield return this;
            }

            public override IEnumerable<Item> Children(VisibilityFilter filter)
            {
                return Enumerable.Empty<Item>();
            }

            public override void DrawTree(Graphics g, RectangleF iconRectangle, VisibilityFilter filter, IColorScheme scheme)
            {
                var start = iconRectangle.Center();
                float treeBranchX = start.X - ItemHeight + 6; //The x coordinate of the point where this node's connector line joins the parents branch line
                g.DrawLine(scheme.TreePen, start, new PointF(treeBranchX, start.Y));
                //g.DrawLine(ContainerItem.TreePen, treeBranchX, start.Y - HEIGHT, treeBranchX, start.Y + 1); //The +1 ensures the lines connect up nicely

                //start, new PointF(start.X, start.Y + HEIGHT * (itemsBeforeLastChild + 1)));
            }

            public override ContainerItem SpawnLocation
            {
                get { return Parent; }
            }
        }

        public abstract class LeafItem<T> : LeafItem where T : ISaveableFileProvider
        {
            private readonly T m_item;

            public new T Item { get { return m_item; } }

            protected LeafItem(Func<RectangleF> area, IProject project, T item, ContainerItem parent, ItemFilter filter, Func<Matrix> toControlTransform, Func<FileSystemObject, string, bool> rename)
                : base(area, project, item.File, parent, filter, item, toControlTransform, rename)
            {
                m_item = item;
            }
        }
    }
}
