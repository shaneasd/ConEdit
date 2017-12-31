using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.IO;

namespace Conversation
{
    public interface IEnumeration
    {
        IEnumerable<Guid> Options { get; }

        /// <summary>
        /// Get the name associated with the specified unique identifier
        /// </summary>
        /// <param name="value"></param>
        /// <returns>
        /// If Options contais id: The name associated with the input id
        /// Else: null
        /// </returns>
        string GetName(Guid id);
        ParameterType TypeId { get; }
        Either<string, Guid> DefaultValue { get; }
    }

    public static class EnumerationUtil
    {
        /// <summary>
        /// Create an enumeration parameter whose type and data matches this enumeration.
        /// </summary>
        /// <param name="e">The enumeration to act as the base type</param>
        /// <param name="name">The name for the parameter</param>
        /// <param name="id">The id for the parameter</param>
        /// <param name="defaultValue">Default value for the parameter. If null, the default is taken from the enumeration itself</param>
        public static Parameter ParameterEnum(this IEnumeration e, string name, Id<Parameter> id, string defaultValue)
        {
            return new EnumParameter(name, id, e, defaultValue);
        }

        /// <summary>
        /// Create an set of enumeration parameter whose type is a set of values from this enumeration and whose data matches this enumeration.
        /// </summary>
        /// <param name="e">The enumeration to act as the base type</param>
        /// <param name="name">The name for the parameter</param>
        /// <param name="id">The id for the parameter</param>
        /// <param name="defaultValue">Default value for the parameter. If null, the default is taken from the enumeration itself</param>
        public static Parameter ParameterSet(this IEnumeration e, string name, Id<Parameter> id, string defaultValue)
        {
            return new SetParameter(name, id, e, defaultValue);
        }
    }
    
    public abstract class Enumeration : IEnumeration
    {
        protected abstract Dictionary<Guid, string> OptionsMap { get; }

        public string GetName(Guid id)
        {
            if (!OptionsMap.ContainsKey(id))
                return null;
            return OptionsMap[id];
        }

        protected Enumeration(ParameterType typeId, Either<string, Guid> def)
        {
            TypeId = typeId;
            DefaultValue = def;
        }

        public IEnumerable<Guid> Options => OptionsMap.Keys;
        public Either<string, Guid> DefaultValue { get; protected set; }

        public ParameterType TypeId { get; }
    }

    public class ImmutableEnumeration : Enumeration
    {
        protected override Dictionary<Guid, string> OptionsMap { get; }

        public ImmutableEnumeration(IEnumerable<Tuple<Guid, string>> options, ParameterType typeId, Either<string, Guid> def)
            : base(typeId, def)
        {
            OptionsMap = options.ToDictionary(t => t.Item1, t => t.Item2);
        }
    }

    public class MutableEnumeration : Enumeration
    {
        private Dictionary<Guid, string> m_options;

        protected override Dictionary<Guid, string> OptionsMap => m_options;

        public MutableEnumeration(IEnumerable<Tuple<Guid, string>> options, ParameterType typeId, Either<string, Guid> def)
            : base(typeId, def)
        {
            m_options = options.ToDictionary(t => t.Item1, t => t.Item2);
        }

        //public void SetName(Guid guid, string name)
        //{
        //    m_options[guid] = name;
        //}

        public void Add(Guid guid, string name)
        {
            m_options.Add(guid, name);
        }

        public void Remove(Guid guid)
        {
            m_options.Remove(guid);
        }

        public EnumerationData GetData(string name)
        {
            return new EnumerationData(name, this.TypeId,
                                       m_options.Select(o => new EnumerationData.Element(o.Value, o.Key)).ToList());
        }

        public void SetOptions(IEnumerable<EnumerationData.Element> elements)
        {
            m_options.Clear();
            foreach (var option in elements)
            {
                Add(option.Guid, option.Name);
            }
        }
    }
}
