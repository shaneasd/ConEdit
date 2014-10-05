using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ConversationEditor;
using Conversation;

namespace PluginPack
{
    public partial class FancyCharacterEditor : UserControl, IParameterEditor<FancyCharacterEditor>
    {
        public FancyCharacterEditor()
        {
            InitializeComponent();
        }

        public static bool WillEdit(Conversation.ID<Conversation.ParameterType> type, WillEdit willEdit)
        {
            throw new NotImplementedException();
        }

        bool IParameterEditor<FancyCharacterEditor>.WillEdit(Conversation.ID<Conversation.ParameterType> type, WillEdit willEdit)
        {
            return FancyCharacterEditor.WillEdit(type, willEdit);
        }

        public void Setup(ParameterEditorSetupData data)
        {
            throw new NotImplementedException();
        }

        public FancyCharacterEditor AsControl
        {
            get { return this; }
        }

        public UpdateParameterData UpdateParameterAction()
        {
            throw new NotImplementedException();
        }

        public bool IsValid()
        {
            throw new NotImplementedException();
        }

        public string DisplayName
        {
            get { throw new NotImplementedException(); }
        }

        public event Action Ok;
    }

    public class FancyCharacterEditorFactory : IParameterEditorFactory
    {
        public bool WillEdit(ID<ParameterType> type, WillEdit willEdit)
        {
            return true; //TODO: Filter to just characters
        }

        public string Name
        {
            get { return "Fancy Character Editor"; }
        }

        public IParameterEditor<Control> Make()
        {
            return new FancyCharacterEditor(); //TODO: Some sort of initialization presumably
        }


        public Guid Guid
        {
            get { return Guid.Parse("20873974-f9de-4fc8-a024-bef48e4c6280"); }
        }
    }
}
