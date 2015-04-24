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
    public class IconConfig : IConfigNodeDefinition
    {
        private static readonly ID<NodeTypeTemp> ID = ID<NodeTypeTemp>.Parse("d2ca0f2a-abed-4000-a41c-7f55a7652585");
        public ID<NodeTypeTemp> Id
        {
            get { return ID; }
        }

        public string Name
        {
            get { return "Icon"; }
        }

        public IEnumerable<Parameter> MakeParameters()
        {
            yield return new StringParameter("Icon", ID<Parameter>.Parse("70de2664-9d86-470d-b3b5-2a23e5afae91"), StringParameter.ParameterType);
        }

        /// <summary>
        /// Retrieve the string for the icon path stored in the input config or null if the config does not contain this data
        /// </summary>
        public static string TryGet(ReadOnlyCollection<NodeData.ConfigData> config)
        {
            foreach (var c in config.Where(c => c.Type == ID))
            {
                return  (c.Parameters.Single() as IStringParameter).Value;
            }
            foreach (var c in config.Where(c => c.Type == GenericNodeConfigDefinition.StaticId))
            {
                var data = GenericNodeConfigDefinition.Extract(c);
                if (string.Compare("Icon", data.Key, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return data.Value;
                }
            }
            return null;
        }
    }
}
