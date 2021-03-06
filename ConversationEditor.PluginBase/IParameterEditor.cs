﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using Conversation;
using System.Windows.Forms;

namespace ConversationEditor
{
    public struct ParameterEditorSetupData
    {
        public ParameterEditorSetupData(IParameter parameter, ILocalizationEngine localizer, IAudioParameterEditorCallbacks audioProvider, AudioGenerationParameters audioGenerationParameters, Func<string, IEnumerable<string>> autoCompleteSuggestions)
        {
            m_parameter = parameter;
            m_localizer = localizer;
            m_audioProvider = audioProvider;
            m_audioGenerationParameters = audioGenerationParameters;
            m_autoCompleteSuggestions = autoCompleteSuggestions;
        }
        private readonly IParameter m_parameter;
        private readonly ILocalizationEngine m_localizer;
        private readonly IAudioParameterEditorCallbacks m_audioProvider;
        private readonly AudioGenerationParameters m_audioGenerationParameters;
        private readonly Func<string, IEnumerable<string>> m_autoCompleteSuggestions;

        public IParameter Parameter => m_parameter;
        public ILocalizationEngine Localizer => m_localizer;
        public IAudioParameterEditorCallbacks AudioProvider => m_audioProvider;
        public AudioGenerationParameters AudioGenerationParameters => m_audioGenerationParameters;
        public Func<string, IEnumerable<string>> AutoCompleteSuggestions => m_autoCompleteSuggestions;
    }

    public interface IParameterEditor : IDisposable
    {
        void Setup(ParameterEditorSetupData data);
        Control AsControl { get; }
        /// <summary>
        /// Get the action pair for actions to 
        /// Redo: set the edited parameter to the value currently entered in the editor
        /// Undo: return the parameter to its current value
        /// Or null if no change is required (i.e. the two values are the same)
        /// </summary>
        /// <param name="updateAudio">set to an audio file whose inclusion in the project should be update or left null</param>
        UpdateParameterData UpdateParameterAction();

        /// <summary>
        /// null if the current value is valid
        /// otherwise describes the reason it's invalid
        /// </summary>
        /// <returns></returns>
        string IsValid();

        event Action Ok;
    }
}
