using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using ConversationEditor;
using System.Globalization;
using System.Collections.ObjectModel;

namespace PluginPack
{
    public class NameConfig : IConfigNodeDefinition
    {
        private static readonly Id<NodeTypeTemp> ID = Id<NodeTypeTemp>.Parse("271466d3-aef4-4d35-884c-f079de48c2a4");
        public Id<NodeTypeTemp> Id
        {
            get { return ID; }
        }

        public string Name
        {
            get { return "Name"; }
        }

        public IEnumerable<IParameter> MakeParameters()
        {
            yield return new StringParameter("Name", Id<Parameter>.Parse("d5428350-4ece-4d7d-bc62-6a1b9c76fb9d"));
        }

        /// <summary>
        /// Search the input config for a suitable "name" and return it if found. Otherwise return nulls
        /// </summary>
        public static string TryGet(IReadOnlyCollection<NodeData.ConfigData> config)
        {
            foreach (var c in config.Where(c => c.Type == ID))
            {
                return (c.Parameters.Single() as IStringParameter).Value;
            }
            foreach (var c in config.Where(c => c.Type == GenericNodeConfigDefinition.StaticId))
            {
                var data = GenericNodeConfigDefinition.Extract(c);
                if (string.Compare("Name", data.Key, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return data.Value;
                }
            }
            return null;
        }
    }

}
