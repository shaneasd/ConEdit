using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace ConversationEditor
{
    public class ColorScheme : Utilities.ColorScheme
    {
        public ColorScheme()
        {
            Connectors = Color.Black;
            ContextMenu = new ToolStripRenderer(this);
            Hatch = new HatchBrush(HatchStyle.Percent50, SelectionRectangle);
        }

        public Color Connectors { get; set; }
        public readonly Color SelectedConnectors = Defaults.Foreground;
        public readonly Color SelectedConversationListItemBorder = Color.Black;
        public readonly Color SelectedConversationListItemPrimaryBackground = Color.FromArgb(96, 96, 96);
        public readonly Color SelectedConversationListItemSecondaryBackground = Color.FromArgb(76, 76, 76);
        public readonly Brush Hatch;
        public readonly ToolStripProfessionalRenderer ContextMenu;
        public readonly Pen TreePen = new Pen(Defaults.Foreground) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot };

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
