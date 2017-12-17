using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Conversation;
using Utilities;

using TControl = Utilities.UI.MySuggestionBox<string>;
using TItem = Utilities.UI.MyComboBoxItem<string>;

namespace ConversationEditor
{
    public class DefaultLocalDynamicEnumEditorFactory : IParameterEditorFactory
    {
        public static readonly Guid StaticId = Guid.Parse("7d9cb0ef-42fa-4975-8811-8ca18c5972d7");
        public bool WillEdit(ParameterType type, WillEdit queries)
        {
            return queries.IsDynamicEnum(type);
        }

        public string Name
        {
            get { return "Default Local Dynamic Enumeration Editor"; }
        }

        public Guid Guid
        {
            get { return StaticId; }
        }

        public IParameterEditor Make(IColorScheme scheme)
        {
            return new DefaultDynamicEnumEditor(scheme);
        }
    }
}
