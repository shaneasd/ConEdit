using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Utilities;
using Conversation;

namespace ConversationEditor
{
    internal class TypeMapConfig<TKey, TData> : IConfigParameter where TData : TypeChoice
    {
        protected Dictionary<string, TData> m_data = new Dictionary<string, TData>();
        protected readonly string m_nodeName;
        protected readonly Func<TKey, string> m_serializeKey;
        protected readonly Func<string, string, TData> m_makeChoice;
        protected readonly Func<TKey, TData> m_defaults;

        public TypeMapConfig(string nodeName, Func<TKey, string> serializeKey, Func<string, string, TData> makeChoice, Func<TKey, TData> defaults)
        {
            m_nodeName = nodeName;
            m_serializeKey = serializeKey;
            m_makeChoice = makeChoice;
            m_defaults = defaults;
        }

        public void Load(XElement root)
        {
            m_data = new Dictionary<string, TData>();
            var node = root.Element(m_nodeName);
            if (node != null)
            {
                foreach (var n in node.Elements("Editor"))
                {
                    var parameter = n.Attribute("parameter");
                    var assembly = n.Attribute("assembly");
                    var editor = n.Attribute("editor");

                    if (parameter != null && assembly != null && editor != null)
                        m_data[parameter.Value] = m_makeChoice(assembly.Value, editor.Value);
                }
            }
        }

        public void Write(XElement root)
        {
            var node = new XElement(m_nodeName);
            root.Add(node);
            foreach (var kvp in m_data)
            {
                node.Add(new XElement("Editor", new XAttribute("parameter", kvp.Key), new XAttribute("assembly", kvp.Value.m_assembly), new XAttribute("editor", kvp.Value.m_typeName)));
            }
        }

        public event Action ValueChanged;

        public TData this[TKey key]
        {
            get
            {
                if (m_data.ContainsKey(m_serializeKey(key)))
                    return m_data[m_serializeKey(key)];
                else
                    return m_defaults(key);
            }
            set
            {
                if (value == null)
                    m_data.Remove(m_serializeKey(key));
                else
                    m_data[m_serializeKey(key)] = value;
                ValueChanged.Execute();
            }
        }

        public bool ContainsKey(TKey key)
        {
            return m_data.ContainsKey(m_serializeKey(key));
        }
    }

    public class MapConfig<TKey, TData> : IConfigParameter
    {
        protected Dictionary<TKey, TData> m_data = new Dictionary<TKey, TData>();
        protected readonly string m_nodeName;
        protected readonly Func<KeyValuePair<TKey, TData>, KeyValuePair<string, string>> m_serialize;
        protected readonly Func<KeyValuePair<string, string>, KeyValuePair<TKey, TData>> m_deserialize;
        protected readonly Func<TKey, TData> m_defaults;

        public MapConfig(string nodeName, Func<KeyValuePair<TKey, TData>, KeyValuePair<string, string>> serialize, Func<KeyValuePair<string, string>, KeyValuePair<TKey, TData>> deserialize, Func<TKey, TData> defaults)
        {
            m_nodeName = nodeName;
            m_serialize = serialize;
            m_deserialize = deserialize;
            m_defaults = defaults;
        }

        public void Load(XElement root)
        {
            m_data = new Dictionary<TKey, TData>();
            var node = root.Element(m_nodeName);
            if (node != null)
            {
                foreach (var n in node.Elements("Editor"))
                {
                    var parameter = n.Attribute("parameter");
                    var editor = n.Attribute("editor");

                    var data = m_deserialize(new KeyValuePair<string, string>(parameter.Value, editor.Value));
                    m_data[data.Key] = data.Value;
                }
            }
        }

        public void Write(XElement root)
        {
            var node = new XElement(m_nodeName);
            root.Add(node);
            foreach (var kvp in m_data)
            {
                var data = m_serialize(kvp);
                node.Add(new XElement("Editor", new XAttribute("parameter", data.Key), new XAttribute("editor", data.Value)));
            }
        }

        public event Action ValueChanged;

        public TData this[TKey key]
        {
            get
            {
                if (m_data.ContainsKey(key))
                    return m_data[key];
                else
                    return m_defaults(key);
            }
            set
            {
                if (value == null)
                    m_data.Remove(key);
                else
                    m_data[key] = value;
                ValueChanged.Execute();
            }
        }

        public bool ContainsKey(TKey key)
        {
            return m_data.ContainsKey(key);
        }
    }
}
