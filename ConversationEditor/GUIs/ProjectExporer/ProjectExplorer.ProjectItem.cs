﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Drawing2D;
using System.Drawing;
using Utilities;
using System.IO;

namespace ConversationEditor
{
    partial class ProjectExplorer
    {
        private class ProjectItem : ContainerItem
        {
            private HashSet<string> m_contents = new HashSet<string>();

            public ProjectItem(Func<RectangleF> area, IProject project, Func<Matrix> toControlTransform, Func<FileSystemObject, string, bool> rename)
                : base(new ConstructorParams(area, project, new FileSystemObject(project.File), null, toControlTransform, rename))
            {
            }

            public void ClearProject()
            {
                Clear();
                m_contents.Clear();
            }

            public bool RemoveProjectChild(Item item, string path)
            {
                bool result = RemoveChild(item);
                m_contents.Remove(path); //Assuming if it's a descendent then it must also be in m_contents
                return result;
            }

            public void InsertProjectChildAlphabetically(ContainerItem parent, Item item)
            {
                m_contents.Add(item.File.FullName);
                ContainerItem.InsertChildAlphabetically(parent, item);
            }

            public override DirectoryInfo Path => Project.Origin;

            public override void DrawIcon(Graphics g, RectangleF iconRectangle)
            {
                g.DrawImage(ProjectIcon, iconRectangle);
            }

            private class DummyProjectItem : ProjectItem
            {
                public DummyProjectItem() : base(() => new RectangleF(0, 0, 0, 0), DummyProject.Instance, () => null, (f, s) => { throw new NotImplementedException(); }) { }
                protected override string PermanentText => "";
                public override IEnumerable<Item> AllItems(VisibilityFilter filter)
                {
                    return Enumerable.Empty<Item>();
                }
            }

            public new static readonly ProjectItem Null = new DummyProjectItem();

            public override bool CanDelete => false;
            public override bool CanRemove => false;

            public override bool CanSave => Project.File.Writable != null;

            public Item MakeElement<T>(Func<RectangleF> area, T item, ProjectExplorer.ContainerItem parent, Func<Matrix> toControlTransform) where T : ISaveableFileProvider
            {
                var definition = ProjectElementDefinition.Get<T>();
                var result = definition.MakeElement(area, item, Project, parent, toControlTransform, Rename);
                InsertProjectChildAlphabetically(parent, result);
                return result;
            }

            public Item MakeMissingElement<T>(Func<RectangleF> area, T item, ProjectExplorer.ContainerItem parent, Func<Matrix> toControlTransform) where T : ISaveableFileProvider
            {
                var definition = ProjectElementDefinition.Get<T>();
                var result = definition.MakeMissingElement(area, item, Project, parent, toControlTransform, Rename);
                InsertProjectChildAlphabetically(parent, result);
                return result;
            }

            internal bool Contains(ISaveableFileProvider element)
            {
                return Contains(element.File.File.FullName);
            }

            internal bool Contains(string path)
            {
                return m_contents.Contains(path);
            }

            /// <summary>
            /// Move a descendent element to the path indicated and change its parent
            /// </summary>
            /// <param name="DragItem">The item to move</param>
            /// <param name="destination">The new parent</param>
            /// <param name="path">The new path for the file</param>
            /// <returns>true if the operation completed successfully</returns>
            internal bool MoveElement(Item DragItem, ContainerItem destination, string path, Func<string, bool> ShouldReplaceFile)
            {
                if (DragItem.File.Move(path, () => ShouldReplaceFile(path)))
                {
                    RemoveProjectChild(DragItem, DragItem.File.FullName);
                    ContainerItem.InsertChildAlphabetically(destination, DragItem);
                    return true;
                }
                return false;
            }

            internal bool RenameElement(FileSystemObject item, string path, Func<string, bool> ShouldReplaceFile)
            {
                string oldName = item.FullName;
                if (item.Move(path, () => ShouldReplaceFile(path)))
                {
                    //This is done implicitly by RemoveProjectChild and InsertChildAlphabetically
                    m_contents.Remove(oldName);
                    m_contents.Add(path);
                    return true;
                }
                return false;
            }

            public override IEnumerable<Item> AllItems(VisibilityFilter filter)
            {
                return AllItemsWithoutFilteringThis(filter);
            }
        }
    }
}