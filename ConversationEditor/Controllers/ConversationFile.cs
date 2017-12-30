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
    using System.Windows;

    public class ConversationFile : GraphFile, IConversationFile
    {
        private ISerializer<TData> m_serializer;

        SaveableFileUndoable m_file;
        public override ISaveableFileUndoable UndoableFile => m_file;

        public Id<FileInProject> Id { get; }

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
        public ConversationFile(Id<FileInProject> id, IEnumerable<GraphAndUI<NodeUIData>> nodes, IEnumerable<NodeGroup> groups, MemoryStream rawData, DocumentPath file, ISerializer<TData> serializer,
            ReadOnlyCollection<LoadError> errors, INodeFactory nodeFactory, GenerateAudio generateAudio,
            Func<IDynamicEnumParameter, object, DynamicEnumParameter.Source> getDocumentSource, IAudioLibrary audioProvider, UpToDateFile.BackEnd backEnd)
            : base(nodes, groups, errors, nodeFactory, generateAudio, getDocumentSource, audioProvider)
        {
            Id = id;
            m_file = new SaveableFileUndoable(rawData, file.FileInfo, SaveTo, backEnd);
            m_serializer = serializer;

            foreach (var node in m_nodes)
            {
                //TODO: AUDIO: Why are we automatically decorrupting specifically audio parameters?
                var audios = node.Data.Parameters.OfType<IAudioParameter>();
                foreach (var aud in audios)
                    if (aud.Corrupted)
                    {
                        var val = generateAudio(this);
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

        public static ConversationFile CreateEmpty(DirectoryInfo directory, Project project, INodeFactory nodeFactory,
            GenerateAudio generateAudio, Func<IDynamicEnumParameter, object, DynamicEnumParameter.Source> getDocumentSource, IAudioLibrary audioProvider, UpToDateFile.BackEnd backEnd, DirectoryInfo origin)
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

                var result = new ConversationFile(Id<FileInProject>.New(), nodes, groups, m, DocumentPath.FromPath(file, origin), project.ConversationSerializer, new ReadOnlyCollection<LoadError>(new LoadError[0]), nodeFactory, generateAudio, getDocumentSource, audioProvider, backEnd);
                result.m_file.Save(); //Make sure the file starts life as a valid xml document
                return result;
            }
        }

        public static IConversationFile Load(Id<FileInProject> file, DocumentPath path, INodeFactory nodeFactory, ISerializerDeserializer<TData> serializer, GenerateAudio generateAudio,
            Func<IDynamicEnumParameter, object, DynamicEnumParameter.Source> getDocumentSource, IAudioLibrary audioProvider, UpToDateFile.BackEnd backEnd)
        {
            try
            {
                using (var stream = Util.LoadFileStream(path.FileInfo, FileMode.Open, FileAccess.Read))
                {
                    using (MemoryStream m = new MemoryStream((int)stream.Length))
                    {
                        stream.CopyTo(m);
                        stream.Dispose();
                        m.Position = 0;
                        TData data = serializer.Read(m);
                        return new ConversationFile(file, data.Nodes.ToList(), data.EditorData.Groups.ToList(), m, path, serializer, data.Errors, nodeFactory, generateAudio, getDocumentSource, audioProvider, backEnd);
                    }
                }
            }
            catch (MyFileLoadException e)
            {
                Console.Out.WriteLine(e.Message);
                Console.Out.WriteLine(e.StackTrace);
                Console.Out.WriteLine(e.InnerException.Message);
                Console.Out.WriteLine(e.InnerException.StackTrace);

                MessageBox.Show("File: " + path.AbsolutePath + " could not be accessed");
                return new MissingConversationFile(file, path);
            }
            catch (DeserializerVersionMismatchException e)
            {
                MessageBox.Show("File: " + path.AbsolutePath + " could not be processed. " + e.Message);
                return new MissingConversationFile(file, path);
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
