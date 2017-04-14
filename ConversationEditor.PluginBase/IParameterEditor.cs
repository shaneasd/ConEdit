using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using Conversation;
using System.Windows.Forms;

namespace ConversationEditor
{
    public interface ILocalizationEngine
    {
        Tuple<Id<LocalizedText>, SimpleUndoPair> DuplicateActions(Id<LocalizedText> iD);

        bool CanLocalize { get; }

        string Localize(Id<LocalizedText> id);

        SimpleUndoPair SetLocalizationAction(Id<LocalizedText> id, string p);

        SimpleUndoPair ClearLocalizationAction(Id<LocalizedText> id);
    }

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

        public IParameter Parameter { get { return m_parameter; } }
        public ILocalizationEngine Localizer { get { return m_localizer; } }
        public IAudioParameterEditorCallbacks AudioProvider { get { return m_audioProvider; } }
        public AudioGenerationParameters AudioGenerationParameters { get { return m_audioGenerationParameters; } }
        public Func<string, IEnumerable<string>> AutoCompleteSuggestions { get { return m_autoCompleteSuggestions; } }
    }

    //TODO: Fairly sure AsControl could just return Control and we could get rid of TUI
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
