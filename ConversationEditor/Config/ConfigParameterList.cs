﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Utilities;
using Conversation;

namespace ConversationEditor
{
    internal class ConfigParameterList<TValue> : Disposable, IConfigParameter, IDisposable
    {
        private readonly string m_name;
        private readonly Func<ConfigParameter<TValue>> m_nodeFactory;
        private readonly CallbackList<TValue> m_data = new CallbackList<TValue>();
        SuppressibleAction m_suppressibleValueChanged;

        public ConfigParameterList(string name, Func<ConfigParameter<TValue>> nodeFactory)
        {
            m_name = name;
            m_suppressibleValueChanged = new SuppressibleAction(() => ValueChanged.Execute());
            m_data.Modified += () => { m_suppressibleValueChanged.TryExecute(); };
            m_nodeFactory = nodeFactory;
        }

        public void Load(XElement root)
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));
            if (m_suppressibleValueChanged.Suppressed)
                throw new InternalLogicException("Can't load config while there are unsaved changes to config");

            using (SuppressCallback()) //The following block will modify the data but during a load that shouldn't trigger ValueChanged
            {
                var a = root.Element(m_name);
                m_data.Clear();
                if (a != null)
                {
                    foreach (var node in a.Elements("Element"))
                    {
                        ConfigParameter<TValue> t = m_nodeFactory();
                        t.Load(node);
                        m_data.Add(t.Value);
                    }
                }
                m_suppressibleValueChanged.Dispose(); //Pretend we haven't changed anything, by destroying the old callback and making a new one
            }

            m_suppressibleValueChanged = new SuppressibleAction(() => ValueChanged.Execute());
        }

        public void Write(XElement root)
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));
            var listRoot = new XElement(m_name);
            foreach (TValue t in m_data)
            {
                var element = new XElement("Element");
                ConfigParameter<TValue> param = m_nodeFactory();
                param.Value = t;
                param.Write(element);
                listRoot.Add(element);
            }
            root.Add(listRoot);
        }

        public event Action ValueChanged;

        public IList<TValue> Value => m_data;

        public IDisposable SuppressCallback()
        {
            return m_suppressibleValueChanged.SuppressCallback();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_suppressibleValueChanged.Dispose();
            }
        }
    }
}
