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
    /// Can generate a Parameter for a given ID<ParameterType>
    /// </summary>
    public class TypeSet
    {
        private Dictionary<ID<ParameterType>, bool> m_hidden = new Dictionary<ID<ParameterType>, bool>();
        private Dictionary<ID<ParameterType>, Func<string, ID<Parameter>, Parameter>> m_types = new Dictionary<ID<ParameterType>, Func<string, ID<Parameter>, Parameter>>();
        private Dictionary<ID<ParameterType>, DynamicEnumerationData> m_dynamicEnums = new Dictionary<ID<ParameterType>, DynamicEnumerationData>();
        private Dictionary<ID<ParameterType>, Tuple<string, MutableEnumeration>> m_enums = new Dictionary<ID<ParameterType>, Tuple<string, MutableEnumeration>>();
        private Dictionary<ID<ParameterType>, IntegerData> m_integers = new Dictionary<ID<ParameterType>, IntegerData>();
        private Dictionary<ID<ParameterType>, DecimalData> m_decimals = new Dictionary<ID<ParameterType>, DecimalData>();

        public IEnumerable<ID<ParameterType>> AllTypes { get { return m_types.Keys; } }
        public event Action<ID<ParameterType>> Modified;

        public IEnumerable<EnumerationData> VisibleEnums { get { return m_enums.Where(kvp => !m_hidden[kvp.Key]).Select(kvp => GetEnumData(kvp.Key)); } }
        public IEnumerable<DynamicEnumerationData> VisibleDynamicEnums { get { return m_dynamicEnums.Where(kvp => !m_hidden[kvp.Key]).Select(kvp=>kvp.Value); } }
        public IEnumerable<IntegerData> VisiblelIntegers { get { return m_integers.Where(kvp => !m_hidden[kvp.Key]).Select(kvp => kvp.Value); } }
        public IEnumerable<DecimalData> VisibleDecimals { get { return m_decimals.Where(kvp => !m_hidden[kvp.Key]).Select(kvp => kvp.Value); } }

        public Parameter Make(ID<ParameterType> typeid, string name, ID<Parameter> id)
        {
            return m_types[typeid](name, id);
        }

        public void AddInteger(IntegerData typeData, bool hidden = false)
        {
            m_hidden[typeData.TypeID] = hidden;
            m_integers.Add(typeData.TypeID, typeData);
            m_types.Add(typeData.TypeID, (name, id) => new IntegerParameter(name, id, typeData.TypeID, m_integers[typeData.TypeID].Definition()));
            Modified.Execute(typeData.TypeID);
        }

        public void ModifyInteger(IntegerData typeData)
        {
            m_types[typeData.TypeID] = (name, id) => new IntegerParameter(name, id, typeData.TypeID, typeData.Definition());
            m_integers[typeData.TypeID] = typeData;
            Modified.Execute(typeData.TypeID);
        }

        public void AddDecimal(DecimalData typeData, bool hidden = false)
        {
            m_hidden[typeData.TypeID] = hidden;
            m_decimals.Add(typeData.TypeID, typeData);
            m_types.Add(typeData.TypeID, (name, id) => new DecimalParameter(name, id, typeData.TypeID, typeData.Definition()));
            Modified.Execute(typeData.TypeID);
        }

        public void ModifyDecimal(DecimalData typeData)
        {
            m_types[typeData.TypeID] = (name, id) => new DecimalParameter(name, id, typeData.TypeID, m_decimals[typeData.TypeID].Definition());
            m_decimals[typeData.TypeID] = typeData;
            Modified.Execute(typeData.TypeID);
        }

        public void AddEnum(EnumerationData typeData, bool hidden = false)
        {
            m_hidden[typeData.Guid] = hidden;
            var elements = typeData.Elements.Select(e => Tuple.Create(e.Guid, e.Name));
            MutableEnumeration enumeration = new MutableEnumeration(elements, typeData.Guid, "");
            m_enums.Add(typeData.Guid, Tuple.Create(typeData.Name, enumeration));
            m_types.Add(m_enums[typeData.Guid].Item2.TypeId, m_enums[typeData.Guid].Item2.Parameter);
            Modified.Execute(typeData.Guid);
        }

        public void ModifyEnum(EnumerationData typeData)
        {
            var e = m_enums[typeData.Guid].Item2;
            foreach (var option in typeData.Elements)
                e.SetName(option.Guid, option.Name);
            Modified.Execute(typeData.Guid);
        }

        public EnumerationData GetEnumData(ID<ParameterType> id)
        {
            return m_enums[id].Item2.GetData(m_enums[id].Item1);
        }

        public void AddDynamicEnum(DynamicEnumerationData typeData, bool hidden = false)
        {
            m_hidden[typeData.TypeID] = hidden;
            m_dynamicEnums.Add(typeData.TypeID, typeData);
            m_types.Add(typeData.TypeID, typeData.Make);
            Modified.Execute(typeData.TypeID);
        }

        public void Remove(ID<ParameterType> id)
        {
            m_types.Remove(id);
            m_integers.Remove(id);
            m_decimals.Remove(id);
            m_enums.Remove(id);
            m_dynamicEnums.Remove(id);
            Modified.Execute(id);
        }

        public void AddOther(ID<ParameterType> id, Func<string, ID<Parameter>, Parameter> factory)
        {
            m_types.Add(id, factory);
            Modified.Execute(id);
        }

        public bool IsInteger(ID<ParameterType> type)
        {
            return m_integers.ContainsKey(type);
        }

        public bool IsDecimal(ID<ParameterType> type)
        {
            return m_decimals.ContainsKey(type);
        }

        public bool IsEnum(ID<ParameterType> type)
        {
            return m_enums.ContainsKey(type);
        }

        public bool IsDynamicEnum(ID<ParameterType> type)
        {
            return m_dynamicEnums.ContainsKey(type);
        }

        public void RenameType(ID<ParameterType> guid, string name)
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
