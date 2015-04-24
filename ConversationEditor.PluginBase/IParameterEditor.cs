using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using Conversation;

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

    public interface ILocalizationEngine
    {
        Tuple<ID<LocalizedText>, SimpleUndoPair> DuplicateActions(ID<LocalizedText> iD);

        bool CanLocalize { get; }

        string Localize(ID<LocalizedText> id);

        SimpleUndoPair SetLocalizationAction(ID<LocalizedText> id, string p);
    }

    public class ParameterEditorSetupData
    {
        public ParameterEditorSetupData(IParameter parameter, ILocalizationEngine localizer, IAudioProvider2 audioProvider, AudioGenerationParameters audioGenerationParameters)
        {
            Parameter = parameter;
            Localizer = localizer;
            AudioProvider = audioProvider;
            AudioGenerationParameters = audioGenerationParameters;
        }
        public readonly IParameter Parameter;
        public readonly ILocalizationEngine Localizer;
        public readonly IAudioProvider2 AudioProvider;
        public readonly AudioGenerationParameters AudioGenerationParameters;
    }
    public interface IParameterEditor<out TUI>
    {
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

        event Action Ok;
    }
}
