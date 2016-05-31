using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConversationEditor;
using Conversation;
using System.Drawing;
using System.IO;
using Utilities;

namespace Clandestine
{
    public class PartialRendererFactory : NodeUI.IFactory
    {
        private static readonly PartialRendererFactory s_instance = new PartialRendererFactory();
        public static PartialRendererFactory Instance { get { return s_instance; } }

        public bool WillRender(Id<NodeTypeTemp> nodeType)
        {
            return true;
        }

        public string DisplayName
        {
            get { return "Partial Renderer"; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "IL might look different to the code but there's nothing you can do to fix the warning")]
        public INodeGui GetRenderer(ConversationNode<INodeGui> n, PointF p, Func<Id<LocalizedText>, string> localizer, Func<IDataSource> datasource)
        {
            return new PartialRenderer(n, p, localizer, datasource);
        }

        public Guid Guid
        {
            get { return Guid.Parse("a24b2f32-571d-4e3b-b091-2712d412ac5e"); }
        }
    }

    public class PartialRenderer : EditableUI
    {
        private Func<IDataSource> m_datasource;

        public PartialRenderer(ConversationNode<ConversationEditor.INodeGui> node, PointF p, Func<Id<LocalizedText>, string> localizer, Func<IDataSource> datasource) :
            base(node, p, localizer)
        {
            m_datasource = datasource;
        }

        protected override bool ShouldRender(Parameter p)
        {
            if (p == null)
                throw new ArgumentNullException(nameof(p));
            var config = m_datasource().GetNode(Node.Type).GetParameterConfig(p.Id);
            return !DoNotRenderConfig.TryGet(config);
        }
    }
}
