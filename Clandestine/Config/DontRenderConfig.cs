using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using ConversationEditor;
using System.Collections.ObjectModel;

namespace Clandestine
{
    public class DoNotRenderConfig : IConfigNodeDefinition
    {
        private static readonly Id<NodeTypeTemp> ID = Id<NodeTypeTemp>.Parse("56e826e6-2db8-42a6-8b29-53e44c2782ec");
        public Id<NodeTypeTemp> Id
        {
            get { return ID; }
        }

        public string Name
        {
            get { return "Don't Render"; }
        }

        public IEnumerable<IParameter> MakeParameters()
        {
            return Enumerable.Empty<IParameter>();
        }

        public static bool TryGet(ReadOnlyCollection<NodeData.ConfigData> config)
        {
            return config.Where(c => c.Type == ID).Any();
        }
    }
}
