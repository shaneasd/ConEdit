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
    using ConversationNode = ConversationNode<INodeGui>;
    using TData = XmlGraphData<NodeUIData, ConversationEditorData>;
    using System.Collections.ObjectModel;

    internal class ConversationFile : GraphFile, IConversationFile
    {
        private ISerializer<TData> m_serializer;

        SaveableFileUndoable m_file;
        public override ISaveableFileUndoable UndoableFile { get { return m_file; } }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="groups"></param>
        /// <param name="rawData">Represents the current contents of the file. Reference is not held. A copy is made.</param>
        /// <param name="file"></param>
        /// <param name="serializer"></param>
        /// <param name="errors"></param>
        /// <param name="nodeFactory"></param>
        /// <param name="generateAudio"></param>
        /// <param name="getDocumentSource"></param>
        /// <param name="audioProvider"></param>
        public ConversationFile(IEnumerable<GraphAndUI<NodeUIData>> nodes, List<NodeGroup> groups, MemoryStream rawData, FileInfo file, ISerializer<TData> serializer,
            ReadOnlyCollection<LoadError> errors, INodeFactory<ConversationNode> nodeFactory, Func<ISaveableFileProvider, IEnumerable<IParameter>, Audio> generateAudio,
            Func<IDynamicEnumParameter, object, DynamicEnumParameter.Source> getDocumentSource, IAudioLibrary audioProvider)
            : base(nodes, groups, errors, nodeFactory, generateAudio, getDocumentSource, audioProvider)
        {
            m_file = new SaveableFileUndoable(rawData, file, SaveTo);
            m_serializer = serializer;

            foreach (var node in m_nodes)
            {
                var audios = node.Parameters.OfType<IAudioParameter>();
                foreach (var aud in audios)
                    if (aud.Corrupted)
                    {
                        var val = generateAudio(this, node.Parameters);
                        aud.SetValueAction(val).Value.Redo();
                        m_file.ChangeNoUndo();
                        audioProvider.UpdateUsage(val);
                    }
                node.UpdateRendererCorruption();
            }
        }

        private void SaveTo(Stream file)
        {
            file.Position = 0;
            m_serializer.Write(SerializationUtils.MakeConversationData(Nodes, new ConversationEditorData(Groups)), file);
        }

        public static FileInfo GetAvailableConversationPath(DirectoryInfo directory, IEnumerable<ISaveableFileProvider> projectFiles)
        {
            //Create a stream under an available filename
            for (int i = 0; ; i++)
            {
                FileInfo path = new FileInfo(directory.FullName + Path.DirectorySeparatorChar + "New Conversation " + i + ".xml");
                if (!path.Exists && !projectFiles.Any(f => f.File.File.FullName == path.FullName))
                    return path;
            }
        }

        public static ConversationFile CreateEmpty(DirectoryInfo directory, Project project, INodeFactory<ConversationNode> nodeFactory,
            Func<ISaveableFileProvider, IEnumerable<IParameter>, Audio> generateAudio, Func<IDynamicEnumParameter, object, DynamicEnumParameter.Source> getDocumentSource, IAudioLibrary audioProvider)
        {
            var file = GetAvailableConversationPath(directory, project.Elements);

            var nodes = Enumerable.Empty<GraphAndUI<NodeUIData>>();
            var groups = new List<NodeGroup>();

            //Fill the stream with the essential content
            using (MemoryStream m = new MemoryStream())
            {
                using (FileStream stream = Util.LoadFileStream(file, FileMode.CreateNew, FileAccess.Write))
                {
                    project.ConversationSerializer.Write(SerializationUtils.MakeConversationData(nodes, new ConversationEditorData(groups)), m);
                    m.Position = 0;
                    m.CopyTo(stream);
                }

                return new ConversationFile(nodes, groups, m, file, project.ConversationSerializer, new ReadOnlyCollection<LoadError>(new LoadError[0]), nodeFactory, generateAudio, getDocumentSource, audioProvider);
            }
        }

        /// <exception cref="MyFileLoadException">If file can't be read</exception>
        public static ConversationFile Load(FileInfo file, INodeFactory nodeFactory, ISerializerDeserializer<TData> serializer, Func<ISaveableFileProvider, IEnumerable<IParameter>, Audio> generateAudio,
            Func<IDynamicEnumParameter, object, DynamicEnumParameter.Source> getDocumentSource, IAudioLibrary audioProvider)
        {
            using (var stream = Util.LoadFileStream(file, FileMode.Open, FileAccess.Read))
            {
                using (MemoryStream m = new MemoryStream((int)stream.Length))
                {
                    stream.CopyTo(m);
                    stream.Dispose();
                    m.Position = 0;
                    TData data = serializer.Read(m);
                    return new ConversationFile(data.Nodes.ToList(), data.EditorData.Groups.ToList(), m, file, serializer, data.Errors, nodeFactory, generateAudio, getDocumentSource, audioProvider);
                }
            }
        }

        public bool CanRemove(Func<bool> prompt)
        {
            //Doesn't care
            return true;
        }

        public void Removed()
        {
            //Do nothing
        }
    }
}
