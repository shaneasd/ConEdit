using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace ConversationEditor
{
    class SpecialConnectors
    {
        public static readonly ConnectorDefinitionData Input = new ConnectorDefinitionData("Input", ConnectorDefinitionData.INPUT_DEFINITION_ID, new List<NodeData.ParameterData>(), ConnectorPosition.Top);
        public static readonly ConnectorDefinitionData Output = new ConnectorDefinitionData("Output", ConnectorDefinitionData.OUTPUT_DEFINITION_ID,
            new List<NodeData.ParameterData>() { new NodeData.ParameterData("Name", ConnectorDefinitionData.OUTPUT_NAME, BaseTypeString.PARAMETER_TYPE) }
            , ConnectorPosition.Bottom);

    }
}
