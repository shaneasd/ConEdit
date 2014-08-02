using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using System.Windows.Forms;
using System.Reflection;
using Utilities;

namespace ConversationEditor
{
    public interface IParameterEditor<out TUI>
    {
        bool WillEdit(ID<ParameterType> type, WillEdit willEdit);
        void Setup(IParameter parameter, LocalizationEngine localizer, IAudioProvider audioProvider);
        TUI AsControl { get; }
        /// <summary>
        /// Get the action pair for actions to 
        /// Redo: set the edited parameter to the value currently entered in the editor
        /// Undo: return the parameter to its current value
        /// Or null if no change is required (i.e. the two values are the same)
        /// </summary>
        SimpleUndoPair? UpdateParameterAction();
        bool IsValid();
        string DisplayName { get; }

        event Action Ok;
    }

    public class ParameterEditorChoice : TypeChoice
    {
        public ParameterEditorChoice(Type type)
            : base(type)
        {
        }

        public ParameterEditorChoice(string assembly, string type)
            : base(assembly, type)
        {
        }

        public bool WillEdit(ID<ParameterType> type, WillEdit willEdit)
        {
            return GetEditor().WillEdit(type, willEdit);
        }

        public override string ToString()
        {
            return DisplayName;
        }

        public override string DisplayName
        {
            get { return GetEditor().DisplayName; }
        }

        private IParameterEditor<Control> GetEditor()
        {
            return m_type.GetConstructor(new Type[0]).Invoke(new object[0]) as IParameterEditor<Control>;
        }

        public IParameterEditor<Control> MakeEditor(IParameter p, LocalizationEngine localizer, IAudioProvider audioProvider)
        {
            var ed = GetEditor();
            ed.Setup(p, localizer, audioProvider);
            return ed;
        }
    }
}
