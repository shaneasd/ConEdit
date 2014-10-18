using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using Utilities;

namespace ConversationEditor
{
    public partial class ProjectExplorer
    {
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

    }
}
