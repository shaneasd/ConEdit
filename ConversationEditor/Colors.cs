using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace ConversationEditor
{
    public class ColorScheme
    {
        private Color m_connectorColor;

        public ColorScheme(Color connectorColor)
        {
            m_connectorColor = connectorColor;
        }

        public ColorScheme()
        {
            m_connectorColor = Color.Black;
        }

        public static readonly Color SelectionRectangle = Color.FromArgb(128, Color.Blue);
        public static readonly Pen ControlBorder = new Pen(Color.Black, 1);
        public static readonly Color FormBackground = Color.FromArgb(45, 45, 45);
        public static readonly Color Background = Color.FromArgb(56, 56, 56);
        public static readonly Color Foreground = Color.FromArgb(205, 205, 205);
        public static readonly Pen ForegroundPen = new Pen(Foreground);
        public static readonly SolidBrush ForegroundBrush = new SolidBrush(Foreground);
        public static readonly SolidBrush BackgroundBrush = new SolidBrush(Background);
        public static readonly Color GroupBackgroundSelected = Color.FromArgb(92, Color.White);
        public static readonly Color GroupBackgroundUnselected = Color.FromArgb(51, Color.White);
        public static readonly Color Grid = Color.FromArgb(42, 42, 42);
        public static readonly Color MinorGrid = Color.FromArgb(49, 49, 49);
        public Color Connectors { get { return m_connectorColor; } }
        //public static readonly Color Connectors = Color.Black;
        public static readonly Color SelectedConnectors = Foreground;
        public static readonly Color SelectedConversationListItemBorder = Color.Black;
        public static readonly Color SelectedConversationListItemPrimaryBackground = Color.FromArgb(96, 96, 96);
        public static readonly Color SelectedConversationListItemSecondaryBackground = Color.FromArgb(76, 76, 76);
        public static readonly Color SelectedText = Color.Blue;
        public static readonly ToolStripProfessionalRenderer ContextMenu = new ToolStripRenderer();

        private class ToolStripRenderer : ToolStripProfessionalRenderer
        {
            public ToolStripRenderer()
                : base(new ContextMenuClass())
            {
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
                    using (Brush b = new SolidBrush(SelectedConversationListItemPrimaryBackground))
                        e.Graphics.FillRectangle(b, Rectangle.FromLTRB(-99999, -99999, 99999, 99999));
                }
                else
                    base.OnRenderMenuItemBackground(e);
            }

            protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
            {
                e.ArrowColor = Foreground;
                base.OnRenderArrow(e);
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = Foreground;
                base.OnRenderItemText(e);
            }

            protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
            {
                base.OnRenderItemCheck(e);
            }
        }

        private class ContextMenuClass : ProfessionalColorTable
        {
            public override Color ToolStripDropDownBackground
            {
                get
                {
                    return ColorScheme.Background;
                }
            }

            public override Color MenuStripGradientBegin
            {
                get
                {
                    return ColorScheme.Background;
                }
            }

            public override Color MenuStripGradientEnd
            {
                get
                {
                    return ColorScheme.Background;
                }
            }

            public override Color ButtonSelectedHighlight
            {
                get
                {
                    return SelectedConversationListItemPrimaryBackground;
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
                    return SelectedConversationListItemPrimaryBackground;
                }
            }

            public override Color ImageMarginGradientEnd
            {
                get
                {
                    return SelectedConversationListItemPrimaryBackground;
                }
            }

            public override Color ImageMarginGradientMiddle
            {
                get
                {
                    return SelectedConversationListItemPrimaryBackground;
                }
            }

            public override Color MenuItemPressedGradientBegin
            {
                get
                {
                    return SelectedConversationListItemPrimaryBackground;
                }
            }

            public override Color MenuItemPressedGradientEnd
            {
                get
                {
                    return SelectedConversationListItemPrimaryBackground;
                }
            }

            public override Color MenuItemSelectedGradientBegin
            {
                get
                {
                    return SelectedConversationListItemPrimaryBackground;
                }
            }

            public override Color MenuItemSelectedGradientEnd
            {
                get
                {
                    return SelectedConversationListItemPrimaryBackground;
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
                    return SelectedConversationListItemPrimaryBackground;
                }
            }

            public override Color CheckPressedBackground
            {
                get
                {
                    return SelectedConversationListItemPrimaryBackground;
                }
            }

            public override Color CheckSelectedBackground
            {
                get
                {
                    return SelectedConversationListItemPrimaryBackground;
                }
            }

            public override Color MenuItemPressedGradientMiddle
            {
                get
                {
                    return SelectedConversationListItemPrimaryBackground;
                }
            }

            public override Color ImageMarginRevealedGradientBegin
            {
                get
                {
                    return SelectedConversationListItemPrimaryBackground;
                }
            }

            public override Color ImageMarginRevealedGradientEnd
            {
                get
                {
                    return SelectedConversationListItemPrimaryBackground;
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
                    return ColorScheme.Foreground;
                }
            }

            public override Color SeparatorDark
            {
                get
                {
                    return ColorScheme.Foreground;
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
                    return SelectedConversationListItemPrimaryBackground;
                }
            }

            public override Color ButtonSelectedGradientEnd
            {
                get
                {
                    return SelectedConversationListItemPrimaryBackground;
                }
            }

            public override Color ButtonSelectedHighlightBorder
            {
                get
                {
                    return SelectedConversationListItemBorder;
                }
            }


            public override Color ButtonPressedHighlight
            {
                get
                {
                    return SelectedConversationListItemPrimaryBackground;
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
