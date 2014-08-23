using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConversationEditor;
using Conversation;
using System.Drawing;

namespace ConversationEditor
{
    public class ShortcutKey : IConfigNodeDefinition
    {
        public static readonly ID<NodeTypeTemp> ID = ID<NodeTypeTemp>.Parse("b3a94816-8236-4ea5-8646-09887e1ebf94");
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
            yield return new StringParameter("Key", ID<Parameter>.Parse("70de2664-9d86-470d-b3b5-2a23e5afae91"), BaseTypeString.PARAMETER_TYPE);
        }

        public static bool TryGet(List<NodeData.ConfigData> config, ref string keys)
        {
            foreach (var c in config.Where(c => c.Type == ID))
            {
                keys = (c.Parameters.Single() as IStringParameter).Value;
                return true;
            }
            foreach (var c in config.Where(c => c.Type == GenericNodeConfigDefinition.ID))
            {
                var data = GenericNodeConfigDefinition.Extract(c);
                if (string.Compare("Shortcut", data.Key, true) == 0)
                {
                    keys = data.Value;
                    return true;
                }
            }
            return false;
        }
    }

    public class BackgroundColor : IConfigNodeDefinition
    {
        static readonly ID<NodeTypeTemp> ID = ID<NodeTypeTemp>.Parse("5887131d-47aa-49ac-b73f-2e21a176af16");
        public ID<NodeTypeTemp> Id
        {
            get { return ID; }
        }

        public string Name
        {
            get { return "Color"; }
        }

        public IEnumerable<Parameter> MakeParameters()
        {
            yield return new StringParameter("Color", ID<Parameter>.Parse("9baa396d-265d-4b69-8a19-0f4799606a3a"), BaseTypeString.PARAMETER_TYPE);
        }

        public static bool TryGet(List<NodeData.ConfigData> config, ref Color color)
        {
            foreach (var c in config.Where(c => c.Type == ID))
            {
                color = Color.FromArgb(int.Parse((c.Parameters.Single() as IStringParameter).Value, System.Globalization.NumberStyles.HexNumber));
                return true;
            }
            foreach (var c in config.Where(c => c.Type == GenericNodeConfigDefinition.ID))
            {
                var data = GenericNodeConfigDefinition.Extract(c);
                if (string.Compare("color", data.Key, true) == 0)
                {
                    color = Color.FromArgb(int.Parse(data.Value, System.Globalization.NumberStyles.HexNumber));
                    return true;
                }
            }
            return false;
        }
    }

    public class MaxWidthConfig
    {
        public static bool TryGet(List<NodeData.ConfigData> config, ref float maxWidth)
        {
            foreach (var c in config.Where(c => c.Type == GenericNodeConfigDefinition.ID))
            {
                var data = GenericNodeConfigDefinition.Extract(c);
                if (string.Compare("maxwidth", data.Key, true) == 0)
                {
                    maxWidth = float.Parse(data.Value);
                    return true;
                }
            }
            return false;
        }
    }

    public class RoundedConfig
    {
        public static bool TryGet(List<NodeData.ConfigData> config, ref bool rounded)
        {
            foreach (var c in config.Where(c => c.Type == GenericNodeConfigDefinition.ID))
            {
                var data = GenericNodeConfigDefinition.Extract(c);
                if (string.Compare("rounded", data.Key, true) == 0)
                {
                    rounded = bool.Parse(data.Value);
                    return true;
                }
            }
            return false;
        }
    }

    public class GenericNodeConfigDefinition : IConfigNodeDefinition
    {
        public static ID<NodeTypeTemp> ID = DomainIDs.CONFIG_GUID;

        public ID<NodeTypeTemp> Id
        {
            get { return ID; }
        }

        public string Name
        {
            get { return "Generic Config"; }
        }

        public IEnumerable<Parameter> MakeParameters()
        {
            yield return new StringParameter("Key", DomainIDs.CONFIG_KEY, BaseTypeString.PARAMETER_TYPE);
            yield return new StringParameter("Value", DomainIDs.CONFIG_VALUE, BaseTypeString.PARAMETER_TYPE);
        }

        public static NodeData.ConfigData Make(string name, string value)
        {
            var pName = new StringParameter("Key", DomainIDs.CONFIG_KEY, BaseTypeString.PARAMETER_TYPE) { Value = name };
            var pValue = new StringParameter("Value", DomainIDs.CONFIG_VALUE, BaseTypeString.PARAMETER_TYPE) { Value = value };
            return new NodeData.ConfigData(ID, new[] { pName, pValue });
        }

        public static KeyValuePair<string, string> Extract(NodeData.ConfigData data)
        {
            if (data.Type != ID)
                throw new Exception("Attempted to extract generic config from a node that was not a generic config node");
            var pName = data.Parameters.Single(p => p.Id == DomainIDs.CONFIG_KEY) as IStringParameter;
            var pValue = data.Parameters.Single(p => p.Id == DomainIDs.CONFIG_VALUE) as IStringParameter;
            return new KeyValuePair<string, string>(pName.Value, pValue.Value);
        }
    }

    public class ConfigNodeDefinitions : IConfigNodeDefinitionFactory
    {
        public ConfigNodeDefinitions()
        {
        }

        IEnumerable<IConfigNodeDefinition> IConfigNodeDefinitionFactory.ConfigNodeDefinitions()
        {
            yield return new GenericNodeConfigDefinition();
            yield return new ShortcutKey();
            yield return new BackgroundColor();
        }
    }
}
