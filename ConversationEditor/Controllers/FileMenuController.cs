using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Conversation;
using ConversationEditor.GUIs;
using Utilities;

namespace ConversationEditor
{
    using ConversationNode = Conversation.ConversationNode<Conversation.INodeGUI<TransitionNoduleUIInfo>, TransitionNoduleUIInfo>;

    public class FileMenuController
    {
        public Project m_project;

        private ConversationFile m_currentFile;
        public ConversationFile CurrentFile
        {
            get
            {
                return m_currentFile;
            }
        }

        //private List<ConversationFile> m_files = new List<ConversationFile>();
        //public IEnumerable<ConversationFile> Files { get { return m_files; } }

        private Func<NodeFactory> m_nodeFactory;
        private SaveFileDialog saveFileDialog1 = new SaveFileDialog() { DefaultExt = "xml", Filter = "Conversations|*.xml" };
        private OpenFileDialog openFileDialog1 = new OpenFileDialog() { DefaultExt = "xml", Filter = "Conversations|*.xml" };
        private IDataSource m_datasource;

        public event Action FileNameChanged;
        public event Action FilesChanged;
        public event Action SelectedFileChanged;
        public event Action Refresh;
        private ConversationList m_list;

        public void New()
        {
            var jumpsource = new DynamicEnumParameter.Source();
            IdProvider idprovider = new IdProvider();
            var nodes = new List<ConversationNode>();
            if (!(m_datasource is DummyDataSource)) //TODO: This could be handled more elegantly
            {
                nodes.Add(m_nodeFactory().MakeNode(m_datasource.GetNode(SpecialNodes.START_GUID)(idprovider.Next()), new Point(50, 50), new Size(10, 10)));
            }
            var file = new ConversationFile(idprovider, nodes, new List<NodeGroup>(), null, jumpsource);
            Select(file);
            Refresh.Execute();
        }

        public void SetDataSource(IDataSource datasource)
        {
            m_datasource = datasource;
        }

        public FileMenuController(IDataSource datasource, Func<NodeFactory> nodeFactory, ConversationList list) //TODO: Don't pass in the whole list...
        {
            m_nodeFactory = nodeFactory;
            m_datasource = datasource;
            m_list = list;
        }

        public bool SaveAs()
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                CurrentFile.SaveAs(new FileStream(saveFileDialog1.FileName, FileMode.OpenOrCreate, FileAccess.Write));
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Save()
        {
            CurrentFile.Save();
            return true;
        }

        public ConversationFile Open()
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                return Open(new FileStream(openFileDialog1.FileName, FileMode.Open, FileAccess.ReadWrite));
            else
                return null;
        }

        public ConversationFile Open(FileStream file)
        {
            saveFileDialog1.FileName = file.Name;

            var cf = ConversationFile.Load(file, m_datasource, m_nodeFactory());
            Select(cf);
            FileNameChanged.Execute();
            Refresh.Execute();

            return cf;
        }

        internal void Clear()
        {
            m_project.CloseFiles();
            New();
            FilesChanged.Execute();
        }

        internal void SetProject(Project p)
        {
            m_project = p;
        }

        internal void Select(ConversationFile conversation)
        {
            m_currentFile = conversation;
            SelectedFileChanged.Execute();
        }
    }
}
