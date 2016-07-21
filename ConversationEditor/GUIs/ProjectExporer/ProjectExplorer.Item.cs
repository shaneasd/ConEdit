using System;
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
            public const float HEIGHT = 20;
            const int CARET_HEIGHT = 15;
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

            private Func<RectangleF> m_area;
            Func<Matrix> ToControlTransform;
            protected readonly Func<FileSystemObject, string, bool> Rename;
            protected int m_indentLevel = 0;
            MyTextBox m_textBox = null;

            protected static void ChangeParent(Item item, ContainerItem parent)
            {
                item.m_parent = parent;
            }
            
            public struct ConstructorParams
            {
                public readonly Func<RectangleF> Area;
                public readonly IProject Project;
                public readonly FileSystemObject File;
                public readonly ContainerItem Parent;
                public readonly Func<Matrix> ToControlTransform;
                public readonly Func<FileSystemObject, string, bool> Rename;

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
                m_project = parameters.Project;
                File = parameters.File;
                m_parent = parameters.Parent;
                m_area = parameters.Area;
                ToControlTransform = parameters.ToControlTransform;
                Rename = parameters.Rename;
            }

            public readonly static Item Null = null;

            protected virtual string PermanentText { get { return File.Name; } }
            public string Text
            {
                get
                {
                    return File.Changed ? PermanentText + " *" : PermanentText;
                }
            }

            private static void DrawReadOnly(Graphics g, RectangleF area)
            {
                ReadonlyBackgroundBrush.TranslateTransform(area.X, area.Y);
                g.FillRectangle(ReadonlyBackgroundBrush, area);
                ReadonlyBackgroundBrush.ResetTransform();
            }

            public RectangleF CalculateIconRectangle(RectangleF area)
            {
                return new RectangleF(area.X + CalculateIndent(area) + 3, area.Y + 3, area.Height - 6, area.Height - 6);
            }

            float CalculateIndent(RectangleF area) { return m_indentLevel * (area.Height - 6); }

            RectangleF CalculateTextArea(RectangleF area, float indent)
            {
                return RectangleF.FromLTRB(5 + indent + CalculateIconRectangle(area).Width, area.Top - 1, area.Right, area.Bottom);
            }

            public RectangleF MinimizedIconRectangle(RectangleF wholeArea)
            {
                var indent = CalculateIndent(wholeArea);
                return MinimizedIconRectangle(wholeArea, indent);
            }
            public RectangleF MinimizedIconRectangle(RectangleF area, float indent)
            {
                const int minimizeRectangleSize = 8;
                //return new Rectangle((int)(m_area.Location.Plus(textStart).X + textSize.Width + 2), (int)(m_area.Location.Plus(textStart).Y + (textSize.Height - minimizeRectangleSize) / 2), minimizeRectangleSize, minimizeRectangleSize);
                return new RectangleF(area.X + indent + 2 - (area.Height - 6) + minimizeRectangleSize / 2,
                                      area.Y + 3 + (area.Height - 6) / 2 - minimizeRectangleSize / 2,
                                      minimizeRectangleSize, minimizeRectangleSize);
            }

            public void DrawSelection(Graphics g, RectangleF area, bool selected, bool conversationSelected, ColorScheme scheme)
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

            public void Draw(Graphics g, VisibilityFilter filter, RectangleF area, ColorScheme scheme)
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

            public void DrawText(Arthur.NativeTextRenderer renderer, VisibilityFilter Visibility, RectangleF area, ColorScheme scheme)
            {
                //g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

                if (m_textBox == null)
                {
                    float indent = CalculateIndent(area);
                    //g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    var textArea = CalculateTextArea(area, indent);

                    renderer.DrawString(Text, SystemFonts.MessageBoxFont, scheme.Foreground, textArea.Location.Plus(MyTextBox.BorderSize, MyTextBox.BorderSize).Round());
                    //TextRenderer.DrawText(g, Text, SystemFonts.MessageBoxFont, textArea.Location.Plus(MyTextBox.BORDER_SIZE, MyTextBox.BORDER_SIZE).Round(), ColorScheme.Foreground, Color.Transparent, TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);

                    //g.DrawString(Text, SystemFonts.MessageBoxFont, ColorScheme.ForegroundBrush, TextArea.Location.Plus(MyTextBox.BORDER_SIZE, MyTextBox.BORDER_SIZE));
                }
            }

            protected virtual void DrawMinimizeIcon(Graphics g, RectangleF minimizeIconRectangle, VisibilityFilter filter,ColorScheme scheme) { }

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

            public abstract void DrawTree(Graphics g, RectangleF iconRectangle, VisibilityFilter filter, ColorScheme scheme);
            public abstract void DrawIcon(Graphics g, RectangleF iconRectangle);
            public abstract IEnumerable<Item> AllItems(VisibilityFilter filter);
            public abstract IEnumerable<Item> Children(VisibilityFilter filter);

            public void SetIndentLevel(int indentLevel)
            {
                m_indentLevel = indentLevel;
            }
            public int IndentLevel { get { return m_indentLevel; } }

            public abstract ContainerItem SpawnLocation { get; }

            public void StartRenaming(int cursorPos, Control control)
            {
                if (m_textBox == null)
                {
                    var area = m_area();
                    var indent = CalculateIndent(area);
                    var textArea = CalculateTextArea(area, indent);
                    m_textBox = new MyTextBox(control, () => new RectangleF(textArea.Location.Plus(ToControlTransform().OffsetX, ToControlTransform().OffsetY), textArea.Size), MyTextBox.InputFormEnum.FileName, null);
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
