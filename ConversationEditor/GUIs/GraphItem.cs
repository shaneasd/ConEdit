using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Utilities;
using ConversationNode = Conversation.ConversationNode<Conversation.INodeGUI>;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace ConversationEditor
{
    public partial class ProjectExplorer
    {
        public class ConversationItem : GraphItem<IConversationFile>
        {
            public ConversationItem(Func<RectangleF> area, IConversationFile item, IProject project, ContainerItem parent, Bitmap icon, Bitmap missingIcon, Func<Matrix> toControlTransform)
                : base(area, item, project, parent, icon, missingIcon, f => f.Conversations, toControlTransform)
            {
            }

            internal override string CanSelect()
            {
                return m_project.CanModifyConversations ? null : "Changes cannot be made to conversations while there are unsaved domain changes";
            }
        }

        public class DomainItem : GraphItem<IDomainFile>
        {
            public DomainItem(Func<RectangleF> area, IDomainFile item, IProject project, ContainerItem parent, Bitmap icon, Bitmap missingIcon, Func<Matrix> toControlTransform)
                : base(area, item, project, parent, icon, missingIcon, f => f.Domains, toControlTransform)
            {
            }

            internal override string CanSelect()
            {
                return m_project.CanModifyDomain ? null : "Changes cannot be made to domain while there are unsaved conversation changes";
            }
        }

        public abstract class GraphItem<T> : RealLeafItem<T, T> where T : IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo>
        {
            private Bitmap m_icon;
            private Bitmap m_missingIcon;
            public GraphItem(Func<RectangleF> area, T item, IProject project, ContainerItem parent, Bitmap icon, Bitmap missingIcon, Func<VisibilityFilter, bool> filter, Func<Matrix> toControlTransform)
                : base(area, item, icon, project, parent, filter, toControlTransform)
            {
                m_icon = icon;
                m_missingIcon = missingIcon;
            }

            public override void DrawIcon(Graphics g, RectangleF iconRectangle)
            {
                if (this.Item.Errors.Any())
                    g.DrawImage(m_missingIcon, iconRectangle);
                else
                    g.DrawImage(m_icon, iconRectangle);
            }

            internal override bool Select(ref Item m_selectedItem, ref Item m_selectedEditable)
            {
                if (Item.Errors.Any())
                {
                    var dr = MessageBox.Show("Document has errors. These errors must be automatically resolved before the document can be edited. This may have undesirable effects on the integrity of the document (once saved). Do you want to continue?", "Resolve document errors?", MessageBoxButtons.OKCancel);
                    if (dr == DialogResult.Cancel)
                        return false;
                    else
                        Item.ClearErrors();
                }

                m_selectedEditable = this;
                return base.Select(ref m_selectedItem, ref m_selectedEditable);
            }
        }
    }
}
