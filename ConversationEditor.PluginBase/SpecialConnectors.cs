using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using System.Collections.ObjectModel;

namespace ConversationEditor
{
    public static class SpecialConnectors
    {
        public static readonly ConnectorDefinitionData Input = new ConnectorDefinitionData("Input", ConnectorDefinitionData.InputDefinitionId, new List<NodeData.ParameterData>(), ConnectorPosition.Top);
        public static readonly ConnectorDefinitionData Output = new ConnectorDefinitionData("Output", ConnectorDefinitionData.OutputDefinitionId,
            new List<NodeData.ParameterData>() { new NodeData.ParameterData("Name", ConnectorDefinitionData.OutputName, StringParameter.ParameterType, new ReadOnlyCollection<NodeData.ConfigData>(new NodeData.ConfigData[0])) }
            , ConnectorPosition.Bottom);

    }
}
