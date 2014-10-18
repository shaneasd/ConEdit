﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Utilities;
using System.Drawing.Drawing2D;
using System.Reflection;

namespace ConversationEditor
{
    public partial class ProjectExplorer
    {
        public abstract class Item
        {
            public static TextureBrush ReadonlyBackgroundBrush;
            static Item()
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream("ConversationEditor.Resources.ReadOnly.png"))
                {
                    //Something about the image makes it unsuitable for the TextureBrush causing an out of memory exception but I'm not sure what
                    using (Image temp = new Bitmap(stream))
                    {
                        Image buffer = new Bitmap(temp.Width, temp.Height);
                        using (var fg = Graphics.FromImage(buffer))
                            fg.DrawImage(temp, 0, 0, temp.Width, temp.Height);
                        ReadonlyBackgroundBrush = new TextureBrush(buffer);
                    }
                }
            }

            public readonly FileSystemObject File;
            protected readonly IProject m_project;
            protected ContainerItem m_parent;

            protected static void ChangeParent(Item item, ContainerItem parent)
            {
                item.m_parent = parent;
            }

            private Func<RectangleF> m_area;
            //public RectangleF Area { get { return m_area(); } }

            public struct ConstructorParams
            {
                public readonly Func<RectangleF> Area;
                public readonly IProject Project;
                public readonly FileSystemObject File;
                public readonly ContainerItem Parent;
                public readonly Func<Matrix> ToControlTransform;

                public ConstructorParams(Func<RectangleF> area, IProject project, FileSystemObject file, ContainerItem parent, Func<Matrix> toControlTransform)
                {
                    Area = area;
                    Project = project;
                    File = file;
                    Parent = parent;
                    ToControlTransform = toControlTransform;
                }
            }

            public Item(ConstructorParams parameters)
            {
                m_project = parameters.Project;
                File = parameters.File;
                m_parent = parameters.Parent;
                m_area = parameters.Area;
                ToControlTransform = parameters.ToControlTransform;
            }

            public readonly static Item Null = null;

            public const float HEIGHT = 20;
            protected virtual string PermanentText { get { return File.Name; } }
            public string Text
            {
                get
                {
                    return File.Changed ? PermanentText + " *" : PermanentText;
                }
            }

            private void DrawReadOnly(Graphics g)
            {
                RectangleF area = m_area();
                g.FillRectangle(ReadonlyBackgroundBrush, area);
            }

            RectangleF CalculateIconRectangle(RectangleF area)
            {
                return new RectangleF(area.X + CalculateIndent(area) + 3, area.Y + 3, area.Height - 6, area.Height - 6);
            }

            float CalculateIndent(RectangleF area) { return m_indentLevel * (area.Height - 6); }

            RectangleF CalculateTextArea(RectangleF area, float indent)
            {
                return RectangleF.FromLTRB(5 + indent + CalculateIconRectangle(area).Width, area.Top - 1, area.Right, area.Bottom);
            }

            public RectangleF MinimizedIconRectangle(Graphics g)
            {
                var area = m_area();
                var indent = CalculateIndent(area);
                return MinimizedIconRectangle(g, area, indent);
            }
            public RectangleF MinimizedIconRectangle(Graphics g, RectangleF area, float indent)
            {
                const int minimizeRectangleSize = 8;
                //return new Rectangle((int)(m_area.Location.Plus(textStart).X + textSize.Width + 2), (int)(m_area.Location.Plus(textStart).Y + (textSize.Height - minimizeRectangleSize) / 2), minimizeRectangleSize, minimizeRectangleSize);
                return new RectangleF(area.X + indent + 2 - (area.Height - 6) + minimizeRectangleSize / 2,
                                      area.Y + 3 + (area.Height - 6) / 2 - minimizeRectangleSize / 2,
                                      minimizeRectangleSize, minimizeRectangleSize);
            }

            public void DrawSelection(Graphics g, RectangleF area, bool selected, bool conversationSelected)
            {
                if (selected)
                {
                    var selectionArea = new RectangleF(area.X + 2, area.Y + 2, area.Width - 4 - 1, area.Height - 4);
                    using (var brush = new SolidBrush(ColorScheme.SelectedConversationListItemPrimaryBackground))
                    {
                        g.FillRectangle(brush, selectionArea);
                    }
                }
                else if (conversationSelected)
                {
                    var selectionArea = new RectangleF(area.X + 2, area.Y + 2, area.Width - 4 - 1, area.Height - 4);
                    using (var brush = new SolidBrush(ColorScheme.SelectedConversationListItemSecondaryBackground))
                    {
                        g.FillRectangle(brush, selectionArea);
                    }
                }
            }

            public void Draw(Graphics g, VisibilityFilter filter)
            {
                var area = m_area();
                float indent = CalculateIndent(area);
                var iconRectangle = CalculateIconRectangle(area);

                DrawMinimizeIcon(g, MinimizedIconRectangle(g, area, indent), filter);

                DrawTree(g, iconRectangle, filter);
                DrawIcon(g, iconRectangle);

                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

                if (m_textBox == null)
                {
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    var textArea = CalculateTextArea(area, indent);
                    TextRenderer.DrawText(g, Text, SystemFonts.MessageBoxFont, textArea.Location.Plus(MyTextBox.BORDER_SIZE, MyTextBox.BORDER_SIZE).Round(), ColorScheme.Foreground, Color.Transparent, TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                    //g.DrawString(Text, SystemFonts.MessageBoxFont, ColorScheme.ForegroundBrush, TextArea.Location.Plus(MyTextBox.BORDER_SIZE, MyTextBox.BORDER_SIZE));
                }
            }

            internal void DrawBackground(Graphics g, VisibilityFilter Visibility)
            {
                if (!File.Writable)
                {
                    DrawReadOnly(g);
                }
            }

            protected virtual void DrawMinimizeIcon(Graphics g, RectangleF minimizeIconRectangle, VisibilityFilter filter) { }

            public int CursorPosition(float x, Graphics g)
            {
                float bestX = float.NegativeInfinity;
                for (int i = 0; i < Text.Length; i++)
                {
                    float width = g.MeasureString(Text.Substring(0, i), SystemFonts.MessageBoxFont).Width;
                    float start = 5 + m_indentLevel * (HEIGHT - 6) + HEIGHT - 6;
                    if (start + width > x)
                    {
                        if (Math.Abs(bestX - x) < Math.Abs(start + width - x))
                        {
                            return i - 1;
                        }
                        else
                        {
                            return i;
                        }
                    }
                    bestX = start + width;
                }
                return Text.Length;
            }

            public abstract void DrawTree(Graphics g, RectangleF iconRectangle, VisibilityFilter filter);
            public abstract void DrawIcon(Graphics g, RectangleF iconRectangle);
            public abstract IEnumerable<Item> AllItems(VisibilityFilter filter);
            public abstract IEnumerable<Item> Children(VisibilityFilter filter);
            protected uint m_indentLevel = 0;
            public void SetIndentLevel(uint indentLevel)
            {
                m_indentLevel = indentLevel;
            }
            public abstract ContainerItem SpawnLocation { get; }

            const int CARET_HEIGHT = 15;

            MyTextBox m_textBox = null;

            public void StartRenaming(int cursorPos, Graphics g, Control control)
            {
                if (m_textBox == null)
                {
                    var area = m_area();
                    var indent = CalculateIndent(area);
                    var textArea = CalculateTextArea(area, indent);
                    m_textBox = new MyTextBox(control, () => new RectangleF(textArea.Location.Plus(ToControlTransform().OffsetX, ToControlTransform().OffsetY), textArea.Size), MyTextBox.InputFormEnum.FileName);
                    m_textBox.Text = PermanentText;
                    m_textBox.CursorPos = new MyTextBox.CP(cursorPos);
                    m_textBox.Colors.Background = Color.Transparent;
                    m_textBox.Colors.BorderPen = Pens.Transparent;
                    MyTextBox.SetupCallbacks(control, m_textBox);
                    m_textBox.GotFocus();
                }
            }

            public void StopRenaming(bool save)
            {
                if (m_textBox != null)
                {
                    if (m_textBox.Text != PermanentText)
                    {
                        if (save)
                        {
                            var newPath = File.Parent.FullName + Path.DirectorySeparatorChar + m_textBox.Text;
                            File.Move(newPath, () => Replace(newPath));
                        }
                    }

                    m_textBox.Dispose();
                    m_textBox = null;
                }
            }

            public bool Replace(string newPath)
            {
                if (m_project.Conversations.Any(f => f.File.File.FullName == (new FileInfo(newPath)).FullName))
                {
                    MessageBox.Show("A file with that name already exists within the project");
                    return false;
                }
                else
                {
                    return DialogResult.OK == MessageBox.Show("Replace existing file?", "Replace?", MessageBoxButtons.OKCancel);
                }
            }

            Func<Matrix> ToControlTransform;

            public abstract bool CanDelete { get; }
            public abstract bool CanRemove { get; }
            public abstract bool CanSave { get; }

            internal void MoveTo(ContainerItem destination, string path)
            {
                if (File.Move(path, () => Replace(path)))
                {
                    m_parent.RemoveChild(this);
                    destination.InsertChildAlphabetically(this);
                }
            }

            internal virtual bool Select(ref Item m_selectedItem, ref Item m_selectedEditable)
            {
                if (this != m_selectedItem)
                {
                    m_selectedItem = this;
                    return true;
                }
                return false;
            }

            internal virtual string CanSelect() { return null; }
        }

        private class ProjectItem : ContainerItem
        {
            public ProjectItem(Func<RectangleF> area, IProject project, Func<Matrix> toControlTransform)
                : base(new ConstructorParams(area, project, new FileSystemObject(project, project.File), null, toControlTransform))
            {
            }

            public IProject Project { get { return m_project; } }

            public override DirectoryInfo Path
            {
                get { return m_project.Origin; }
            }

            public override void DrawIcon(Graphics g, RectangleF iconRectangle)
            {
                g.DrawImage(ProjectIcon, iconRectangle);
            }

            private class DummyProjectItem : ProjectItem
            {
                public DummyProjectItem() : base(() => new RectangleF(0, 0, 0, 0), DummyProject.Instance, () => null) { }
                protected override string PermanentText { get { return ""; } }
                public override IEnumerable<Item> AllItems(VisibilityFilter filter)
                {
                    return Enumerable.Empty<Item>();
                }
            }

            public new static readonly ProjectItem Null = new DummyProjectItem();

            public override bool CanDelete { get { return false; } }
            public override bool CanRemove { get { return false; } }

            public override bool CanSave { get { return Project.File.Writable != null; } }
        }
    }
}
