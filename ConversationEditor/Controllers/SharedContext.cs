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
    internal interface ILocalizationContext
    {
        NotifierProperty<ILocalizationFile> CurrentLocalization { get; }
    }

    /// <summary>
    /// Context of the editor shared by several controls
    /// </summary>
    internal class SharedContext : ILocalizationContext
    {
        public NotifierProperty<IProject> CurrentProject = new NotifierProperty<IProject>(DummyProject.Instance);
        public event Action<Changed<FileInfo>> ProjectMoved;
        public NotifierProperty<ILocalizationFile> CurrentLocalization { get { return m_currentLocalization; } }
        private readonly NotifierProperty<ILocalizationFile> m_currentLocalization = new NotifierProperty<ILocalizationFile>(DummyLocalizationFile.Instance);

        public ErrorCheckerUtils<ConversationNode> ErrorCheckerUtils()
        {
            //TODO: Should have a concept of current conversation so we don't have to search the whole project
            Func<IEditable, ConversationNode> reverseLookup = f => CurrentProject.Value.Conversations.SelectMany(c => c.Nodes).Where(n => n.m_data == f).First();
            return new ErrorCheckerUtils<ConversationNode>(CurrentProject.Value.ConversationDataSource, reverseLookup);
        }

        private void OnProjectMoved(Changed<FileInfo> change)
        {
            ProjectMoved.Execute(change);
        }

        public SharedContext()
        {
            CurrentProject.Changed.Register(change => { change.from.File.Moved -= OnProjectMoved; change.to.File.Moved += OnProjectMoved; });
        }
    }
}