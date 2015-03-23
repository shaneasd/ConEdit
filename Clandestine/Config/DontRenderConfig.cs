using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using ConversationEditor;

namespace Clandestine
{
    public class DontRenderConfig : IConfigNodeDefinition
    {
        public static readonly ID<NodeTypeTemp> ID = ID<NodeTypeTemp>.Parse("56e826e6-2db8-42a6-8b29-53e44c2782ec");
        public ID<NodeTypeTemp> Id
        {
            get { return ID; }
        }

        public string Name
        {
            get { return "Don't Render"; }
        }

        public IEnumerable<Parameter> MakeParameters()
        {
            return Enumerable.Empty<Parameter>();
        }

        public static bool TryGet(List<NodeData.ConfigData> config)
        {
            foreach (var c in config.Where(c => c.Type == ID))
            {
                return true;
            }
            return false;
        }
    }
}
