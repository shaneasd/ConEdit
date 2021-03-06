﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.IO;

using TDocument = System.Object;
using System.Collections.Concurrent;

namespace Conversation
{
    public delegate IParameter ParameterGenerator(string name, Id<Parameter> id, string defaultValue, TDocument document);

    /// <summary>
    /// A TypeSet with types which cannot be modified by this interface
    /// </summary>
    public interface ITypeSetFixedTypes
    {
        IEnumerable<ParameterType> AllTypes { get; }
        string GetTypeName(ParameterType guid);
        DynamicEnumParameter.Source GetDynamicEnumSource(ParameterType type);
        DynamicEnumParameter.Source GetLocalDynamicEnumSource(ParameterType type, TDocument document);
        bool IsInteger(ParameterType type);
        bool IsDecimal(ParameterType type);
        bool IsEnum(ParameterType type);
        bool IsDynamicEnum(ParameterType type);
        bool IsLocalDynamicEnum(ParameterType type);
        IParameter Make(ParameterType typeId, string name, Id<Parameter> id, string defaultValue, TDocument document);
    }

    /// <summary>
    /// A TypeSet whose types do not change once created
    /// This class is threadsafe
    /// </summary>
    public class ConstantTypeSet : ITypeSetFixedTypes
    {
        private class TypeData
        {
            public ParameterGenerator Generator;
            public string Name;
            public TypeData(ParameterGenerator generator, string name)
            {
                Generator = generator;
                Name = name;
            }
        }

        private Dictionary<ParameterType, TypeData> m_types = new Dictionary<ParameterType, TypeData>();
        private Dictionary<ParameterType, DynamicEnumerationData> m_dynamicEnums = new Dictionary<ParameterType, DynamicEnumerationData>();
        private Dictionary<ParameterType, LocalDynamicEnumerationData> m_localDynamicEnums = new Dictionary<ParameterType, LocalDynamicEnumerationData>();
        private Dictionary<ParameterType, Tuple<string, ImmutableEnumeration>> m_enums = new Dictionary<ParameterType, Tuple<string, ImmutableEnumeration>>();
        private Dictionary<ParameterType, IntegerData> m_integers = new Dictionary<ParameterType, IntegerData>();
        private Dictionary<ParameterType, DecimalData> m_decimals = new Dictionary<ParameterType, DecimalData>();
        private Dictionary<ParameterType, LocalizedStringData> m_localizedStrings = new Dictionary<ParameterType, LocalizedStringData>();

        //The contents of this collection can change as the enum source is a transient property of the types
        ConcurrentDictionary<Tuple<ParameterType, TDocument>, DynamicEnumParameter.Source> m_localDynamicEnumSources = new ConcurrentDictionary<Tuple<ParameterType, TDocument>, DynamicEnumParameter.Source>();

        public ConstantTypeSet(IEnumerable<DynamicEnumerationData> dynamicEnumerations, IEnumerable<LocalDynamicEnumerationData> localDynamicEnumerations, IEnumerable<EnumerationData> enumerations, IEnumerable<DecimalData> decimals, IEnumerable<IntegerData> integers, IEnumerable<LocalizedStringData> localizedStrings, IEnumerable<Tuple<ParameterType, string, ParameterGenerator>> others)
        {
            foreach (var other in others)
            {
                m_types.Add(other.Item1, new TypeData(other.Item3, other.Item2));
            }

            //Types must be generated before Nodes and can be generated before NodeTypes
            foreach (var td in dynamicEnumerations)
            {
                var typeData = td;
                m_dynamicEnums.Add(typeData.TypeId, typeData);
                m_types.Add(typeData.TypeId, new TypeData((a, b, c, document) => typeData.Make(a, b, c, GetDynamicEnumSource(typeData.TypeId)), typeData.Name));
            }

            foreach (var td in localDynamicEnumerations)
            {
                var typeData = td;
                m_localDynamicEnums.Add(typeData.TypeId, typeData);
                m_types.Add(typeData.TypeId, new TypeData((name, id, defaultValue, document) => typeData.Make(name, id, defaultValue, GetLocalDynamicEnumSource(typeData.TypeId, document)), typeData.Name));
            }

            foreach (var td in enumerations)
            {
                var typeData = td;
                var enumType = typeData.TypeId;
                var setType = ParameterType.ValueSetType.Of(enumType);

                var elements = typeData.Elements.Select(e => Tuple.Create(e.Guid, e.Name));
                ImmutableEnumeration enumeration = new ImmutableEnumeration(elements, enumType, "");
                m_enums.Add(enumType, Tuple.Create(typeData.Name, enumeration));
                m_types.Add(enumType, new TypeData((a, b, c, d) => m_enums[enumType].Item2.ParameterEnum(a, b, c), typeData.Name));
                m_types.Add(setType, new TypeData((a, b, c, d) => m_enums[enumType].Item2.ParameterSet(a, b, c), "Set of " + typeData.Name));
            }

            foreach (var td in decimals)
            {
                var typeData = td;
                m_decimals.Add(typeData.TypeId, typeData);
                m_types.Add(typeData.TypeId, new TypeData((name, id, defaultValue, document) => new DecimalParameter(name, id, typeData.TypeId, m_decimals[typeData.TypeId].Definition(), defaultValue), typeData.Name));
            }

            foreach (var td in integers)
            {
                var typeData = td;
                m_integers.Add(typeData.TypeId, typeData);
                m_types.Add(typeData.TypeId, new TypeData((name, id, defaultValue, document) => new IntegerParameter(name, id, typeData.TypeId, m_integers[typeData.TypeId].Definition(), defaultValue), typeData.Name));
            }

            foreach (var td in localizedStrings)
            {
                var typeData = td;
                m_localizedStrings.Add(typeData.TypeId, typeData);
                m_types.Add(typeData.TypeId, new TypeData((name, id, defaultValue, document) => new LocalizedStringParameter(name, id, typeData.TypeId), typeData.Name));
            }
        }

        public IEnumerable<ParameterType> AllTypes => m_types.Keys;

        public DynamicEnumParameter.Source GetDynamicEnumSource(ParameterType type)
        {
            var key = Tuple.Create(type, (object)null);
            return m_localDynamicEnumSources.GetOrAdd(key, k => new DynamicEnumParameter.Source());
        }

        public DynamicEnumParameter.Source GetLocalDynamicEnumSource(ParameterType type, TDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            var key = Tuple.Create(type, document);
            return m_localDynamicEnumSources.GetOrAdd(key, k => new DynamicEnumParameter.Source());
        }

        public string GetTypeName(ParameterType guid)
        {
            return m_types[guid].Name;
        }

        public bool IsDecimal(ParameterType type)
        {
            return m_decimals.ContainsKey(type);
        }

        public bool IsLocalizedString(ParameterType type)
        {
            return m_localizedStrings.ContainsKey(type);
        }

        public bool IsDynamicEnum(ParameterType type)
        {
            return m_dynamicEnums.ContainsKey(type);
        }

        public bool IsEnum(ParameterType type)
        {
            return m_enums.ContainsKey(type);
        }

        public bool IsInteger(ParameterType type)
        {
            return m_integers.ContainsKey(type);
        }

        public bool IsLocalDynamicEnum(ParameterType type)
        {
            return m_localDynamicEnums.ContainsKey(type);
        }

        public IParameter Make(ParameterType typeId, string name, Id<Parameter> id, string defaultValue, TDocument document)
        {
            if (!m_types.ContainsKey(typeId))
                return new UnknownParameter(id, defaultValue);
            else
                return m_types[typeId].Generator(name, id, defaultValue, document);
        }
    }

    /// <summary>
    /// Keeps track of all types within a domain.
    /// Can be queried to determine the base type of a typeID
    /// Can generate a Parameter for a given ParameterType
    /// </summary>
    public class TypeSet : ITypeSetFixedTypes
    {
        private class TypeProvider<T>
        {
            private Dictionary<ParameterType, T> m_data = new Dictionary<ParameterType, T>();
            private Dictionary<ParameterType, bool> m_hidden { get; } //Shared with the other TypeProviders
            public IEnumerable<T> Visible => m_data.Where(kvp => !m_hidden[kvp.Key]).Select(kvp => kvp.Value);

            public TypeProvider(Dictionary<ParameterType, bool> hidden)
            {
                m_hidden = hidden;
            }

            public void Add(ParameterType typeId, T value)
            {
                m_data.Add(typeId, value);
            }

            public void Remove(ParameterType typeId)
            {
                m_data.Remove(typeId);
            }

            public bool Contains(ParameterType typeId)
            {
                return m_data.ContainsKey(typeId);
            }

            public T this[ParameterType typeId]
            {
                get { return m_data[typeId]; }
                set { m_data[typeId] = value; }
            }
        }

        private class TypeData
        {
            public ParameterGenerator Generator;
            public string Name;
            public TypeData(ParameterGenerator generator, string name)
            {
                Generator = generator;
                Name = name;
            }
        }

        private Dictionary<ParameterType, bool> m_hidden { get; } = new Dictionary<ParameterType, bool>();
        private Dictionary<ParameterType, TypeData> m_types { get; } = new Dictionary<ParameterType, TypeData>();
        private Dictionary<ParameterType, Tuple<string, MutableEnumeration>> m_enums { get; } = new Dictionary<ParameterType, Tuple<string, MutableEnumeration>>();
        private TypeProvider<DynamicEnumerationData> m_dynamicEnums { get; }
        private TypeProvider<LocalDynamicEnumerationData> m_localDynamicEnums { get; }
        private TypeProvider<LocalizedStringData> m_localizedStrings { get; }
        private TypeProvider<IntegerData> m_integers { get; }
        private TypeProvider<DecimalData> m_decimals { get; }

        public TypeSet()
        {
            m_dynamicEnums = new TypeProvider<DynamicEnumerationData>(m_hidden);
            m_localDynamicEnums = new TypeProvider<LocalDynamicEnumerationData>(m_hidden);
            m_localizedStrings = new TypeProvider<LocalizedStringData>(m_hidden);
            m_integers = new TypeProvider<IntegerData>(m_hidden);
            m_decimals = new TypeProvider<DecimalData>(m_hidden);
        }

        public IEnumerable<ParameterType> AllTypes => m_types.Keys;

        /// <summary>
        /// Triggered whenever a new type is added or an existing type is modified or removed
        /// </summary>
        public event Action<ParameterType> Modified;

        public IEnumerable<EnumerationData> VisibleEnums => m_enums.Where(kvp => !m_hidden[kvp.Key]).Select(kvp => GetEnumData(kvp.Key));
        public IEnumerable<DynamicEnumerationData> VisibleDynamicEnums => m_dynamicEnums.Visible;
        public IEnumerable<LocalDynamicEnumerationData> VisibleLocalDynamicEnums => m_localDynamicEnums.Visible;
        public IEnumerable<IntegerData> VisibleIntegers => m_integers.Visible;
        public IEnumerable<DecimalData> VisibleDecimals => m_decimals.Visible;
        public IEnumerable<LocalizedStringData> VisibleLocalizedStrings => m_localizedStrings.Visible;

        public Tuple<int?, int?> GetIntegerRange(ParameterType type)
        {
            if (m_integers.Contains(type))
            {
                var data = m_integers[type];
                return Tuple.Create(data.Min, data.Max);
            }
            else
            {
                return null;
            }
        }

        public Tuple<decimal?, decimal?> GetDecimalRange(ParameterType type)
        {
            if (m_decimals.Contains(type))
            {
                var data = m_decimals[type];
                return Tuple.Create(data.Min, data.Max);
            }
            else
            {
                return null;
            }
        }

        public IParameter Make(ParameterType typeId, string name, Id<Parameter> id, string defaultValue, TDocument document)
        {
            return m_types[typeId].Generator(name, id, defaultValue, document);
        }

        public void AddInteger(IntegerData typeData)
        {
            m_hidden[typeData.TypeId] = false;
            m_integers.Add(typeData.TypeId, typeData);
            m_types.Add(typeData.TypeId, new TypeData((name, id, defaultValue, document) => new IntegerParameter(name, id, typeData.TypeId, m_integers[typeData.TypeId].Definition(), defaultValue), typeData.Name));
            Modified.Execute(typeData.TypeId);
        }

        public void ModifyInteger(IntegerData typeData)
        {
            m_types[typeData.TypeId] = new TypeData((name, id, defaultValue, document) => new IntegerParameter(name, id, typeData.TypeId, typeData.Definition(), defaultValue), typeData.Name);
            m_integers[typeData.TypeId] = typeData;
            Modified.Execute(typeData.TypeId);
        }

        public void AddDecimal(DecimalData typeData)
        {
            m_hidden[typeData.TypeId] = false;
            m_decimals.Add(typeData.TypeId, typeData);
            m_types.Add(typeData.TypeId, new TypeData((name, id, defaultValue, document) => new DecimalParameter(name, id, typeData.TypeId, m_decimals[typeData.TypeId].Definition(), defaultValue), typeData.Name));
            Modified.Execute(typeData.TypeId);
        }

        public void ModifyDecimal(DecimalData typeData)
        {
            m_types[typeData.TypeId] = new TypeData((name, id, defaultValue, document) => new DecimalParameter(name, id, typeData.TypeId, m_decimals[typeData.TypeId].Definition(), defaultValue), typeData.Name);
            m_decimals[typeData.TypeId] = typeData;
            Modified.Execute(typeData.TypeId);
        }

        public void AddLocalizedString(LocalizedStringData typeData)
        {
            m_hidden[typeData.TypeId] = false;
            m_localizedStrings.Add(typeData.TypeId, typeData);
            m_types.Add(typeData.TypeId, new TypeData((name, id, defaultValue, document) => new LocalizedStringParameter(name, id, typeData.TypeId), typeData.Name));
            Modified.Execute(typeData.TypeId);
        }

        public void ModifyLocalizedString(LocalizedStringData typeData)
        {
            m_types[typeData.TypeId] = new TypeData((name, id, defaultValue, document) => new LocalizedStringParameter(name, id, typeData.TypeId), typeData.Name);
            m_localizedStrings[typeData.TypeId] = typeData;
            Modified.Execute(typeData.TypeId);
        }

        public void AddEnum(EnumerationData typeData, bool hidden)
        {
            var enumType = typeData.TypeId;
            var setType = ParameterType.ValueSetType.Of(enumType);

            m_hidden[enumType] = hidden;
            var elements = typeData.Elements.Select(e => Tuple.Create(e.Guid, e.Name));
            MutableEnumeration enumeration = new MutableEnumeration(elements, enumType, "");
            m_enums.Add(enumType, Tuple.Create(typeData.Name, enumeration));
            m_types.Add(enumType, new TypeData((a, b, c, d) => m_enums[enumType].Item2.ParameterEnum(a, b, c), typeData.Name));
            m_types.Add(setType, new TypeData((a, b, c, d) => m_enums[enumType].Item2.ParameterSet(a, b, c), "Set of " + typeData.Name));
            Modified.Execute(enumType);
        }

        public void ModifyEnum(EnumerationData typeData)
        {
            if (m_enums.ContainsKey(typeData.TypeId)) //If we're removing an entire domain file, an enum declaration can be removed before its values. In this circumstance, when the values are removed, this method will be called but the enum wont exist.
            {
                m_types[typeData.TypeId].Name = typeData.Name;
                m_types[ParameterType.ValueSetType.Of(typeData.TypeId)].Name = "Set of " + typeData.Name;
                m_enums[typeData.TypeId] = Tuple.Create(typeData.Name, m_enums[typeData.TypeId].Item2);
                MutableEnumeration e = m_enums[typeData.TypeId].Item2;
                e.SetOptions(typeData.Elements);
                Modified.Execute(typeData.TypeId);
            }
        }

        public EnumerationData GetEnumData(ParameterType id)
        {
            return m_enums[id].Item2.GetData(m_enums[id].Item1);
        }

        public void AddDynamicEnum(DynamicEnumerationData typeData)
        {
            m_hidden[typeData.TypeId] = false;
            m_dynamicEnums.Add(typeData.TypeId, typeData);
            m_types.Add(typeData.TypeId, new TypeData((a, b, c, document) => typeData.Make(a, b, c, GetDynamicEnumSource(typeData.TypeId)), typeData.Name));
            Modified.Execute(typeData.TypeId);
        }

        Dictionary<Tuple<ParameterType, TDocument>, DynamicEnumParameter.Source> m_localDynamicEnumSources = new Dictionary<Tuple<ParameterType, TDocument>, DynamicEnumParameter.Source>();

        public DynamicEnumParameter.Source GetDynamicEnumSource(ParameterType type)
        {
            var key = Tuple.Create(type, (object)null);
            return m_localDynamicEnumSources.GetOrAdd(key, k => new DynamicEnumParameter.Source());
        }

        public DynamicEnumParameter.Source GetLocalDynamicEnumSource(ParameterType type, TDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            var key = Tuple.Create(type, document);
            return m_localDynamicEnumSources.GetOrAdd(key, k => new DynamicEnumParameter.Source());
        }

        public void AddLocalDynamicEnum(LocalDynamicEnumerationData typeData)
        {
            m_hidden[typeData.TypeId] = false;
            m_localDynamicEnums.Add(typeData.TypeId, typeData);
            m_types.Add(typeData.TypeId, new TypeData((name, id, defaultValue, document) => typeData.Make(name, id, defaultValue, GetLocalDynamicEnumSource(typeData.TypeId, document)), typeData.Name));
            Modified.Execute(typeData.TypeId);
        }

        public void Remove(ParameterType id)
        {
            m_types.Remove(id);
            m_types.Remove(ParameterType.ValueSetType.Of(id));
            m_integers.Remove(id);
            m_decimals.Remove(id);
            m_enums.Remove(id);
            m_dynamicEnums.Remove(id);
            m_localDynamicEnums.Remove(id);
            m_localizedStrings.Remove(id);
            Modified.Execute(id);
        }

        public void AddOther(ParameterType id, string name, ParameterGenerator factory)
        {
            m_types.Add(id, new TypeData(factory, name));
            Modified.Execute(id);
        }

        public bool IsInteger(ParameterType type)
        {
            return m_integers.Contains(type);
        }

        public bool IsDecimal(ParameterType type)
        {
            return m_decimals.Contains(type);
        }

        public bool IsLocalizedString(ParameterType type)
        {
            return m_localizedStrings.Contains(type);
        }

        public bool IsEnum(ParameterType type)
        {
            return m_enums.ContainsKey(type);
        }

        public bool IsDynamicEnum(ParameterType type)
        {
            return m_dynamicEnums.Contains(type);
        }

        public bool IsLocalDynamicEnum(ParameterType type)
        {
            return m_localDynamicEnums.Contains(type);
        }

        public string GetTypeName(ParameterType guid)
        {
            return m_types[guid].Name;
        }

        public void RenameType(ParameterType guid, string name)
        {
            m_types[guid].Name = name;
            if (IsInteger(guid))
            {
                IntegerData data = m_integers[guid];
                data = new IntegerData(name, data.TypeId, data.Max, data.Min);
                m_integers[guid] = data;
            }
            else if (IsDecimal(guid))
            {
                DecimalData data = m_decimals[guid];
                data = new DecimalData(name, data.TypeId, data.Max, data.Min);
                m_decimals[guid] = data;
            }
            else if (IsLocalizedString(guid))
            {
                LocalizedStringData data = m_localizedStrings[guid];
                data = new LocalizedStringData(name, data.TypeId/*, data.File*/);
                m_localizedStrings[guid] = data;
            }
            else if (IsEnum(guid))
            {
                var data = m_enums[guid];
                m_enums[guid] = Tuple.Create(name, data.Item2);
            }
            else if (IsDynamicEnum(guid))
            {
                DynamicEnumerationData data = m_dynamicEnums[guid];
                data = new DynamicEnumerationData(name, data.TypeId);
                m_dynamicEnums[guid] = data;
            }
            else if (IsLocalDynamicEnum(guid))
            {
                LocalDynamicEnumerationData data = m_localDynamicEnums[guid];
                data = new LocalDynamicEnumerationData(name, data.TypeId);
                m_localDynamicEnums[guid] = data;
            }
            Modified.Execute(guid);
        }
    }
}
