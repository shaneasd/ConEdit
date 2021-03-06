﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;
using System.Reflection;

namespace ConversationEditor
{
    using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGui>;

    internal class ErrorCheckerController
    {
        /// <summary>
        /// Represents a type defined in a plugin assembly
        /// </summary>
        public class PluginType
        {
            public readonly string DisplayName;
            public readonly string SerializeName;
            public readonly Type Type;

            public PluginType(Type type)
            {
                Type = type;
                DisplayName = GetName(type);
                SerializeName = DisplayName;
            }

            private static string GetName(Type concrete)
            {
                dynamic instance = concrete.GetConstructor(Type.EmptyTypes).Invoke(new object[0]);
                string name = instance.Name;
                return name;
            }

            public override string ToString()
            {
                return DisplayName;
            }
        }

        public class ErrorCheckerData : PluginType
        {
            public bool Enabled;

            public ErrorCheckerData(Type type, bool enabled)
                : base(type)
            {
                Enabled = enabled;
            }
        }

        private ErrorCheckersConfig m_config;
        private PluginsConfig m_pluginsConfig;

        public ErrorCheckerController(ErrorCheckersConfig config, PluginsConfig pluginsConfig)
        {
            m_config = config;
            m_pluginsConfig = pluginsConfig;
        }

        public void Configure(IColorScheme scheme)
        {
            using (ErrorCheckerSelectorDialog dialog = new ErrorCheckerSelectorDialog())
            {
                dialog.Scheme = scheme;
                dialog.Init(m_config, m_pluginsConfig);
                dialog.ShowDialog();
            }
        }

        internal IEnumerable<ConversationError<ConversationNode>> CheckForErrors(IEnumerable<ConversationNode> nodes, IErrorCheckerUtilities<ConversationNode> utils)
        {
            m_config.RefreshConfig(m_pluginsConfig);
            foreach (var assembly in m_config.Assemblies)
                foreach (var checker in assembly.GetEnabledErrorCheckers())
                    foreach (var error in checker.Check(nodes, utils))
                        yield return error;
        }
    }
}
