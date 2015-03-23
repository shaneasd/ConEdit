using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;

namespace Conversation
{
    /// <summary>
    /// Keeps track of all types within a domain.
    /// Can be queried to determine the base type of a typeID
    /// Can generate a Parameter for a given ParameterType
    /// </summary>
    public class TypeSet
    {
        private Dictionary<ParameterType, bool> m_hidden = new Dictionary<ParameterType, bool>();
        private Dictionary<ParameterType, Func<string, ID<Parameter>, string, Parameter>> m_types = new Dictionary<ParameterType, Func<string, ID<Parameter>, string, Parameter>>();
        private Dictionary<ParameterType, string> m_typeNames = new Dictionary<ParameterType, string>();
        private Dictionary<ParameterType, DynamicEnumerationData> m_dynamicEnums = new Dictionary<ParameterType, DynamicEnumerationData>();
        private Dictionary<ParameterType, Tuple<string, MutableEnumeration>> m_enums = new Dictionary<ParameterType, Tuple<string, MutableEnumeration>>();
        private Dictionary<ParameterType, IntegerData> m_integers = new Dictionary<ParameterType, IntegerData>();
        private Dictionary<ParameterType, DecimalData> m_decimals = new Dictionary<ParameterType, DecimalData>();

        public IEnumerable<ParameterType> AllTypes { get { return m_types.Keys; } }
        public event Action<ParameterType> Modified;

        public IEnumerable<EnumerationData> VisibleEnums { get { return m_enums.Where(kvp => !m_hidden[kvp.Key]).Select(kvp => GetEnumData(kvp.Key)); } }
        public IEnumerable<DynamicEnumerationData> VisibleDynamicEnums { get { return m_dynamicEnums.Where(kvp => !m_hidden[kvp.Key]).Select(kvp=>kvp.Value); } }
        public IEnumerable<IntegerData> VisiblelIntegers { get { return m_integers.Where(kvp => !m_hidden[kvp.Key]).Select(kvp => kvp.Value); } }
        public IEnumerable<DecimalData> VisibleDecimals { get { return m_decimals.Where(kvp => !m_hidden[kvp.Key]).Select(kvp => kvp.Value); } }

        public Parameter Make(ParameterType typeid, string name, ID<Parameter> id, string defaultValue)
        {
            return m_types[typeid](name, id, defaultValue);
        }

        public void AddInteger(IntegerData typeData, bool hidden = false)
        {
            m_hidden[typeData.TypeID] = hidden;
            m_integers.Add(typeData.TypeID, typeData);
            m_types.Add(typeData.TypeID, (name, id, defaultValue) => new IntegerParameter(name, id, typeData.TypeID, m_integers[typeData.TypeID].Definition(), defaultValue));
            m_typeNames.Add(typeData.TypeID, typeData.Name);
            Modified.Execute(typeData.TypeID);
        }

        public void ModifyInteger(IntegerData typeData)
        {
            m_types[typeData.TypeID] = (name, id, defaultValue) => new IntegerParameter(name, id, typeData.TypeID, typeData.Definition(), defaultValue);
            m_integers[typeData.TypeID] = typeData;
            m_typeNames.Add(typeData.TypeID, typeData.Name);
            Modified.Execute(typeData.TypeID);
        }

        public void AddDecimal(DecimalData typeData, bool hidden = false)
        {
            m_hidden[typeData.TypeID] = hidden;
            m_decimals.Add(typeData.TypeID, typeData);
            m_types.Add(typeData.TypeID, (name, id, defaultValue) => new DecimalParameter(name, id, typeData.TypeID, typeData.Definition(), defaultValue));
            m_typeNames.Add(typeData.TypeID, typeData.Name);
            Modified.Execute(typeData.TypeID);
        }

        public void ModifyDecimal(DecimalData typeData)
        {
            m_types[typeData.TypeID] = (name, id, defaultValue) => new DecimalParameter(name, id, typeData.TypeID, m_decimals[typeData.TypeID].Definition(), defaultValue);
            m_typeNames.Add(typeData.TypeID, typeData.Name);
            m_decimals[typeData.TypeID] = typeData;
            Modified.Execute(typeData.TypeID);
        }

        public void AddEnum(EnumerationData typeData, bool hidden = false)
        {
            var enumType = typeData.TypeID;
            var setType = ParameterType.Set.Of(enumType);

            m_hidden[enumType] = hidden;
            var elements = typeData.Elements.Select(e => Tuple.Create(e.Guid, e.Name));
            MutableEnumeration enumeration = new MutableEnumeration(elements, enumType, "");
            m_enums.Add(enumType, Tuple.Create(typeData.Name, enumeration));
            m_types.Add(enumType, m_enums[enumType].Item2.ParameterEnum);
            m_types.Add(setType, m_enums[enumType].Item2.ParameterSet);
            m_typeNames.Add(enumType, typeData.Name);
            m_typeNames.Add(setType, "Set of " + typeData.Name);
            Modified.Execute(enumType);
        }

        public void ModifyEnum(EnumerationData typeData)
        {
            if (m_enums.ContainsKey(typeData.TypeID)) //If we're removing an entire domain file, an enum declaration can be removed before its values. In this circumstance, when the values are removed, this method will be called but the enum wont exist.
            {
                var e = m_enums[typeData.TypeID].Item2;
                e.SetOptions(typeData.Elements);
                Modified.Execute(typeData.TypeID);
            }
        }

        public EnumerationData GetEnumData(ParameterType id)
        {
            return m_enums[id].Item2.GetData(m_enums[id].Item1);
        }

        public void AddDynamicEnum(DynamicEnumerationData typeData, bool hidden = false)
        {
            m_hidden[typeData.TypeID] = hidden;
            m_dynamicEnums.Add(typeData.TypeID, typeData);
            m_types.Add(typeData.TypeID, typeData.Make);
            m_typeNames.Add(typeData.TypeID, typeData.Name);
            Modified.Execute(typeData.TypeID);
        }

        public void Remove(ParameterType id)
        {
            m_types.Remove(id);
            m_integers.Remove(id);
            m_decimals.Remove(id);
            m_enums.Remove(id);
            m_dynamicEnums.Remove(id);
            Modified.Execute(id);
        }

        public void AddOther(ParameterType id, string name, Func<string, ID<Parameter>, string, Parameter> factory)
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
            Modified.Execute(guid);
        }
    }

}
