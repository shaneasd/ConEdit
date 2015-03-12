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
    public partial class ProjectExplorer
    {
        public class RealLeafItem<TConcrete, TInterface> : LeafItem<TInterface>
            where TConcrete : TInterface
            where TInterface : ISaveableFileProvider
        {
            private readonly TConcrete m_item;
            private readonly Bitmap m_icon;
            public new TConcrete Item { get { return m_item; } }
            public RealLeafItem(Func<RectangleF> area, TConcrete item, Bitmap icon, IProject project, ContainerItem parent, Func<VisibilityFilter, bool> filter, Func<Matrix> toControlTransform, Func<FileSystemObject, string, bool> rename)
                : base(area, project, item, parent, filter, toControlTransform, rename)
            {
                m_item = item;
                m_icon = icon;
            }

            public override void DrawIcon(Graphics g, RectangleF iconRectangle)
            {
                g.DrawImage(m_icon, iconRectangle);
            }

            public override bool CanDelete { get { return true; } }
            public override bool CanRemove { get { return true; } }
            public override bool CanSave { get { return m_item.File.Writable != null; } }
        }
    }
}