using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Conversation;
using Conversation.Serialization;

namespace ConversationEditor
{
    public interface IGraphEditorControl<TNode> where TNode : IRenderable<IGUI>
    {
        void CopySelection();

        void DuplicateSelection();

        void SelectAll();

        void Paste(Point? point);

        void UngroupSelection();

        void GroupSelection();

        Control AsControl();

        void SelectNode(TNode node);

        float GraphScale { get; set; }

        bool ShowIDs { get; set; }

        IConversationEditorControlData<TNode, TransitionNoduleUIInfo> CurrentFile
        {
            get;
            set;
        }

        IDataSource DataSource { get; }

        void AddNode(IEditableGenerator nn, Point p);

        void Insert(Point? p, Tuple<IEnumerable<GraphAndUI<NodeUIData>>, IEnumerable<NodeGroup>> additions);
    }
}
