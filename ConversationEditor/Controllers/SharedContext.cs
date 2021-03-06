﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.IO;
using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGui>;
using Conversation;

namespace ConversationEditor
{
    public interface ILocalizationContext
    {
        NotifierProperty<Project.TData.LocalizerSetData> CurrentLocalization { get; }
    }

    /// <summary>
    /// Context of the editor shared by several controls
    /// </summary>
    public class SharedContext : ILocalizationContext
    {
        public NotifierProperty<IProject> CurrentProject { get; } = new NotifierProperty<IProject>(DummyProject.Instance);
        public event Action<Changed<FileInfo>> ProjectMoved;
        public NotifierProperty<Project.TData.LocalizerSetData> CurrentLocalization { get; } = new NotifierProperty<Project.TData.LocalizerSetData>(Project.TData.LocalizerSetData.Empty);

        public ErrorCheckerUtils<ConversationNode> ErrorCheckerUtils()
        {
            //TODO: Should have a concept of current conversation so we don't have to search the whole project
            Func<IConversationNodeData, ConversationNode> reverseLookup = f => CurrentProject.Value.Conversations.SelectMany(c => c.Nodes).Where(n => n.Data == f).First();
            return new ErrorCheckerUtils<ConversationNode>(CurrentProject.Value.ConversationDataSource, reverseLookup);
        }

        private void OnProjectMoved(Changed<FileInfo> change)
        {
            ProjectMoved.Execute(change);
        }

        public SharedContext()
        {
            CurrentProject.Changed.Register(change => { change.From.File.Moved -= OnProjectMoved; change.To.File.Moved += OnProjectMoved; });
        }
    }
}
