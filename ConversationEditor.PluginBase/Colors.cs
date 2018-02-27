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
    public class ColorScheme : Disposable, IColorScheme
    {
        /// <summary>
        /// A default color scheme to be used by controls that have not had their color scheme set
        /// </summary>
        public static IColorScheme Default { get; } = new ColorScheme();

        public ColorScheme()
        {
            Connectors = Color.Black;
            ContextMenu = new ToolStripRenderer(this);
            m_hatch = new HatchBrush(HatchStyle.Percent50, SelectionRectangle);
        }

        public Color Connectors { get; set; }
        private readonly Color m_selectedConnectors = Defaults.Foreground;
        private readonly Color m_selectedConversationListItemBorder = Color.Black;
        private readonly Color m_selectedConversationListItemPrimaryBackground = Color.FromArgb(96, 96, 96);
        private readonly Color m_selectedConversationListItemSecondaryBackground = Color.FromArgb(76, 76, 76);
        private readonly Brush m_hatch;

        public Color SelectedConnectors => m_selectedConnectors;
        public Color SelectedConversationListItemBorder => m_selectedConversationListItemBorder;
        public Color SelectedConversationListItemPrimaryBackground => m_selectedConversationListItemPrimaryBackground;
        public Color SelectedConversationListItemSecondaryBackground => m_selectedConversationListItemSecondaryBackground;
        public Brush Hatch => m_hatch;
        public ToolStripProfessionalRenderer ContextMenu { get; }

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

        public Pen ControlBorder { get; } = new Pen(Color.Black, 1);

        public Color FormBackground { get; } = Color.FromArgb(45, 45, 45);
        public Color MenuBackground { get; } = Color.FromArgb(45, 45, 45);
        public Color Background { get; } = Color.FromArgb(56, 56, 56);
        public Color Foreground { get; } = Color.FromArgb(205, 205, 205);

        public Pen ForegroundPen { get; } = new Pen(Defaults.Foreground);

        private readonly SolidBrush m_foregroundBrush = new SolidBrush(Defaults.Foreground);
        public SolidBrush ForegroundBrush => m_foregroundBrush;

        private readonly SolidBrush m_backgroundBrush = new SolidBrush(Defaults.Background);
        public SolidBrush BackgroundBrush => m_backgroundBrush;

        private readonly Color m_groupBackgroundSelected = Color.FromArgb(92, Color.White);
        public Color GroupBackgroundSelected => m_groupBackgroundSelected;

        private readonly Color m_groupBackgroundUnselected = Color.FromArgb(51, Color.White);
        public Color GroupBackgroundUnselected => m_groupBackgroundUnselected;

        private readonly Color m_grid = Color.FromArgb(42, 42, 42);
        public Color Grid => m_grid;

        private readonly Color m_minorGrid = Color.FromArgb(49, 49, 49);
        public Color MinorGrid => m_minorGrid;

        private readonly Color m_selectedText = Color.Blue;
        public Color SelectedText => m_selectedText;

        public DrawWindow.IColorScheme DrawWindowColorScheme => this;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ControlBorder.Dispose();
                ForegroundPen.Dispose();
                m_foregroundBrush.Dispose();
                m_backgroundBrush.Dispose();
                m_hatch.Dispose();
            }
        }

        private class ToolStripRenderer : ToolStripProfessionalRenderer
        {
            private IColorScheme m_scheme;
            public ToolStripRenderer(IColorScheme scheme)
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
            IColorScheme m_scheme;
            public ContextMenuClass(IColorScheme scheme)
            {
                m_scheme = scheme;
            }

            //TODO: Run this on a windows 10 machine and fix the red bits.
            //      - When you have a dropdown with an arrow to scroll through it, like for an enum, when you click the arrow it highlights in red
            //      - Same goes for selecting an item from the list actually
            public override Color ToolStripDropDownBackground => m_scheme.MenuBackground;

            public override Color MenuStripGradientBegin => m_scheme.MenuBackground;

            public override Color MenuStripGradientEnd => m_scheme.MenuBackground;

            public override Color ButtonSelectedHighlight => m_scheme.SelectedConversationListItemPrimaryBackground;

            public override Color MenuBorder => Color.Black;

            public override Color MenuItemBorder => Color.Black;

            public override Color ImageMarginGradientBegin => m_scheme.SelectedConversationListItemPrimaryBackground;

            public override Color ImageMarginGradientEnd => m_scheme.SelectedConversationListItemPrimaryBackground;

            public override Color ImageMarginGradientMiddle => m_scheme.SelectedConversationListItemPrimaryBackground;

            public override Color MenuItemPressedGradientBegin => m_scheme.SelectedConversationListItemPrimaryBackground;

            public override Color MenuItemPressedGradientEnd => m_scheme.SelectedConversationListItemPrimaryBackground;

            public override Color MenuItemSelectedGradientBegin => m_scheme.SelectedConversationListItemPrimaryBackground;

            public override Color MenuItemSelectedGradientEnd => m_scheme.SelectedConversationListItemPrimaryBackground;

            public override Color ButtonSelectedBorder => Color.Black;

            public override Color CheckBackground => m_scheme.SelectedConversationListItemPrimaryBackground;

            public override Color CheckPressedBackground => m_scheme.SelectedConversationListItemPrimaryBackground;

            public override Color CheckSelectedBackground => m_scheme.SelectedConversationListItemPrimaryBackground;

            public override Color MenuItemPressedGradientMiddle => m_scheme.SelectedConversationListItemPrimaryBackground;

            public override Color ImageMarginRevealedGradientBegin => m_scheme.SelectedConversationListItemPrimaryBackground;

            public override Color ImageMarginRevealedGradientEnd => m_scheme.SelectedConversationListItemPrimaryBackground;

            public override Color ImageMarginRevealedGradientMiddle => Color.Red;

            public override Color SeparatorLight => m_scheme.Foreground;

            public override Color SeparatorDark => m_scheme.Foreground;

            public override Color StatusStripGradientBegin => Color.Red;

            public override Color StatusStripGradientEnd => Color.Red;

            public override Color ToolStripBorder => Color.Red;

            public override Color ToolStripGradientEnd => Color.Red;

            public override Color ToolStripGradientMiddle => Color.Red;

            public override Color ToolStripPanelGradientEnd => Color.Red;

            public override Color MenuItemSelected => Color.Red;

            public override Color OverflowButtonGradientBegin => Color.Red;

            public override Color OverflowButtonGradientEnd => Color.Red;

            public override Color OverflowButtonGradientMiddle => Color.Red;

            public override Color RaftingContainerGradientBegin => Color.Red;

            public override Color RaftingContainerGradientEnd => Color.Red;

            public override Color ButtonPressedHighlightBorder => Color.Red;

            public override Color ButtonSelectedGradientBegin => m_scheme.SelectedConversationListItemPrimaryBackground;

            public override Color ButtonSelectedGradientEnd => m_scheme.SelectedConversationListItemPrimaryBackground;

            public override Color ButtonSelectedHighlightBorder => m_scheme.SelectedConversationListItemBorder;


            public override Color ButtonPressedHighlight => m_scheme.SelectedConversationListItemPrimaryBackground;

            public override Color ButtonPressedGradientMiddle => Color.Red;

            public override Color ButtonPressedGradientEnd => Color.Red;

            public override Color ButtonCheckedHighlightBorder => Color.Red;

            public override Color ButtonCheckedHighlight => Color.Red;

            public override Color ButtonCheckedGradientMiddle => Color.Red;

            public override Color ButtonCheckedGradientEnd => Color.Red;

            public override Color ButtonPressedGradientBegin => Color.Red;

            public override Color GripDark => Color.Red;

            public override Color GripLight => Color.Red;

            public override Color ButtonPressedBorder => Color.Red;

            public override Color ButtonSelectedGradientMiddle => Color.Red;

            public override Color ToolStripGradientBegin => Color.Red;

            public override Color ToolStripPanelGradientBegin => Color.Red;

            public override Color ButtonCheckedGradientBegin => Color.Red;

            public override Color ToolStripContentPanelGradientBegin => Color.Red;

            public override Color ToolStripContentPanelGradientEnd => Color.Red;
        }
    }
}
