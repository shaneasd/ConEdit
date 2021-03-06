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
using System.Diagnostics;
using Utilities.UI;

namespace ConversationEditor
{
    partial class ProjectExplorer
    {
        public abstract class Item : Disposable
        {
            public const float ItemHeight = 20;
            public static TextureBrush ReadOnlyBackgroundBrush { get; }
            public static Item Null => null;
            static Item()
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream("ConversationEditor.Resources.ReadOnly.png"))
                {
                    //Something about the image makes it unsuitable for the TextureBrush causing an out of memory exception but I'm not sure what
                    using (Image temp = new Bitmap(stream))
                    {
                        using (Image buffer = new Bitmap(temp.Width, temp.Height))
                        {
                            using (var fg = Graphics.FromImage(buffer))
                                fg.DrawImage(temp, 0, 0, temp.Width, temp.Height);
                            ReadOnlyBackgroundBrush = new TextureBrush(buffer);
                        }
                    }
                }
            }

            public FileSystemObject File { get; }
            public IProject Project { get; }
            protected ContainerItem Parent { get; private set; }

            private Func<RectangleF> m_area;
            Func<Matrix> ToControlTransform;
            protected Func<FileSystemObject, string, bool> Rename { get; }
            MyTextBox m_textBox = null;

            protected static void ChangeParent(Item item, ContainerItem parent)
            {
                item.Parent = parent;
            }

            public struct ConstructorParams
            {
                public Func<RectangleF> Area { get; }
                public IProject Project { get; }
                public FileSystemObject File { get; }
                public ContainerItem Parent { get; }
                public Func<Matrix> ToControlTransform { get; }
                public Func<FileSystemObject, string, bool> Rename { get; }

                public ConstructorParams(Func<RectangleF> area, IProject project, FileSystemObject file, ContainerItem parent, Func<Matrix> toControlTransform, Func<FileSystemObject, string, bool> rename)
                {
                    Area = area;
                    Project = project;
                    File = file;
                    Parent = parent;
                    ToControlTransform = toControlTransform;
                    Rename = rename;
                }
            }

            protected Item(ConstructorParams parameters)
            {
                Project = parameters.Project;
                File = parameters.File;
                Parent = parameters.Parent;
                m_area = parameters.Area;
                ToControlTransform = parameters.ToControlTransform;
                Rename = parameters.Rename;
            }

            protected virtual string PermanentText => File.Name;
            public string Text => File.Changed ? PermanentText + " *" : PermanentText;

            private static void DrawReadOnly(Graphics g, RectangleF area)
            {
                ReadOnlyBackgroundBrush.TranslateTransform(area.X, area.Y);
                g.FillRectangle(ReadOnlyBackgroundBrush, area);
                ReadOnlyBackgroundBrush.ResetTransform();
            }

            public RectangleF CalculateIconRectangle(RectangleF area)
            {
                return new RectangleF(area.X + CalculateIndent(area) + 3, area.Y + 3, area.Height - 6, area.Height - 6);
            }

            float CalculateIndent(RectangleF area) { return IndentLevel * (area.Height - 6); }

            RectangleF CalculateTextArea(RectangleF area, float indent)
            {
                return RectangleF.FromLTRB(5 + indent + CalculateIconRectangle(area).Width, area.Top - 1, area.Right, area.Bottom);
            }

            public RectangleF MinimizedIconRectangle(RectangleF wholeArea)
            {
                var indent = CalculateIndent(wholeArea);
                return MinimizedIconRectangle(wholeArea, indent);
            }
            private static RectangleF MinimizedIconRectangle(RectangleF area, float indent)
            {
                const int minimizeRectangleSize = 8;
                //return new Rectangle((int)(m_area.Location.Plus(textStart).X + textSize.Width + 2), (int)(m_area.Location.Plus(textStart).Y + (textSize.Height - minimizeRectangleSize) / 2), minimizeRectangleSize, minimizeRectangleSize);
                return new RectangleF(area.X + indent + 2 - (area.Height - 6) + minimizeRectangleSize / 2,
                                      area.Y + 3 + (area.Height - 6) / 2 - minimizeRectangleSize / 2,
                                      minimizeRectangleSize, minimizeRectangleSize);
            }

            public void DrawSelection(Graphics g, RectangleF area, bool selected, bool conversationSelected, IColorScheme scheme)
            {
                if (selected)
                {
                    var selectionArea = new RectangleF(area.X + 2, area.Y + 2, area.Width - 4 - 1, area.Height - 4);
                    using (var brush = new SolidBrush(scheme.SelectedConversationListItemPrimaryBackground))
                    {
                        g.FillRectangle(brush, selectionArea);
                    }
                }
                else if (conversationSelected)
                {
                    var selectionArea = new RectangleF(area.X + 2, area.Y + 2, area.Width - 4 - 1, area.Height - 4);
                    using (var brush = new SolidBrush(scheme.SelectedConversationListItemSecondaryBackground))
                    {
                        g.FillRectangle(brush, selectionArea);
                    }
                }
            }

            public void Draw(Graphics g, VisibilityFilter filter, RectangleF area, IColorScheme scheme)
            {
                float indent = CalculateIndent(area);
                var iconRectangle = CalculateIconRectangle(area);
                DrawMinimizeIcon(g, MinimizedIconRectangle(area, indent), filter, scheme);
                DrawTree(g, iconRectangle, filter, scheme);
                DrawIcon(g, iconRectangle);
            }

            internal void DrawBackground(Graphics g, VisibilityFilter Visibility, RectangleF area)
            {
                if (!File.Writable)
                {
                    DrawReadOnly(g, area);
                }
            }

            public void DrawText(Arthur.NativeTextRenderer renderer, VisibilityFilter visibility, RectangleF area, IColorScheme scheme)
            {
                //g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

                if (m_textBox == null)
                {
                    float indent = CalculateIndent(area);
                    var textArea = CalculateTextArea(area, indent);
                    int margin = 4;
                    renderer.DrawString(Text, SystemFonts.MessageBoxFont, scheme.Foreground, textArea.Location.Plus(margin, margin).Round());
                }
            }

            protected virtual void DrawMinimizeIcon(Graphics g, RectangleF minimizeIconRectangle, VisibilityFilter filter, IColorScheme scheme) { }

            public int CursorPosition(float x, Graphics g)
            {
                float bestX = float.NegativeInfinity;
                for (int i = 0; i < Text.Length; i++)
                {
                    float width = g.MeasureString(Text.Substring(0, i), SystemFonts.MessageBoxFont).Width;
                    float start = 5 + IndentLevel * (ItemHeight - 6) + ItemHeight - 6;
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

            public abstract void DrawTree(Graphics g, RectangleF iconRectangle, VisibilityFilter filter, IColorScheme scheme);
            public abstract void DrawIcon(Graphics g, RectangleF iconRectangle);
            public abstract IEnumerable<Item> AllItems(VisibilityFilter filter);
            public abstract IEnumerable<Item> Children(VisibilityFilter filter);

            public int IndentLevel { get; set; }

            public abstract ContainerItem SpawnLocation { get; }

            public void StartRenaming(int cursorPos, Control control)
            {
                if (m_textBox == null)
                {
                    var area = m_area();
                    var indent = CalculateIndent(area);
                    var textArea = CalculateTextArea(area, indent);
                    m_textBox = new MyTextBox(control, () => new RectangleF(textArea.Location.Plus(ToControlTransform().OffsetX, ToControlTransform().OffsetY), textArea.Size), MyTextBox.InputFormEnum.FileName, null, x => new SimpleTextBoxBorderDrawer(x), 4, Fonts.Default);
                    m_textBox.Text = PermanentText;
                    m_textBox.SetCursorPosition(cursorPos);
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
                            Rename(File, newPath);
                        }
                    }

                    m_textBox.Dispose();
                    m_textBox = null;
                }
            }

            public abstract bool CanDelete { get; }
            public abstract bool CanRemove { get; }
            public abstract bool CanSave { get; }

            //internal void MoveTo(ContainerItem destination, string path)
            //{
            //    if (File.Move(path, () => Replace(path)))
            //    {
            //        m_parent.RemoveChild(this);
            //        destination.InsertChildAlphabetically(this);
            //    }
            //}

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

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    m_textBox.Dispose();
                }
            }
        }
    }
}
