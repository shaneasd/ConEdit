using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using Utilities;

namespace Conversation
{
    public interface IParameter
    {
        /// <summary>
        /// Guid of the underlying type
        /// </summary>
        ID<ParameterType> TypeId { get; }
        /// <summary>
        /// Name of the parameter displayed in the editor
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Unique identifier identifying this parameter
        /// </summary>
        ID<Parameter> Id { get; }

        bool Corrupted { get; }
    }

    public interface IParameter<T> : IParameter
    {
        T Value { get; }
        /// <summary>
        /// Return the action pair required to 
        /// Redo: set Value to 'value'
        /// Undo: return Value to its current value
        /// Or null if Value = value
        /// </summary>
        SimpleUndoPair? SetValueAction(T value);
    }

    public interface IIntegerParameter : IParameter<int>
    {
        int Max { get; }
        int Min { get; }
    }

    public interface IBooleanParameter : IParameter<bool>
    {
    }

    public interface IConnectorParameter : IParameter<ID<NodeTemp>>
    {
    }

    public interface IDecimalParameter : IParameter<decimal>
    {
        decimal Max { get; }
        decimal Min { get; }
    }

    public interface IStringParameter : IParameter<string>
    {
    }

    public interface ILocalizedStringParameter : IParameter<ID<LocalizedText>>
    {
    }

    public interface IEnumParameter : IParameter<Guid>
    {
        IEnumerable<Guid> Options { get; }

        /// <summary>
        /// Return the name associated with the specified value. null if the value isn't assoicated with this enumeration.
        /// </summary>
        string GetName(Guid value);

        Guid EditorSelected { get; set; }
    }

    public interface IDynamicEnumParameter : IParameter<string>
    {
        IEnumerable<string> Options { get; }
    }

    public interface IAudioParameter : IParameter<Audio>
    {
    }

    public abstract class Parameter : IParameter
    {
        private readonly string m_name;
        public string Name { get { return m_name; } }

        private readonly ID<Parameter> m_id;
        public ID<Parameter> Id { get { return m_id; } }

        private readonly ID<ParameterType> m_typeId;
        public ID<ParameterType> TypeId { get { return m_typeId; } }
        public Parameter(string name, ID<Parameter> id, ID<ParameterType> typeid, string value)
        {
            m_name = name;
            m_id = id;
            m_typeId = typeid;
            TryDeserialiseValue(value);
            if (value == null)
                Corrupted = true; //Corrupt if null so editors don't get confused
        }

        public abstract string DisplayValue(Func<ID<LocalizedText>, string> localize);
        protected abstract string InnerValueAsString();
        public string ValueAsString()
        {
            return Corrupted ? m_lastValueString : InnerValueAsString();
        }

        /// <summary>
        /// Attempt to parse the input and store the corresponding value
        /// </summary>
        /// <returns>true iff the input was successfully parsed into the appropriate type</returns>
        protected abstract bool DeserialiseValue(string value);
        public bool Corrupted { get; protected set; }

        private string m_lastValueString = null;
        public bool TryDeserialiseValue(string value)
        {
            m_lastValueString = value;
            Corrupted = !DeserialiseValue(value);
            return !Corrupted;
        }

        protected abstract void DecorruptFromNull();

        /// <summary>
        /// Only called on load so doesn't need to be undoable
        /// </summary>
        public void TryDecorrupt()
        {
            Corrupted = !DeserialiseValue(m_lastValueString);
            if (Corrupted && m_lastValueString == null)
                DecorruptFromNull();
        }
    }

    public abstract class Parameter<T> : Parameter
    {
        protected T m_value;

        public virtual T Value
        {
            get
            {
                return m_value;
            }
            set
            {
                m_value = value;
                Corrupted = false;
            }
        }

        public SimpleUndoPair? SetValueAction(T value)
        {
            var wasCorrupted = Corrupted;
            var oldValue = m_value;

            if (value.Equals(oldValue) && !wasCorrupted)
                return null;

            return new SimpleUndoPair
            {
                Redo = () => { Value = value; Corrupted = false; },
                Undo = () => { Value = oldValue; Corrupted = wasCorrupted; }
            };
        }

        string m_defaultValue = null;

        public Parameter(string name, ID<Parameter> id, ID<ParameterType> typeId, string defaultValue)
            : base(name, id, typeId, defaultValue)
        {
            m_defaultValue = defaultValue;
        }

        public override string ToString()
        {
            throw new Exception("Dont call this");
        }

        protected override void DecorruptFromNull()
        {
            TryDeserialiseValue(m_defaultValue);
        }
    }
}
