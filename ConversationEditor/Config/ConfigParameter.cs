using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Utilities;

namespace ConversationEditor
{
    public interface IConfigParameter
    {
        void Load(XElement root);
        void Write(XElement root);
        event Action ValueChanged;
    }
    public abstract class ConfigParameter<T> : IConfigParameter
    {
        public abstract void Load(XElement root);
        public abstract void Write(XElement root);
        public event Action ValueChanged;
        protected virtual T InnerValue { get; set; }
        public T Value
        {
            get { return InnerValue; }
            set { InnerValue = value; ValueChanged.Execute(); }
        }
    }
}
