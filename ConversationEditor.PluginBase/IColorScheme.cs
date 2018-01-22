using System.Drawing;
using System.Windows.Forms;
using Utilities.UI;

namespace ConversationEditor
{
    public interface IColorScheme : GreyScrollBar.IColorScheme, DrawWindow.IColorScheme
    {
        new Color Background { get; }
        Color MenuBackground { get; }
        SolidBrush BackgroundBrush { get; }
        Color Connectors { get; set; }
        ToolStripProfessionalRenderer ContextMenu { get; }
        Pen ControlBorder { get; }
        Color Foreground { get; }
        SolidBrush ForegroundBrush { get; }
        Pen ForegroundPen { get; }
        Color FormBackground { get; }
        Color Grid { get; }
        Color GroupBackgroundSelected { get; }
        Color GroupBackgroundUnselected { get; }
        Brush Hatch { get; }
        Color MinorGrid { get; }
        Color SelectedConnectors { get; }
        Color SelectedConversationListItemBorder { get; }
        Color SelectedConversationListItemPrimaryBackground { get; }
        Color SelectedConversationListItemSecondaryBackground { get; }
        Color SelectedText { get; }
        Color SelectionRectangle { get; }
        Pen TreePen { get; }
    }
}