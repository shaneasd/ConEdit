using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using ConversationEditor;

namespace PluginPack
{
    public class ConfigNodeDefinitions : IConfigNodeDefinitionFactory
    {
        public ConfigNodeDefinitions()
        {
        }

        IEnumerable<IConfigNodeDefinition> IConfigNodeDefinitionFactory.ConfigNodeDefinitions()
        {
            yield return new IconConfig();
        }
    }


    public class IconConfig : IConfigNodeDefinition
    {
        public static readonly ID<NodeTypeTemp> ID = ID<NodeTypeTemp>.Parse("d2ca0f2a-abed-4000-a41c-7f55a7652585");
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
            yield return new StringParameter("Icon", ID<Parameter>.Parse("70de2664-9d86-470d-b3b5-2a23e5afae91"), BaseTypeString.PARAMETER_TYPE);
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

    public class NameConfig : IConfigNodeDefinition
    {
        public static readonly ID<NodeTypeTemp> ID = ID<NodeTypeTemp>.Parse("271466d3-aef4-4d35-884c-f079de48c2a4");
        public ID<NodeTypeTemp> Id
        {
            get { return ID; }
        }

        public string Name
        {
            get { return "Shortcut Key"; }
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
