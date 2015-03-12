using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using ConversationEditor;

namespace PluginPack
{
    public class NameConfig : IConfigNodeDefinition
    {
        public static readonly ID<NodeTypeTemp> ID = ID<NodeTypeTemp>.Parse("271466d3-aef4-4d35-884c-f079de48c2a4");
        public ID<NodeTypeTemp> Id
        {
            get { return ID; }
        }

        public string Name
        {
            get { return "Name"; }
        }

        public IEnumerable<Parameter> MakeParameters()
        {
            yield return new StringParameter("Name", ID<Parameter>.Parse("d5428350-4ece-4d7d-bc62-6a1b9c76fb9d"), BaseTypeString.PARAMETER_TYPE);
        }

        public static bool TryGet(List<NodeData.ConfigData> config, ref string name)
        {
            foreach (var c in config.Where(c => c.Type == ID))
            {
                name = (c.Parameters.Single() as IStringParameter).Value;
                return true;
            }
            foreach (var c in config.Where(c => c.Type == GenericNodeConfigDefinition.ID))
            {
                var data = GenericNodeConfigDefinition.Extract(c);
                if (string.Compare("Name", data.Key, true) == 0)
                {
                    name = data.Value;
                    return true;
                }
            }
            return false;
        }
    }

}
