using Conversation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utilities;

namespace ConversationEditor
{
    public partial class SetUpLocalizationsForm : Form
    {
        private Action<Project.TData.LocalizerSetData[]> m_registerResult;
        private IEnumerable<Id<LocalizedStringType>> m_stringTypes;
        private Func<Id<FileInProject>, ILocalizationFile> m_idToFile;
        private IEnumerable<ILocalizationFile> m_localizationFiles;

        public SetUpLocalizationsForm()
        {
            InitializeComponent();
        }

        public void Setup(Action<Project.TData.LocalizerSetData[]> registerResult, IEnumerable<Project.TData.LocalizerSetData> localizationSets, IEnumerable<ILocalizationFile> localizationFiles, IEnumerable<Id<LocalizedStringType>> stringTypes, Func<Id<FileInProject>, ILocalizationFile> idToFile, Func<ParameterType, string> getTypeName)
        {
            m_registerResult = registerResult;
            m_stringTypes = stringTypes;
            m_idToFile = idToFile;
            m_localizationFiles = localizationFiles;

            dataGridView1.DataError += DataGridView1_DataError;

            var nameColumn = new DataGridViewTextBoxColumn() { HeaderText = "Name" };
            dataGridView1.Columns.Add(nameColumn);

            foreach (var type in stringTypes)
            {
                string typeName = getTypeName(ParameterType.Basic.ConvertFrom(type));
                var column = new DataGridViewComboBoxColumn() { HeaderText = typeName };
                foreach (var file in localizationFiles)
                    column.Items.Add(file.File.File.Name);
                column.Items.Add("");
                column.ValueType = typeof(string);
                dataGridView1.Columns.Add(column);
            }

            foreach (var set in localizationSets)
            {
                var ids = stringTypes.Select(type => set.Sources.LookupOrDefault(type, null));
                var files = ids.Select(id => id != null ? idToFile(id) : null);
                var cellValues = files.Select(f => f?.File?.File?.Name ?? "");
                object[] values = set.Name.Only<object>().Concat(cellValues.Cast<object>()).ToArray();
                int rowIndex = dataGridView1.Rows.Add(values);
                dataGridView1.Rows[rowIndex].Tag = set.Id;
            }
        }

        private void DataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Project.TData.LocalizerSetData[] result = new Project.TData.LocalizerSetData[dataGridView1.RowCount - 1]; //As far as I can tell the last row is, by requirement, empty
            for (int i = 0; i < result.Length; i++)
            {
                var row = dataGridView1.Rows[i];
                Dictionary<Id<LocalizedStringType>, Id<FileInProject>> sources = new Dictionary<Id<LocalizedStringType>, Id<FileInProject>>();
                foreach (var type in m_stringTypes.Select((value, index) => new { Value = value, Index = index }))
                {
                    sources[type.Value] = m_localizationFiles.FirstOrDefault(f => f.File.File.Name == (string)row.Cells[type.Index + 1].Value)?.Id ?? Id<FileInProject>.FromGuid(Guid.Empty);
                }
                var id = row.Tag as Id<Project.TData.LocalizerSetData> ?? Id<Project.TData.LocalizerSetData>.New();
                result[i] = new Project.TData.LocalizerSetData(id, row.Cells[0].Value.ToString(), sources);
            }
            if (CheckResultSane(result))
            {
                m_registerResult(result);
                Close();
            }
        }

        /// <summary>
        /// Check that the input array of localization sets is sane (i.e. can be handled by the software)
        /// If so, return true.
        /// If not, display an appropriate message to the user and then return false.
        /// </summary>
        private bool CheckResultSane(Project.TData.LocalizerSetData[] result)
        {
            if (result.GroupBy(set => set.Name).Any(g => g.Count() != 1))
            {
                MessageBox.Show("Each localization set must have a unique name");
                return false;
            }
            return true;
        }

        public static void SetupLocalizations(IProject project)
        {
            SetUpLocalizationsForm form = new SetUpLocalizationsForm();

            Action<Project.TData.LocalizerSetData[]> registerResult = project.Localizer.UpdateLocalizationSets;

            form.Setup(registerResult,
                       project.Localizer.LocalizationSets,
                       project.LocalizationFiles,
                       project.DomainDataSource.ParameterTypes.Where(project.DomainDataSource.IsLocalizedString).Select(t => Id<LocalizedStringType>.FromGuid(t.Guid)),
                       id => project.LocalizationFiles.FirstOrDefault(file => file.Id == id),
                       project.DomainDataSource.GetTypeName);
            form.ShowDialog();
        }
    }
}
