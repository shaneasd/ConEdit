using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using ConversationEditor;
using System.Collections.ObjectModel;

namespace PluginPack
{
    public class DoNotRenderConfig : IConfigNodeDefinition
    {
        private static readonly Id<NodeTypeTemp> ID = Id<NodeTypeTemp>.Parse("56e826e6-2db8-42a6-8b29-53e44c2782ec");
        public Id<NodeTypeTemp> Id => ID;

        public string Name => "Don't Render";

        public IEnumerable<IParameter> MakeParameters()
        {
            return Enumerable.Empty<IParameter>();
        }

        public static bool TryGet(IReadOnlyList<NodeData.ConfigData> config)
        {
            return config.Where(c => c.Type == ID).Any();
        }
    }
}
