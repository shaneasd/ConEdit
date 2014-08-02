using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Conversation;
using Utilities;

namespace ConversationEditor
{
    public partial class DataEditor : Form
    {
        class TempEnumeration
        {
            public string TypeName;
            public List<Enumeration.EnumObj> Options;
            public Enumeration.EnumObj Default;
            private Enumeration m_wrapped;

            public TempEnumeration(Enumeration enumeration)
            {
                m_wrapped = enumeration;
                TypeName = enumeration.TypeName;
                Options = enumeration.Options.ToList();
                Default = enumeration.Default;
            }

            internal void Save()
            {
                m_wrapped.Update(Options, Default);
            }
        }

        private IDataSource m_datasource;
        private List<Action> m_updates = new List<Action>();
        private ToStringWrapper<TempEnumeration> m_lastSelection = null;

        public DataEditor()
        {
            InitializeComponent();
        }

        public void Initialize(IDataSource datasource)
        {
            m_datasource = datasource;
            foreach (var type in m_datasource.Enumerations)
            {
                var temp = new TempEnumeration(type);
                comboBox1.Items.Add(new ToStringWrapper<TempEnumeration>(temp, temp.TypeName));
            }
            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
            }
        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            SaveUpdate();

            dataGridView1.Rows.Clear();
            foreach (var value in SelectedEnumeration.Value.Options)
            {
                int row = dataGridView1.Rows.Add(value);
                dataGridView1[0, row].Tag = value;
            }

            comboBox2.Items.Clear();
            comboBox2.Items.AddRange(SelectedEnumeration.Value.Options.Select(a => a.Name).ToArray());
            if (SelectedEnumeration.Value.Default != null)
                comboBox2.Text = SelectedEnumeration.Value.Default.Name;

            m_lastSelection = SelectedEnumeration;
        }

        private void SaveUpdate()
        {
            if (m_lastSelection != null)
            {
                var newValues = GetValues();
                var enumeration = m_lastSelection.Value;
                var def = newValues.SingleOrDefault(a => a.Name == comboBox2.Text, a => a, string.IsNullOrWhiteSpace(comboBox2.Text) ? null : new Enumeration.EnumObj(comboBox2.Text, Guid.NewGuid()));
                enumeration.Options = newValues.ToList();
                enumeration.Default = def;
                m_updates.Add(() => { enumeration.Save(); });
            }
        }

        private IEnumerable<Enumeration.EnumObj> GetValues()
        {
            var cells = Enumerable.Range(0, dataGridView1.Rows.Count).Select(i => dataGridView1[0, i]);

            Func<DataGridViewCell, Enumeration.EnumObj> GetEnumObj = cell =>
            {
                var cellString = cell.Value as string;
                if (cell.Value == null)
                {
                    return null;
                }
                else if (cellString != null) //The user changed it
                {
                    if (!string.IsNullOrWhiteSpace(cellString))
                        if (cell.Tag == null) //It's new
                            return new Enumeration.EnumObj(cellString, Guid.NewGuid());
                        else
                            return new Enumeration.EnumObj(cellString, (cell.Tag as Enumeration.EnumObj).GUID);
                    else
                        return null;
                }
                else
                    return cell.Tag as Enumeration.EnumObj;
            };

            var newValues = cells.Select(GetEnumObj).Where(a => a != null).Evaluate();
            return newValues;
        }

        private ToStringWrapper<TempEnumeration> SelectedEnumeration
        {
            get { return comboBox1.SelectedItem as ToStringWrapper<TempEnumeration>; }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveUpdate();
            foreach (var update in m_updates)
                update();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
