using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using System.Reflection;
using ConversationEditor.DataSources;
using Utilities;

namespace ConversationEditor
{
    public class DataSourceController
    {
        IDataSource m_dataSource = new DummyDataSource();
        public IDataSource DataSource { get { return m_dataSource; } }
        public List<string> DataSourcePaths
        {
            get
            {
                return m_dataSource.Paths;
            }
        }

        XmlDataSource m_xmlDataSource = new XmlDataSource();

        public DataSourceController(ConfigParameter<List<string>> config)
        {
            m_config = config;
            LoadFromXml(m_config.Value);
        }

        public bool LoadFromXml(List<string> paths)
        {
            var newDataSource = m_xmlDataSource.Load(paths);
            if (newDataSource != null)
            {
                m_dataSource = newDataSource;
                DatasourceChanged.Execute();
                m_config.Value = DataSourcePaths;
                return true;
            }
            return false;
        }

        public event Action DatasourceChanged;
        private ConfigParameter<List<string>> m_config;
    }
}
