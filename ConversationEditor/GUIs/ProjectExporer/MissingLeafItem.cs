using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using Utilities;

namespace ConversationEditor
{
    partial class ProjectExplorer
    {
        public class MissingLeafItem<T> : LeafItem<T> where T : ISaveableFileProvider
        {
            Bitmap Icon { get; }
            public MissingLeafItem(Func<RectangleF> area, T item, IProject project, Bitmap icon, ContainerItem parent, ItemFilter filter, Func<Matrix> toControlTransform, Func<FileSystemObject, string, bool> rename)
                : base(area, project, item, parent, filter, toControlTransform, rename)
            {
                Icon = icon;
            }

            public override void DrawIcon(Graphics g, RectangleF iconRectangle)
            {
                g.DrawImage(Icon, iconRectangle);
            }

            public override bool CanDelete => false;
            public override bool CanRemove => true;

            public override bool CanSave => false;
        }
    }
}
