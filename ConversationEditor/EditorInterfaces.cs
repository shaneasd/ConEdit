﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using System.Windows.Forms;
using System.Reflection;
using Utilities;

namespace ConversationEditor
{
    public class UpdateParameterData
    {
        /// <summary>
        /// Undo/Redo actions to perform to change the state of the parameter based on the editor selection
        /// </summary>
        public SimpleUndoPair? Actions = null;
        /// <summary>
        /// An audio file whose inclusion in the project should be updated
        /// </summary>
        public Audio? Audio = null;

        public static implicit operator UpdateParameterData(SimpleUndoPair? actions)
        {
            return new UpdateParameterData() { Actions = actions };
        }
    }

    public class ParameterEditorSetupData
    {
        public ParameterEditorSetupData(IParameter parameter, LocalizationEngine localizer, IAudioProvider audioProvider, AudioGenerationParameters audioGenerationParameters)
        {
            Parameter = parameter;
            Localizer = localizer;
            AudioProvider = audioProvider;
            AudioGenerationParameters = audioGenerationParameters;
        }
        public readonly IParameter Parameter;
        public readonly LocalizationEngine Localizer;
        public readonly IAudioProvider AudioProvider;
        public readonly AudioGenerationParameters AudioGenerationParameters;
    }

    public interface IParameterEditor<out TUI>
    {
        bool WillEdit(ID<ParameterType> type, WillEdit willEdit);
        void Setup(ParameterEditorSetupData data);
        TUI AsControl { get; }
        /// <summary>
        /// Get the action pair for actions to 
        /// Redo: set the edited parameter to the value currently entered in the editor
        /// Undo: return the parameter to its current value
        /// Or null if no change is required (i.e. the two values are the same)
        /// </summary>
        /// <param name="updateAudio">set to an audio file whose inclusion in the project should be update or left null</param>
        UpdateParameterData UpdateParameterAction();
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

        public IParameterEditor<Control> MakeEditor(ParameterEditorSetupData data)
        {
            var ed = GetEditor();
            ed.Setup(data);
            return ed;
        }
    }
}
