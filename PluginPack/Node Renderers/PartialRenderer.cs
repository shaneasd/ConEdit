﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConversationEditor;
using Conversation;
using System.Drawing;
using System.IO;
using Utilities;

namespace PluginPack
{
    public class PartialRendererFactory : NodeUI.IFactory
    {
        public static PartialRendererFactory Instance { get; } = new PartialRendererFactory();
        public bool WillRender(Id<NodeTypeTemp> nodeType)
        {
            return true;
        }

        public string DisplayName => "Partial Renderer";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "IL might look different to the code but there's nothing you can do to fix the warning")]
        public INodeGui GetRenderer(ConversationNode<INodeGui> n, PointF p, Func<Id<LocalizedStringType>, Id<LocalizedText>, string> localizer, Func<IDataSource> datasource)
        {
            return new PartialRenderer(n, p, localizer, datasource);
        }

        public Guid Guid => Guid.Parse("a24b2f32-571d-4e3b-b091-2712d412ac5e");
    }

    public class PartialRenderer : EditableUI
    {
        private Func<IDataSource> m_datasource;

        public PartialRenderer(ConversationNode<ConversationEditor.INodeGui> node, PointF p, Func<Id<LocalizedStringType>, Id<LocalizedText>, string> localizer, Func<IDataSource> datasource) :
            base(node, p, localizer)
        {
            m_datasource = datasource;
        }

        protected override bool ShouldRender(IParameter p)
        {
            if (p == null)
                throw new ArgumentNullException(nameof(p));
            if (p is UnknownParameter) //TODO: Is there a way to determine this without reflection?
                return true;
            var config = m_datasource().GetNode(Node.Data.NodeTypeId).GetParameterConfig(p.Id);
            return !DoNotRenderConfig.TryGet(config);
        }
    }
}
