﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using Utilities;

namespace ConversationEditor
{
    partial class ProjectExplorer
    {
        private class FolderItem : ContainerItem
        {
            public FolderItem(Func<RectangleF> area, DirectoryInfo path, IProject project, ContainerItem parent, Func<Matrix> toControlTransform, Func<FileSystemObject, string, bool> rename)
                : base(new ConstructorParams(area, project, new FileSystemObject( path), parent, toControlTransform, rename))
            {
                m_path = path ?? throw new ArgumentNullException("path");
                File.Moved += (o, n) => { Children(VisibilityFilter.Everything).ForAll(a => a.File.ParentMoved(o, n)); };
            }

            private DirectoryInfo m_path;

            public override void DrawIcon(Graphics g, RectangleF iconRectangle)
            {
                g.DrawImage(FolderIcon, iconRectangle);
            }

            public override DirectoryInfo Path => m_path;

            public override bool CanDelete => false;
            public override bool CanRemove => false;
            public override bool CanSave => false;
        }

    }
}
