using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using Utilities;
using System.Diagnostics;

namespace Conversation
{
    public interface IParameter
    {
        /// <summary>
        /// Guid of the underlying type
        /// </summary>
        ParameterType TypeId { get; }

        /// <summary>
        /// Name of the parameter displayed in the editor
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Unique identifier identifying this parameter
        /// </summary>
        Id<Parameter> Id { get; }

        /// <summary>
        /// True iff the parameter has an invalid value (e.g. null, out of range or not within a set of acceptable values)
        /// This could occur if an old file is loaded with a new domain or vice versa resulting in out of data data or definitions or missing parameter data.
        /// </summary>
        bool Corrupted { get; }

        /// <summary>
        /// String representation of the parameter suitable for serialization to a file. This value is not guaranteed to be able to be parsed to a valid value as
        /// it may simply be the string that was read originally for this parameter.
        /// </summary>
        string ValueAsString();

        /// <summary>
        /// String representation of the parameter suitable for display in the UI
        /// </summary>
        /// <param name="localize">Lookup of localized text Ids to strings</param>
        string DisplayValue(Func<Id<LocalizedText>, string> localize);

        /// <summary>
        /// Attempt to update current value by parsing the input string. Value and Corrupted may both change as a result of this.
        /// </summary>
        void TryDeserialiseValue(string value);
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

    public interface IConnectorParameter : IParameter<Id<NodeTemp>>
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

    public interface ILocalizedStringParameter : IParameter<Id<LocalizedText>>
    {
    }

    public interface ISetParameter : IParameter<ReadonlySet<Guid>>
    {
        IEnumerable<Guid> Options { get; }

        /// <summary>
        /// Return the name associated with the specified value. null if the value isn't assoicated with this enumeration.
        /// </summary>
        string GetName(Guid value);
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
        void MergeInto(DynamicEnumParameter.Source newSource);
        bool Local { get; }
    }

    public interface IAudioParameter : IParameter<Audio>
    {
    }

    public abstract class Parameter : IParameter
    {
        public abstract bool Corrupted { get; }

        public string Name { get; }

        public Id<Parameter> Id { get; }

        public ParameterType TypeId { get; }

        protected Parameter(string name, Id<Parameter> id, ParameterType typeid, string stringValue)
        {
            Name = name;
            Id = id;
            TypeId = typeid;
            m_lastValueString = stringValue;
        }

        public abstract string DisplayValue(Func<Id<LocalizedText>, string> localize);
        protected abstract string InnerValueAsString();
        public string ValueAsString()
        {
            return Corrupted ? m_lastValueString : InnerValueAsString();
        }

        /// <summary>
        /// Attempt to parse the input and store the corresponding value
        /// updating value and corrupted based on success
        /// </summary>
        protected abstract void DeserialiseValue(string value);

        private string m_lastValueString = null;
        public void TryDeserialiseValue(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            m_lastValueString = value;
            DeserialiseValue(value);
        }
    }

    public abstract class Parameter<T> : Parameter
    {
        public T Value
        {
            get;
            private set;
        }

        public sealed override bool Corrupted { get { return m_corrupted; } }
        private bool m_corrupted;

        /// <summary>
        /// Change Value to the input value.
        /// </summary>
        private void SetValue(T value)
        {
            Value = value;
            OnSetValue(value);
        }

        protected virtual void OnSetValue(T value)
        {
        }

        protected abstract bool ValueValid(T value);

        public SimpleUndoPair? SetValueAction(T value)
        {
            if (!ValueValid(value))
                throw new ArgumentException("Invalid value");

            var wasCorrupted = Corrupted;
            var oldValue = Value;

            if (value.Equals(oldValue) && !wasCorrupted)
                return null;

            return new SimpleUndoPair
            {
                Redo = () =>
                {
                    if (!this.ValueValid(value))
                        throw new ArgumentException("Invalid value");
                    SetValue(value);
                    m_corrupted = false;
                },
                Undo = () =>
                {
                    SetValue(oldValue);
                    m_corrupted = wasCorrupted;
                }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="id">Unique identifier for this parameter</param>
        /// <param name="typeId">Unique identifier for the type of this parameter</param>
        /// <param name="defaultValue">Default value this parameter should take when no value is given (e.g. in an incomplete data file)</param>
        /// <param name="value">Initial value of the parameter and whether that value is corrupted</param>
        protected Parameter(string name, Id<Parameter> id, ParameterType typeId, string defaultValue, Tuple<T, bool> value)
            : base(name, id, typeId, defaultValue)
        {
            Value = value.Item1;
            m_corrupted = value.Item2;
        }

        public override string ToString()
        {
            throw new InternalLogicException("Dont call this");
        }

        protected override void DeserialiseValue(string value)
        {
            var a = DeserializeValueInner(value);
            SetValue(a.Item1);
            m_corrupted = a.Item2;
        }

        /// <summary>
        /// Attempt to deserialize a value from the input string.
        /// </summary>
        /// <param name="value">The string to read the value from</param>
        /// <returns>The value to store and whether that value is corrupted.</returns>
        protected abstract Tuple<T, bool> DeserializeValueInner(string value);
    }
}
