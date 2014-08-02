using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Utilities;

namespace ConversationEditor
{
    public class DomainFile : ISaveableFile
    {
        private FileStream m_file;
        public DomainFile(FileStream file)
        {
            m_file = file;
        }

        public event Action Modified;
        public event Action<FileInfo, FileInfo> Moved;

        public FileInfo File
        {
            get { return new FileInfo(m_file.Name); }
        }

        public Stream Stream
        {
            get { return m_file; }
        }

        public bool Save()
        {
            SaveTo(m_file);
            return true;
        }

        public bool SaveAs(FileStream newFile)
        {
            throw new NotImplementedException();
        }

        private void SaveTo(FileStream newFile)
        {
            m_file.Position = 0;
            m_file.SetLength(0);
            XmlDataSource.WriteDomain(this.Data, m_file);
        }

        public void Move(FileInfo path, Func<bool> replace)
        {
            var oldFile = File;
            if (path.Exists)
                if (replace())
                    path.Delete();
                else
                    return;
            FileStream newFile = path.Open(FileMode.CreateNew, FileAccess.ReadWrite);
            m_file.Position = 0;
            m_file.CopyTo(newFile);
            newFile.Flush();
            m_file.Close();
            File.Delete();
            m_file = newFile;
            Moved.Execute(oldFile, File);
        }

        public bool Changed
        {
            get;
            private set;
        }

        public bool CanClose()
        {
            return true;
        }

        public void ForceClose()
        {
            Stream.Close();
        }

        internal static DomainFile CreateEmpty(DirectoryInfo directory)
        {
            //Create a stream under an available filename
            FileStream file = null;
            for (int i = 0; file == null; i++)
            {
                FileInfo path = new FileInfo(directory.FullName + Path.DirectorySeparatorChar + "New Domain " + i + ".xml");
                if (!path.Exists)
                    file = path.Create();
            }

            XmlDataSource.WriteEmptyDomain(file);

            return new DomainFile(file);
        }

        public void Modify(List<NodeTypeData> nodeTypes, List<DynamicEnumerationData> dynamicEnumerations, List<EnumerationData> enumerations, List<DecimalData> decimals, List<IntegerData> integers, List<NodeData> nodes)
        {
            Data.NodeTypes = nodeTypes;
            Data.DynamicEnumerations = dynamicEnumerations;
            Data.Enumerations = enumerations;
            Data.Decimals = decimals;
            Data.Integers = integers;
            Data.Nodes = nodes;

            Changed = true;
            Modified.Execute();
        }

        public DomainData Data = new DomainData();
    }
}
