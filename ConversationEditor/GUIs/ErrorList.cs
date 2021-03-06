﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utilities;
using Conversation;
using Utilities.UI;

namespace ConversationEditor
{
    using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGui>;

    internal partial class ErrorList : UserControl
    {
        private class Element : IErrorListElement
        {
            public IEnumerable<ConversationNode> Nodes { get; private set; }
            public IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo> File { get; private set; }
            public string Message { get; private set; }

            public Element(ConversationNode node, IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo> file, string message)
            {
                Nodes = node.Only();
                File = file;
                Message = message;
            }

            public Element(ConversationError<ConversationNode> error, IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo> file)
            {
                Nodes = error.Nodes;
                File = file;
                Message = error.Message;
            }

            public IEnumerator<Tuple<ConversationNode, IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo>>> MakeEnumerator()
            {
                if (Nodes.Any())
                    return Nodes.Select(n => new Tuple<ConversationNode, IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo>>(n, File)).InfiniteRepeat().GetEnumerator();
                else
                    return null;
            }
        }

        public static IErrorListElement MakeElement(ConversationNode node, IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo> file, string message)
        {
            return new Element(node, file, message);
        }

        public static IErrorListElement MakeElement(ConversationError<ConversationNode> error, IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo> file)
        {
            return new Element(error, file);
        }

        public ErrorList()
        {
            InitializeComponent();

            drawWindow1.Paint += drawWindow1_Paint;
            greyScrollBar1.Scrolled += () => drawWindow1.Invalidate(true);
            drawWindow1.MouseClick += (a, b) => drawWindow1_MouseClick(b);
            drawWindow1.MouseWheel += (a, b) => greyScrollBar1.MouseWheeled(b);
            Resize += (a, b) => UpdateScrollbar();
        }

        public event Action<ConversationNode, IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo>, BoolRef> HightlightNode;

        IEnumerator<Tuple<ConversationNode, IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo>>> m_nodeIterator;

        void drawWindow1_MouseClick(MouseEventArgs e)
        {
            var oldSelected = m_selectedItem;
            m_selectedItem = null;
            float y = -greyScrollBar1.Value;
            foreach (var item in m_items)
            {
                if (e.Y >= y && e.Y <= y + item.Height)
                {
                    m_selectedItem = item;
                    break;
                }
                y += item.Height;
            }
            drawWindow1.Invalidate(true);

            if (m_selectedItem != oldSelected)
            {
                if (m_selectedItem != null)
                {
                    m_nodeIterator = m_selectedItem.Error.MakeEnumerator();
                    if (m_nodeIterator != null)
                        m_nodeIterator.MoveNext();
                }
                else
                    m_nodeIterator = null;
            }
            else if (m_nodeIterator != null)
            {
                m_nodeIterator.MoveNext();
            }

            if (m_nodeIterator != null && m_nodeIterator.Current != null)
            {
                BoolRef success = true;
                HightlightNode.Execute(m_nodeIterator.Current.Item1, m_nodeIterator.Current.Item2, success);
            }
        }

        void drawWindow1_Paint(object sender, PaintEventArgs e)
        {
            float y = -greyScrollBar1.Value;
            foreach (var item in m_items)
            {
                item.Draw(ColorScheme, e.Graphics, y, drawWindow1.Width - 1, item == m_selectedItem);
                y += item.Height;
            }
            e.Graphics.DrawRectangle(ColorScheme.ControlBorder, new Rectangle(0, 0, drawWindow1.Width - 1, drawWindow1.Height - 1));
        }

        ErrorItem m_selectedItem = null;

        private class ErrorItem
        {
            const float HEIGHT = 23;
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
            public float Height => HEIGHT;

            IErrorListElement m_data;
            public IErrorListElement Error => m_data;

            public ErrorItem(IErrorListElement data)
            {
                m_data = data;
            }

            internal void Draw(IColorScheme scheme, Graphics g, float y, float width, bool selected)
            {
                using (Brush background = new SolidBrush(selected ? scheme.SelectedConversationListItemSecondaryBackground : scheme.Background))
                {
                    using (Brush text = new SolidBrush(scheme.Foreground))
                    {
                        g.FillRectangle(background, new RectangleF(0, y, width, HEIGHT));
                        g.DrawRectangle(scheme.ControlBorder, new RectangleF(0, y, width, HEIGHT));
                        g.DrawString(m_data.Message, SystemFonts.DefaultFont, text, new PointF(4, y + 5));
                    }
                }
            }
        }

        List<ErrorItem> m_items = new List<ErrorItem>();

        public void SetErrors(IEnumerable<IErrorListElement> errors)
        {
            m_items = errors.Select(n => new ErrorItem(n)).ToList();
            UpdateScrollbar();
            Invalidate(true);
        }

        private void UpdateScrollbar()
        {
            float totalHeight = m_items.Sum(i => i.Height);
            greyScrollBar1.Maximum = Math.Max(0.0f, totalHeight - Height);
            greyScrollBar1.PercentageCovered = Height / totalHeight;
        }

        IColorScheme m_scheme = ConversationEditor.ColorScheme.Default; //So the designer has something to work with
        public IColorScheme ColorScheme
        {
            get { return m_scheme; }
            set
            {
                m_scheme = value;
                //greyScrollBar1.ColorScheme = value;
                //drawWindow1.ColorScheme = value;
            }
        }
    }
}
