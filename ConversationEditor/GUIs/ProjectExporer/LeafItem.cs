using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ConversationEditor
{
    public partial class ProjectExplorer
    {
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
