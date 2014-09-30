using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using Utilities;

namespace ConversationEditor
{
    public partial class ProjectExplorer
    {
        public class MissingLeafItem<T> : LeafItem<T> where T : ISaveableFileProvider
        {
            Bitmap m_icon;
            public MissingLeafItem(Func<RectangleF> area, T item, IProject project, Bitmap icon, ContainerItem parent, Func<VisibilityFilter, bool> filter, Func<Matrix> toControlTransform)
                : base(area, project, item, parent, filter, toControlTransform)
            {
                m_icon = icon;
            }

            public override void DrawIcon(Graphics g, RectangleF iconRectangle)
            {
                g.DrawImage(m_icon, iconRectangle);
            }

            public override bool CanDelete { get { return false; } }
            public override bool CanRemove { get { return true; } }

            public override bool CanSave { get { return false; } }
        }
    }
}
