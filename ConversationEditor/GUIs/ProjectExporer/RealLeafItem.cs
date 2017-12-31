using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Utilities;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ConversationEditor
{
    partial class ProjectExplorer
    {
        public class RealLeafItem<TConcrete, TInterface> : LeafItem<TInterface>
            where TConcrete : TInterface
            where TInterface : ISaveableFileProvider
        {
            private Bitmap Icon { get; }
            public new TConcrete Item { get; }
            public RealLeafItem(Func<RectangleF> area, TConcrete item, Bitmap icon, IProject project, ContainerItem parent, ItemFilter filter, Func<Matrix> toControlTransform, Func<FileSystemObject, string, bool> rename)
                : base(area, project, item, parent, filter, toControlTransform, rename)
            {
                Item = item;
                Icon = icon;
            }

            public override void DrawIcon(Graphics g, RectangleF iconRectangle)
            {
                g.DrawImage(Icon, iconRectangle);
            }

            public override bool CanDelete => true;
            public override bool CanRemove => true;
            public override bool CanSave => Item.File.Writable != null;
        }
    }
}