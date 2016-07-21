﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.IO;

using TDocument = System.Object;

namespace Conversation
{
    public delegate Parameter ParameterGenerator(string name, Id<Parameter> id, string value, TDocument document);
    /// <summary>
    /// Keeps track of all types within a domain.
    /// Can be queried to determine the base type of a typeID
    /// Can generate a Parameter for a given ParameterType
    /// </summary>
    public class TypeSet
    {
        private Dictionary<ParameterType, bool> m_hidden = new Dictionary<ParameterType, bool>();
        private Dictionary<ParameterType, ParameterGenerator> m_types = new Dictionary<ParameterType, ParameterGenerator>();
        private Dictionary<ParameterType, string> m_typeNames = new Dictionary<ParameterType, string>();
        private Dictionary<ParameterType, DynamicEnumerationData> m_dynamicEnums = new Dictionary<ParameterType, DynamicEnumerationData>();
        private Dictionary<ParameterType, LocalDynamicEnumerationData> m_localDynamicEnums = new Dictionary<ParameterType, LocalDynamicEnumerationData>();
        private Dictionary<ParameterType, Tuple<string, MutableEnumeration>> m_enums = new Dictionary<ParameterType, Tuple<string, MutableEnumeration>>();
        private Dictionary<ParameterType, IntegerData> m_integers = new Dictionary<ParameterType, IntegerData>();
        private Dictionary<ParameterType, DecimalData> m_decimals = new Dictionary<ParameterType, DecimalData>();

        public IEnumerable<ParameterType> AllTypes { get { return m_types.Keys; } }
        public event Action<ParameterType> Modified;

        public IEnumerable<EnumerationData> VisibleEnums { get { return m_enums.Where(kvp => !m_hidden[kvp.Key]).Select(kvp => GetEnumData(kvp.Key)); } }
        public IEnumerable<DynamicEnumerationData> VisibleDynamicEnums { get { return m_dynamicEnums.Where(kvp => !m_hidden[kvp.Key]).Select(kvp => kvp.Value); } }
        public IEnumerable<LocalDynamicEnumerationData> VisibleLocalDynamicEnums { get { return m_localDynamicEnums.Where(kvp => !m_hidden[kvp.Key]).Select(kvp => kvp.Value); } }
        public IEnumerable<IntegerData> VisibleIntegers { get { return m_integers.Where(kvp => !m_hidden[kvp.Key]).Select(kvp => kvp.Value); } }
        public IEnumerable<DecimalData> VisibleDecimals { get { return m_decimals.Where(kvp => !m_hidden[kvp.Key]).Select(kvp => kvp.Value); } }

        public Parameter Make(ParameterType typeid, string name, Id<Parameter> id, string defaultValue, TDocument document)
        {
            return m_types[typeid](name, id, defaultValue, document);
        }

        public void AddInteger(IntegerData typeData)
        {
            m_hidden[typeData.TypeId] = false;
            m_integers.Add(typeData.TypeId, typeData);
            m_types.Add(typeData.TypeId, (name, id, defaultValue, document) => new IntegerParameter(name, id, typeData.TypeId, m_integers[typeData.TypeId].Definition(), defaultValue));
            m_typeNames.Add(typeData.TypeId, typeData.Name);
            Modified.Execute(typeData.TypeId);
        }

        public void ModifyInteger(IntegerData typeData)
        {
            m_types[typeData.TypeId] = (name, id, defaultValue, document) => new IntegerParameter(name, id, typeData.TypeId, typeData.Definition(), defaultValue);
            m_integers[typeData.TypeId] = typeData;
            m_typeNames.Add(typeData.TypeId, typeData.Name);
            Modified.Execute(typeData.TypeId);
        }

        public void AddDecimal(DecimalData typeData)
        {
            m_hidden[typeData.TypeId] = false;
            m_decimals.Add(typeData.TypeId, typeData);
            m_types.Add(typeData.TypeId, (name, id, defaultValue, document) => new DecimalParameter(name, id, typeData.TypeId, typeData.Definition(), defaultValue));
            m_typeNames.Add(typeData.TypeId, typeData.Name);
            Modified.Execute(typeData.TypeId);
        }

        public void ModifyDecimal(DecimalData typeData)
        {
            m_types[typeData.TypeId] = (name, id, defaultValue, document) => new DecimalParameter(name, id, typeData.TypeId, m_decimals[typeData.TypeId].Definition(), defaultValue);
            m_typeNames.Add(typeData.TypeId, typeData.Name);
            m_decimals[typeData.TypeId] = typeData;
            Modified.Execute(typeData.TypeId);
        }

        public void AddEnum(EnumerationData typeData, bool hidden)
        {
            var enumType = typeData.TypeId;
            var setType = ParameterType.Set.Of(enumType);

            m_hidden[enumType] = hidden;
            var elements = typeData.Elements.Select(e => Tuple.Create(e.Guid, e.Name));
            MutableEnumeration enumeration = new MutableEnumeration(elements, enumType, "");
            m_enums.Add(enumType, Tuple.Create(typeData.Name, enumeration));
            m_types.Add(enumType, (a, b, c, d) => m_enums[enumType].Item2.ParameterEnum(a, b, c));
            m_types.Add(setType, (a, b, c, d) => m_enums[enumType].Item2.ParameterSet(a, b, c));
            m_typeNames.Add(enumType, typeData.Name);
            m_typeNames.Add(setType, "Set of " + typeData.Name);
            Modified.Execute(enumType);
        }

        public void ModifyEnum(EnumerationData typeData)
        {
            if (m_enums.ContainsKey(typeData.TypeId)) //If we're removing an entire domain file, an enum declaration can be removed before its values. In this circumstance, when the values are removed, this method will be called but the enum wont exist.
            {
                var e = m_enums[typeData.TypeId].Item2;
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
            m_types.Add(typeData.TypeId, (a, b, c, document) => typeData.Make(a, b, c, GetDynamicEnumSource(typeData.TypeId)));
            m_typeNames.Add(typeData.TypeId, typeData.Name);
            Modified.Execute(typeData.TypeId);
        }

        Dictionary<Tuple<ParameterType, TDocument>, DynamicEnumParameter.Source> m_localDynamicEnumSources = new Dictionary<Tuple<ParameterType, TDocument>, DynamicEnumParameter.Source>();

        public DynamicEnumParameter.Source GetDynamicEnumSource(ParameterType type)
        {
            var key = Tuple.Create(type, (object)null);
            if (!m_localDynamicEnumSources.ContainsKey(key))
                m_localDynamicEnumSources[key] = new DynamicEnumParameter.Source();
            return m_localDynamicEnumSources[key];
        }

        public DynamicEnumParameter.Source GetLocalDynamicEnumSource(ParameterType type, TDocument document)
        {
            var key = Tuple.Create(type, document);
            if (!m_localDynamicEnumSources.ContainsKey(key))
                m_localDynamicEnumSources[key] = new DynamicEnumParameter.Source();
                return m_localDynamicEnumSources[key];
        }

        public void AddLocalDynamicEnum(LocalDynamicEnumerationData typeData)
        {
            m_hidden[typeData.TypeId] = false;
            m_localDynamicEnums.Add(typeData.TypeId, typeData);
            m_types.Add(typeData.TypeId, (name, id, defaultValue, document) => typeData.Make(name, id, defaultValue, GetLocalDynamicEnumSource(typeData.TypeId, document)));
            m_typeNames.Add(typeData.TypeId, typeData.Name);
            Modified.Execute(typeData.TypeId);
        }

        public void Remove(ParameterType id)
        {
            m_types.Remove(id);
            m_integers.Remove(id);
            m_decimals.Remove(id);
            m_enums.Remove(id);
            m_dynamicEnums.Remove(id);
            m_localDynamicEnums.Remove(id);
            Modified.Execute(id);
        }

        public void AddOther(ParameterType id, string name, ParameterGenerator factory)
        {
            m_types.Add(id, factory);
            m_typeNames.Add(id, name);
            Modified.Execute(id);
        }

        public bool IsInteger(ParameterType type)
        {
            return m_integers.ContainsKey(type);
        }

        public bool IsDecimal(ParameterType type)
        {
            return m_decimals.ContainsKey(type);
        }

        public bool IsEnum(ParameterType type)
        {
            return m_enums.ContainsKey(type);
        }

        public bool IsDynamicEnum(ParameterType type)
        {
            return m_dynamicEnums.ContainsKey(type);
        }

        public bool IsLocalDynamicEnum(ParameterType type)
        {
            return m_localDynamicEnums.ContainsKey(type);
        }

        public string GetTypeName(ParameterType guid)
        {
            return m_typeNames[guid];
        }

        public void RenameType(ParameterType guid, string name)
        {
            if (IsInteger(guid))
            {
                var data = m_integers[guid];
                data.Name = name;
                m_integers[guid] = data;
            }
            else if (IsDecimal(guid))
            {
                var data = m_decimals[guid];
                data.Name = name;
                m_decimals[guid] = data;
            }
            else if (IsEnum(guid))
            {
                var data = m_enums[guid];
                m_enums[guid] = Tuple.Create(name, data.Item2);
            }
            else if (IsDynamicEnum(guid))
            {
                var data = m_dynamicEnums[guid];
                data.Name = name;
                m_dynamicEnums[guid] = data;
            }
            else if (IsLocalDynamicEnum(guid))
            {
                var data = m_localDynamicEnums[guid];
                data.Name = name;
                m_localDynamicEnums[guid] = data;
            }
            Modified.Execute(guid);
        }
    }

}
