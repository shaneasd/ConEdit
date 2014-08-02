using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;
using Conversation.Serialization;

namespace ConversationEditor
{
    using ConversationNode = ConversationNode<INodeGUI>;
    using TData = XmlGraphData<NodeUIData, ConversationEditorData>;

    public class ConversationFile : GraphFile, IConversationFile
    {
        private ISerializer<TData> m_serializer;

        SaveableFileUndoable m_file;
        public override ISaveableFileUndoable UndoableFile { get { return m_file; } }

        public ConversationFile(IEnumerable<GraphAndUI<NodeUIData>> nodes, List<NodeGroup> groups, FileInfo file, ISerializer<TData> serializer, List<Error> errors, INodeFactory<ConversationNode> nodeFactory)
            : base(nodes, groups, errors, nodeFactory)
        {
            m_file = new SaveableFileUndoable(file, SaveTo);
            m_serializer = serializer;
        }

        private void SaveTo(Stream file)
        {
            file.Position = 0;
            m_serializer.Write(SerializationUtils.MakeConversationData(Nodes, new ConversationEditorData() { Groups = Groups }), file);
        }

        public static FileInfo GetAvailableConversationPath(DirectoryInfo directory, IEnumerable<ISaveableFileProvider> projectFiles, Func<FileInfo, bool> pathOk)
        {
            //Create a stream under an available filename
            for (int i = 0; ; i++)
            {
                FileInfo path = new FileInfo(directory.FullName + Path.DirectorySeparatorChar + "New Conversation " + i + ".xml");
                if (!path.Exists && !projectFiles.Any(f => f.File.File.FullName == path.FullName))
                    return path;
            }
        }

        public static ConversationFile CreateEmpty(DirectoryInfo directory, Project project, Func<FileInfo, bool> pathOk, INodeFactory<ConversationNode> nodeFactory)
        {
            var file = GetAvailableConversationPath(directory, project.Elements, pathOk);

            var nodes = Enumerable.Empty<GraphAndUI<NodeUIData>>();
            var groups = new List<NodeGroup>();

            //Fill the stream with the essential content
            using (FileStream stream = Util.LoadFileStream(file, FileMode.CreateNew))
            {
                project.ConversationSerializer.Write(SerializationUtils.MakeConversationData(nodes, new ConversationEditorData { Groups = groups }), stream);
            }

            return new ConversationFile(nodes, groups, file, project.ConversationSerializer, new List<Error>(), nodeFactory);
        }

        /// <exception cref="FileLoadException">If file can't be read</exception>
        public static ConversationFile Load(FileInfo file, IDataSource datasource, INodeFactory nodeFactory, ISerializerDeserializer<TData> serializer)
        {
            TData data;
            using (var stream = Util.LoadFileStream(file, FileMode.Open, FileAccess.Read))
            {
                data = serializer.Read(stream);
            }
            return new ConversationFile(data.Nodes.ToList(), data.EditorData.Groups.ToList(), file, serializer, data.Errors, nodeFactory);
        }

        public void Removed()
        {
            //Doesn't care
        }
    }
}
