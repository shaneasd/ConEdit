using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using Utilities;
using Utilities.UI;

namespace ConversationEditor
{
    public class ColorScheme : Disposable, GreyScrollBar.IColorScheme, DrawWindow.IColorScheme
    {
        public ColorScheme()
        {
            Connectors = Color.Black;
            m_contextMenu = new ToolStripRenderer(this);
            m_hatch = new HatchBrush(HatchStyle.Percent50, SelectionRectangle);
        }

        public Color Connectors { get; set; }
        private readonly Color m_selectedConnectors = Defaults.Foreground;
        private readonly Color m_selectedConversationListItemBorder = Color.Black;
        private readonly Color m_selectedConversationListItemPrimaryBackground = Color.FromArgb(96, 96, 96);
        private readonly Color m_selectedConversationListItemSecondaryBackground = Color.FromArgb(76, 76, 76);
        private readonly Brush m_hatch;
        private readonly ToolStripProfessionalRenderer m_contextMenu;

        public Color SelectedConnectors { get { return m_selectedConnectors; } }
        public Color SelectedConversationListItemBorder { get { return m_selectedConversationListItemBorder; } }
        public Color SelectedConversationListItemPrimaryBackground { get { return m_selectedConversationListItemPrimaryBackground; } }
        public Color SelectedConversationListItemSecondaryBackground { get { return m_selectedConversationListItemSecondaryBackground; } }
        public Brush Hatch { get { return m_hatch; } }
        public ToolStripProfessionalRenderer ContextMenu { get { return m_contextMenu; } }

        private static Pen MakeTreePen()
        {
            var result = new Pen(Defaults.Foreground);
            try
            {
                result.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
            }
            catch
            {
                result.Dispose();
                throw;
            }
            return result;
        }

        public Pen TreePen { get; } = MakeTreePen();

        protected static class Defaults
        {
            public static Color Foreground { get; } = Color.FromArgb(205, 205, 205);
            public static Color Background { get; } = Color.FromArgb(56, 56, 56);
        }

        public Color SelectionRectangle { get; } = Color.FromArgb(128, Color.Blue);

        private readonly Pen m_controlBorder = new Pen(Color.Black, 1);
        public Pen ControlBorder { get { return m_controlBorder; } }

        private readonly Color m_formBackground = Color.FromArgb(45, 45, 45);
        public Color FormBackground { get { return m_formBackground; } }

        public Color Background { get; } = Color.FromArgb(56, 56, 56);

        private readonly Color m_foreground = Color.FromArgb(205, 205, 205);
        public Color Foreground { get { return m_foreground; } }

        private readonly Pen m_foregroundPen = new Pen(Defaults.Foreground);
        public Pen ForegroundPen { get { return m_foregroundPen; } }

        private readonly SolidBrush m_foregroundBrush = new SolidBrush(Defaults.Foreground);
        public SolidBrush ForegroundBrush { get { return m_foregroundBrush; } }

        private readonly SolidBrush m_backgroundBrush = new SolidBrush(Defaults.Background);
        public SolidBrush BackgroundBrush { get { return m_backgroundBrush; } }

        private readonly Color m_groupBackgroundSelected = Color.FromArgb(92, Color.White);
        public Color GroupBackgroundSelected { get { return m_groupBackgroundSelected; } }

        private readonly Color m_groupBackgroundUnselected = Color.FromArgb(51, Color.White);
        public Color GroupBackgroundUnselected { get { return m_groupBackgroundUnselected; } }

        private readonly Color m_grid = Color.FromArgb(42, 42, 42);
        public Color Grid { get { return m_grid; } }

        private readonly Color m_minorGrid = Color.FromArgb(49, 49, 49);
        public Color MinorGrid { get { return m_minorGrid; } }

        private readonly Color m_selectedText = Color.Blue;
        public Color SelectedText { get { return m_selectedText; } }

        public DrawWindow.IColorScheme DrawWindowColorScheme
        {
            get
            {
                return this;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_controlBorder.Dispose();
                m_foregroundPen.Dispose();
                m_foregroundBrush.Dispose();
                m_backgroundBrush.Dispose();
                m_hatch.Dispose();
            }
        }

        private class ToolStripRenderer : ToolStripProfessionalRenderer
        {
            private ColorScheme m_scheme;
            public ToolStripRenderer(ColorScheme scheme)
                : base(new ContextMenuClass(scheme))
            {
                m_scheme = scheme;
            }

            //protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            //{
            //    //base.OnRenderToolStripBorder(e);
            //    //e.Graphics.DrawRectangle(Pens.Red, e.AffectedBounds);
            //    //e.Graphics.FillRectangle(Brushes.Red, e.AffectedBounds);
            //}

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                if (e.Item.Selected && e.Item.IsOnDropDown)
                {
                    using (Brush b = new SolidBrush(m_scheme.SelectedConversationListItemPrimaryBackground))
                        e.Graphics.FillRectangle(b, Rectangle.FromLTRB(-99999, -99999, 99999, 99999));
                }
                else
                    base.OnRenderMenuItemBackground(e);
            }

            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
            {
                e.ArrowColor = m_scheme.Foreground;
                base.OnRenderArrow(e);
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = m_scheme.Foreground;
                base.OnRenderItemText(e);
            }

            protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
            {
                base.OnRenderItemCheck(e);
            }
        }

        private class ContextMenuClass : ProfessionalColorTable
        {
            ColorScheme m_scheme;
            public ContextMenuClass(ColorScheme scheme)
            {
                m_scheme = scheme;
            }

            public override Color ToolStripDropDownBackground
            {
                get
                {
                    return m_scheme.Background;
                }
            }

            public override Color MenuStripGradientBegin
            {
                get
                {
                    return m_scheme.Background;
                }
            }

            public override Color MenuStripGradientEnd
            {
                get
                {
                    return m_scheme.Background;
                }
            }

            public override Color ButtonSelectedHighlight
            {
                get
                {
                    return m_scheme.SelectedConversationListItemPrimaryBackground;
                }
            }

            public override Color MenuBorder
            {
                get
                {
                    return Color.Black;
                }
            }

            public override Color MenuItemBorder
            {
                get
                {
                    return Color.Black;
                }
            }

            public override Color ImageMarginGradientBegin
            {
                get
                {
                    return m_scheme.SelectedConversationListItemPrimaryBackground;
                }
            }

            public override Color ImageMarginGradientEnd
            {
                get
                {
                    return m_scheme.SelectedConversationListItemPrimaryBackground;
                }
            }

            public override Color ImageMarginGradientMiddle
            {
                get
                {
                    return m_scheme.SelectedConversationListItemPrimaryBackground;
                }
            }

            public override Color MenuItemPressedGradientBegin
            {
                get
                {
                    return m_scheme.SelectedConversationListItemPrimaryBackground;
                }
            }

            public override Color MenuItemPressedGradientEnd
            {
                get
                {
                    return m_scheme.SelectedConversationListItemPrimaryBackground;
                }
            }

            public override Color MenuItemSelectedGradientBegin
            {
                get
                {
                    return m_scheme.SelectedConversationListItemPrimaryBackground;
                }
            }

            public override Color MenuItemSelectedGradientEnd
            {
                get
                {
                    return m_scheme.SelectedConversationListItemPrimaryBackground;
                }
            }

            public override Color ButtonSelectedBorder
            {
                get
                {
                    return Color.Black;
                }
            }

            public override Color CheckBackground
            {
                get
                {
                    return m_scheme.SelectedConversationListItemPrimaryBackground;
                }
            }

            public override Color CheckPressedBackground
            {
                get
                {
                    return m_scheme.SelectedConversationListItemPrimaryBackground;
                }
            }

            public override Color CheckSelectedBackground
            {
                get
                {
                    return m_scheme.SelectedConversationListItemPrimaryBackground;
                }
            }

            public override Color MenuItemPressedGradientMiddle
            {
                get
                {
                    return m_scheme.SelectedConversationListItemPrimaryBackground;
                }
            }

            public override Color ImageMarginRevealedGradientBegin
            {
                get
                {
                    return m_scheme.SelectedConversationListItemPrimaryBackground;
                }
            }

            public override Color ImageMarginRevealedGradientEnd
            {
                get
                {
                    return m_scheme.SelectedConversationListItemPrimaryBackground;
                }
            }

            public override Color ImageMarginRevealedGradientMiddle
            {
                get
                {
                    return Color.Red;
                }
            }

            public override Color SeparatorLight
            {
                get
                {
                    return m_scheme.Foreground;
                }
            }

            public override Color SeparatorDark
            {
                get
                {
                    return m_scheme.Foreground;
                }
            }

            public override Color StatusStripGradientBegin
            {
                get
                {
                    return Color.Red;
                }
            }

            public override Color StatusStripGradientEnd
            {
                get
                {
                    return Color.Red;
                }
            }

            public override Color ToolStripBorder
            {
                get
                {
                    return Color.Red;
                }
            }

            public override Color ToolStripGradientEnd
            {
                get
                {
                    return Color.Red;
                }
            }

            public override Color ToolStripGradientMiddle
            {
                get
                {
                    return Color.Red;
                }
            }

            public override Color ToolStripPanelGradientEnd
            {
                get
                {
                    return Color.Red;
                }
            }

            public override Color MenuItemSelected
            {
                get
                {
                    return Color.Red;
                }
            }

            public override Color OverflowButtonGradientBegin
            {
                get
                {
                    return Color.Red;
                }
            }

            public override Color OverflowButtonGradientEnd
            {
                get
                {
                    return Color.Red;
                }
            }

            public override Color OverflowButtonGradientMiddle
            {
                get
                {
                    return Color.Red;
                }
            }

            public override Color RaftingContainerGradientBegin
            {
                get
                {
                    return Color.Red;
                }
            }

            public override Color RaftingContainerGradientEnd
            {
                get
                {
                    return Color.Red;
                }
            }

            public override Color ButtonPressedHighlightBorder
            {
                get
                {
                    return Color.Red;
                }
            }

            public override Color ButtonSelectedGradientBegin
            {
                get
                {
                    return m_scheme.SelectedConversationListItemPrimaryBackground;
                }
            }

            public override Color ButtonSelectedGradientEnd
            {
                get
                {
                    return m_scheme.SelectedConversationListItemPrimaryBackground;
                }
            }

            public override Color ButtonSelectedHighlightBorder
            {
                get
                {
                    return m_scheme.SelectedConversationListItemBorder;
                }
            }


            public override Color ButtonPressedHighlight
            {
                get
                {
                    return m_scheme.SelectedConversationListItemPrimaryBackground;
                }
            }

            public override Color ButtonPressedGradientMiddle
            {
                get
                {
                    return Color.Red;
                }
            }

            public override Color ButtonPressedGradientEnd
            {
                get
                {
                    return Color.Red;
                }
            }

            public override Color ButtonCheckedHighlightBorder
            {
                get
                {
                    return Color.Red;
                }
            }

            public override Color ButtonCheckedHighlight
            {
                get
                {
                    return Color.Red;
                }
            }

            public override Color ButtonCheckedGradientMiddle
            {
                get
                {
                    return Color.Red;
                }
            }

            public override Color ButtonCheckedGradientEnd
            {
                get
                {
                    return Color.Red;
                }
            }

            public override Color ButtonPressedGradientBegin
            {
                get
                {
                    return Color.Red;
                }
            }

            public override Color GripDark
            {
                get
                {
                    return Color.Red;
                }
            }

            public override Color GripLight
            {
                get
                {
                    return Color.Red;
                }
            }

            public override Color ButtonPressedBorder
            {
                get
                {
                    return Color.Red;
                }
            }

            public override Color ButtonSelectedGradientMiddle
            {
                get
                {
                    return Color.Red;
                }
            }

            public override Color ToolStripGradientBegin
            {
                get
                {
                    return Color.Red;
                }
            }

            public override Color ToolStripPanelGradientBegin
            {
                get
                {
                    return Color.Red;
                }
            }

            public override Color ButtonCheckedGradientBegin
            {
                get
                {
                    return Color.Red;
                }
            }

            public override Color ToolStripContentPanelGradientBegin
            {
                get
                {
                    return Color.Red;
                }
            }

            public override Color ToolStripContentPanelGradientEnd
            {
                get
                {
                    return Color.Red;
                }
            }
        }
    }
}
