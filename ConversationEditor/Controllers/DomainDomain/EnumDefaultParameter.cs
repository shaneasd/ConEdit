using Conversation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace ConversationEditor
{
    public class EnumDefaultParameter : Parameter, IDynamicEnumParameter
    {
        private bool m_corrupted;
        public override bool Corrupted { get { return m_corrupted; } }

        public new static ParameterType TypeId { get; } = ParameterType.Parse("82e83436-f1b0-4f71-8882-51c171d14ff3");

        Func<Dictionary<ParameterType, IEnumerable<EnumerationData.Element>>> m_enumOptions;

        private Dictionary<ParameterType, IEnumerable<EnumerationData.Element>> EnumOptions { get { return m_enumOptions(); } }

        Func<ParameterType> m_currentEnumType;

        Guid m_valueGuid;
        string m_value;

        public EnumDefaultParameter(Func<Dictionary<ParameterType, IEnumerable<EnumerationData.Element>>> enumOptions, Func<ParameterType> currentEnumType)
            : base("Default", DomainIDs.ParameterDefault, TypeId, null)
        {
            m_value = "";
            m_valueGuid = Guid.Empty;
            m_enumOptions = enumOptions;
            m_currentEnumType = currentEnumType;
        }

        private void UpdateValueGuid()
        {
            m_valueGuid = EnumOptions[m_currentEnumType()].FirstOrDefault(a => a.Name == m_value).Guid;
        }

        private void UpdateText()
        {
            string newText = null;
            if (m_valueGuid != Guid.Empty && EnumOptions.ContainsKey(m_currentEnumType()))
            {
                var element = EnumOptions[m_currentEnumType()].FirstOrDefault(a => a.Guid == m_valueGuid);
                if (element.Guid != Guid.Empty)
                    newText = element.Name;
            }
            m_value = newText ?? m_value;
        }

        public string Value
        {
            get
            {
                UpdateText();
                return m_value;
            }
        }

        public Either<Guid, string> BetterValue
        {
            get
            {
                UpdateText();
                if (m_valueGuid != Guid.Empty)
                    return m_valueGuid;
                else
                    return m_value;
            }
        }

        public SimpleUndoPair? SetValueAction(string value)
        {
            var oldGuid = m_valueGuid;
            var oldValue = m_value;
            var oldCorrupted = Corrupted;

            if (value.Equals(oldValue) && !oldCorrupted)
                return null;

            return new SimpleUndoPair
            {
                Redo = () => { m_value = value; m_corrupted = false; UpdateValueGuid(); },
                Undo = () => { m_value = oldValue; m_corrupted = oldCorrupted; m_valueGuid = oldGuid; }
            };
        }

        public override string DisplayValue(Func<Id<LocalizedStringType>, Id<LocalizedText>, string> localize)
        {
            UpdateText();
            return m_value;
        }

        protected override string InnerValueAsString()
        {
            UpdateText();
            if (m_valueGuid != Guid.Empty)
                return m_valueGuid.ToString();
            else
                return m_value;
        }

        protected override void DeserialiseValue(string value)
        {
            if (Guid.TryParse(value, out m_valueGuid))
            {
                m_value = null;
            }
            else
            {
                m_valueGuid = Guid.Empty;
                m_value = value;
            }
            m_corrupted = false;
        }

        public IEnumerable<string> Options
        {
            get
            {
                ParameterType guid = m_currentEnumType();
                if (EnumOptions.ContainsKey(guid))
                    return EnumOptions[guid].Select(a => a.Name);
                else
                    return new string[0];
            }
        }

        public bool Local
        {
            get
            {
                return false;
            }
        }

        public void MergeInto(DynamicEnumParameter.Source newSource)
        {
            //Do nothing. This object maintains its own source and nothing needs to know about it
            //This method can, however, be called so that, for example, the editor can switch parameters to a junk data source.
        }
    }

}
