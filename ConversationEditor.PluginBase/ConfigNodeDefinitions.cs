using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using System.Drawing;
using System.Globalization;
using System.Collections.ObjectModel;

namespace ConversationEditor
{
    public class ShortcutKey : IConfigNodeDefinition
    {
        private static readonly Id<NodeTypeTemp> ID = Id<NodeTypeTemp>.Parse("b3a94816-8236-4ea5-8646-09887e1ebf94");
        public Id<NodeTypeTemp> Id
        {
            get { return ID; }
        }

        public string Name
        {
            get { return "Shortcut Key"; }
        }

        public IEnumerable<Parameter> MakeParameters()
        {
            yield return new StringParameter("Key", Id<Parameter>.Parse("70de2664-9d86-470d-b3b5-2a23e5afae91"), StringParameter.ParameterType);
        }

        public static bool TryGet(ReadOnlyCollection<NodeData.ConfigData> config, out string keys)
        {
            keys = "";
            foreach (var c in config.Where(c => c.Type == ID))
            {
                keys = (c.Parameters.Single() as IStringParameter).Value;
                return true;
            }
            foreach (var c in config.Where(c => c.Type == GenericNodeConfigDefinition.StaticId))
            {
                var data = GenericNodeConfigDefinition.Extract(c);
                if (string.Compare("Shortcut", data.Key, StringComparison.OrdinalIgnoreCase) == 0)
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
        static readonly Id<NodeTypeTemp> ID = Id<NodeTypeTemp>.Parse("5887131d-47aa-49ac-b73f-2e21a176af16");
        public Id<NodeTypeTemp> Id
        {
            get { return ID; }
        }

        public string Name
        {
            get { return "Color"; }
        }

        public IEnumerable<Parameter> MakeParameters()
        {
            yield return new StringParameter("Color", Id<Parameter>.Parse("9baa396d-265d-4b69-8a19-0f4799606a3a"), StringParameter.ParameterType);
        }

        public static Color? TryGet(ReadOnlyCollection<NodeData.ConfigData> config)
        {
            foreach (var c in config.Where(c => c.Type == ID))
            {
                return Color.FromArgb(int.Parse((c.Parameters.Single() as IStringParameter).Value, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture));
            }
            foreach (var c in config.Where(c => c.Type == GenericNodeConfigDefinition.StaticId))
            {
                var data = GenericNodeConfigDefinition.Extract(c);
                if (string.Compare("color", data.Key, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return Color.FromArgb(int.Parse(data.Value, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                }
            }
            return null;
        }
    }

    public static class MaxWidthConfig
    {
        /// <summary>
        /// Determine the maximum width configuration for the input config or null if it is not specified
        /// </summary>
        public static float? TryGet(ReadOnlyCollection<NodeData.ConfigData> config)
        {
            foreach (var c in config.Where(c => c.Type == GenericNodeConfigDefinition.StaticId))
            {
                var data = GenericNodeConfigDefinition.Extract(c);
                if (string.Compare("maxwidth", data.Key, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return float.Parse(data.Value, CultureInfo.InvariantCulture);
                }
            }
            return null;
        }
    }

    public static class RoundedConfig
    {
        public static bool? TryGet(ReadOnlyCollection<NodeData.ConfigData> config)
        {
            foreach (var c in config.Where(c => c.Type == GenericNodeConfigDefinition.StaticId))
            {
                var data = GenericNodeConfigDefinition.Extract(c);
                if (string.Compare("rounded", data.Key, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return bool.Parse(data.Value);
                }
            }
            return null;
        }
    }

    public class GenericNodeConfigDefinition : IConfigNodeDefinition
    {
        public static Id<NodeTypeTemp> StaticId { get { return Id<NodeTypeTemp>.Parse("ba33c8fb-6f3e-4a0f-ba97-b346e02304f8"); } }
        public static Id<Parameter> ConfigKey { get { return Id<Parameter>.Parse("d7e0e8bd-534c-4827-9f57-cbe5446b716d"); } }
        public static Id<Parameter> ConfigValue { get { return Id<Parameter>.Parse("0b82c6b5-3a74-4511-b52b-0d8070839d89"); } }

        public Id<NodeTypeTemp> Id
        {
            get { return StaticId; }
        }

        public string Name
        {
            get { return "Generic Config"; }
        }

        public IEnumerable<Parameter> MakeParameters()
        {
            yield return new StringParameter("Key", ConfigKey, StringParameter.ParameterType);
            yield return new StringParameter("Value", ConfigValue, StringParameter.ParameterType);
        }

        public static NodeData.ConfigData Make(string name, string value)
        {
            var pName = new StringParameter("Key", ConfigKey, StringParameter.ParameterType) { Value = name };
            var pValue = new StringParameter("Value", ConfigValue, StringParameter.ParameterType) { Value = value };
            return new NodeData.ConfigData(StaticId, new[] { pName, pValue });
        }

        public static KeyValuePair<string, string> Extract(NodeData.ConfigData data)
        {
            if (data.Type != StaticId)
                throw new InternalLogicException("Attempted to extract generic config from a node that was not a generic config node");

            var pName = data.Parameters.Single(p => p.Id == ConfigKey) as IStringParameter;
            var pValue = data.Parameters.Single(p => p.Id == ConfigValue) as IStringParameter;
            return new KeyValuePair<string, string>(pName.Value, pValue.Value);
        }
    }

    public class ConfigNodeDefinitions : IConfigNodeDefinitionFactory
    {
        public ConfigNodeDefinitions()
        {
        }

        public IEnumerable<IConfigNodeDefinition> GetConfigNodeDefinitions()
        {
            yield return new GenericNodeConfigDefinition();
            yield return new ShortcutKey();
            yield return new BackgroundColor();
        }
    }
}
