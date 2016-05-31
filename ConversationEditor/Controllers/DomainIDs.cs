using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace ConversationEditor
{
    internal static class DomainIDs
    {
        public static readonly Id<NodeTypeTemp> CategoryGuid = Id<NodeTypeTemp>.Parse("88cb9861-9a78-4da5-bd9d-88840ee2085c");
        public static readonly Id<Parameter> CategoryName = Id<Parameter>.Parse("a55c1343-1e54-4b84-a6ba-a0ecd31d7839");
        public static readonly Id<Parameter> CategoryParent = Id<Parameter>.Parse("54c1256d-8229-432f-a2ba-2897abb71c67");

        public static readonly Id<Parameter> IntegerName = Id<Parameter>.Parse("f113eb22-59d0-4929-ae36-d5a4d7150dfa");
        public static readonly Id<Parameter> IntegerMax = Id<Parameter>.Parse("22567012-b580-460e-911c-a9a4f686f897");
        public static readonly Id<Parameter> IntegerMin = Id<Parameter>.Parse("657a1c56-f23b-4727-b303-1b7822a7b66d");

        public static readonly Id<Parameter> DecimalName = Id<Parameter>.Parse("84c24890-7301-4bb8-b702-6dee963b25fc");
        public static readonly Id<Parameter> DecimalMax = Id<Parameter>.Parse("3ac38058-ebd9-4730-911f-1229c9b5e5c5");
        public static readonly Id<Parameter> DecimalMin = Id<Parameter>.Parse("d39f103d-1f6f-4ed3-bdf8-e76923da5d96");

        public static readonly Id<Parameter> DynamicEnumName = Id<Parameter>.Parse("a3e258c8-5446-4272-80ba-a5b9417a46f6");
        public static readonly Id<Parameter> LocalDynamicEnumName = Id<Parameter>.Parse("fcbd0154-7865-4e24-ba50-cd06e65cc543");

        public static readonly Guid EnumerationMenu = Guid.Parse("648b52a7-0f26-4600-a580-5d84290d87bb");
        public static readonly Id<Parameter> EnumerationName = Id<Parameter>.Parse("65ff02e9-3474-4892-b54d-6abbbfadd667");
        public static readonly Id<NodeTypeTemp> EnumerationValueDeclaration = Id<NodeTypeTemp>.Parse("877fb888-c998-4bac-82ff-f801277ec93f");
        public static readonly Id<Parameter> EnumerationValueParameter = Id<Parameter>.Parse("4467ec81-1e43-40c6-9703-df31b629c680");

        public static readonly Guid NodeMenu = Guid.Parse("084ae578-f59b-4b42-96d1-3df9937014ba");

        public static readonly Id<Parameter> ParameterName = Id<Parameter>.Parse("c867cb04-e7d9-448b-aad2-dd7f2cb0a549");
        public static readonly Id<Parameter> PARAMETER_TYPE = Id<Parameter>.Parse("167a9bb2-116f-4894-b4d8-6b8973abf619"); //Guid of the parameter which identifies the specific subtype of integer/decimal/enumeration/dynamicenumeration/localdynamicenumeration
        public static readonly Id<Parameter> ParameterDefault = Id<Parameter>.Parse("f864db77-2467-46d0-b224-90c99ef91aa0");

        public static readonly Id<NodeTypeTemp> NodeGuid = Id<NodeTypeTemp>.Parse("36607dcf-bbd7-4563-851a-6473d2b408e1"); //Type id of node definition node
        public static readonly Id<Parameter> NodeName = Id<Parameter>.Parse("bb08a9ae-dc69-44e7-bef9-6c2fc8a615b2");
        public static readonly Id<Parameter> NodeCategory = Id<Parameter>.Parse("a4078e53-072c-4889-81bd-268f3a6a04f1"); //Parameter id of category parameter of node definition node

        public static readonly Id<NodeTypeTemp> ConnectorDefinitionGuid = Id<NodeTypeTemp>.Parse("844bbe9e-726f-4020-bd8c-c252a34103a0");
        public static readonly Id<Parameter> ConnectorDefinitionName = Id<Parameter>.Parse("9b33ce7c-40c3-4000-a0cc-a65f5186c17c");

        public static readonly Id<NodeTypeTemp> ConnectionDefinitionGuid = Id<NodeTypeTemp>.Parse("a530d784-7cea-4df6-b189-1bc1ae801bc0");
        public static readonly Id<Parameter> ConnectionDefinitionConnector1 = Id<Parameter>.Parse("928841b6-8e5a-4f45-a643-c6ea4449b1d2");
        public static readonly Id<Parameter> ConnectionDefinitionConnector2 = Id<Parameter>.Parse("63a9a8da-9086-4b67-beb0-1281760c2b9e");

        public static readonly Guid CategoryNone = Guid.Parse("a5142af5-3d14-4169-9638-df2e52e97963");

        public static readonly ParameterType CategoryType = ParameterType.Parse("5d823e77-52bc-46c0-8f47-3ddb1211fdcc");
        public static readonly ParameterType ConnectionType = ParameterType.Parse("36829fb5-0cdd-439d-930c-f9128022f9c2");

        public static Guid ConfigMenu = Guid.Parse("8d552ab9-bd06-4cf1-b671-f59455387009");

        public static readonly ConnectorDefinitionData ParameterOutputDefinition = new ConnectorDefinitionData("", Id<TConnectorDefinition>.Parse("4124e6cb-63f7-40fd-85b4-c41f080edbd7"), new List<NodeData.ParameterData>(), ConnectorPosition.Top, true);
        public static readonly ConnectorDefinitionData ParameterConfigConnectorDefinition = new ConnectorDefinitionData("", Id<TConnectorDefinition>.Parse("1551129e-a394-4411-8974-0c3dccab3ba8"), new List<NodeData.ParameterData>(), ConnectorPosition.Right, true);

        public static readonly ConnectorDefinitionData ConnectorOutputDefinition = new ConnectorDefinitionData("", Id<TConnectorDefinition>.Parse("a37842f0-52d9-406c-8ef7-56d1be90b7c5"), new List<NodeData.ParameterData>(), ConnectorPosition.Top, true);
        public static readonly ConnectorDefinitionData ConfigOutputDefinition = new ConnectorDefinitionData("", Id<TConnectorDefinition>.Parse("8f0329d7-65f4-4e76-bbf8-3999f8adae5f"), new List<NodeData.ParameterData>(), ConnectorPosition.Top, true);
        public static readonly ConnectorDefinitionData NodeOutputConfigDefinition = new ConnectorDefinitionData("", Id<TConnectorDefinition>.Parse("41352cc9-3228-418b-984e-c9825a35e7ad"), new List<NodeData.ParameterData>(), ConnectorPosition.Bottom, true);
        public static readonly ConnectorDefinitionData NodeOutputParametersDefinition = new ConnectorDefinitionData("", Id<TConnectorDefinition>.Parse("d5c63852-ba95-41bd-95fb-03e120168f13"), new List<NodeData.ParameterData>(), ConnectorPosition.Bottom, true);
        public static readonly ConnectorDefinitionData NodeOutputConnectorsDefinition = new ConnectorDefinitionData("", Id<TConnectorDefinition>.Parse("3825e67f-fd0f-44b6-a3bc-0e001ea3a519"), new List<NodeData.ParameterData>(), ConnectorPosition.Bottom, true);

        public static readonly ConnectorDefinitionData ConnectorDefiinitionOutputDefinition = new ConnectorDefinitionData("", Id<TConnectorDefinition>.Parse("d27006af-d400-4631-a67b-5bd61188c13d"), new List<NodeData.ParameterData>(), ConnectorPosition.Bottom, true);
        //public static readonly ConnectorDefinitionData CONNECTOR_DEFINITION_CONNECTION_DEFINITION = new ConnectorDefinitionData("", ID<TConnectorDefinition>.Parse("18c7e5b3-1486-44f3-b603-72d4240930de"), new List<NodeData.ParameterData>(), ConnectorPosition.Right, true);
        //public static readonly ConnectorDefinitionData CONNECTION_DEFINITION_CONNECTOR = new ConnectorDefinitionData("", ID<TConnectorDefinition>.Parse("7be72853-e334-4cb1-bc74-38ed747ddfb2"), new List<NodeData.ParameterData>(), ConnectorPosition.Right, true);

        public static readonly ConnectorDefinitionData EnumValueOutputDefinition = new ConnectorDefinitionData("", Id<TConnectorDefinition>.Parse("4fc9bf16-a7cb-4e4c-bd16-6084e6ebee7b"), new List<NodeData.ParameterData>(), ConnectorPosition.Top, true);
        public static readonly ConnectorDefinitionData EnumOutputDefinition = new ConnectorDefinitionData("", Id<TConnectorDefinition>.Parse("fcca13e8-149e-43e9-a2a8-03d3675e98b9"), new List<NodeData.ParameterData>(), ConnectorPosition.Bottom, true);


        public static class AutoComplete
        {
            public static readonly Guid Menu = Guid.Parse("3ced8738-4ff1-47a6-8392-d6bd4d11a1db");

            public static readonly ConnectorDefinitionData Next = new ConnectorDefinitionData("Next", Id<TConnectorDefinition>.Parse("ecf2eb22-7370-4d01-b2bc-f78b5da75da1"), new List<NodeData.ParameterData>(), ConnectorPosition.Right, true);
            public static readonly ConnectorDefinitionData Previous = new ConnectorDefinitionData("Previous", Id<TConnectorDefinition>.Parse("8ad99532-d5b6-4103-908d-536ac351abd8"), new List<NodeData.ParameterData>(), ConnectorPosition.Left, true);
            public static readonly ConnectorDefinitionData Child = new ConnectorDefinitionData("Child", Id<TConnectorDefinition>.Parse("e427b037-35a1-4617-81cc-165a3f8b1402"), new List<NodeData.ParameterData>(), ConnectorPosition.Bottom, true);
            public static readonly ConnectorDefinitionData Parent = new ConnectorDefinitionData("Parent", Id<TConnectorDefinition>.Parse("60d2fb0d-08b7-4bb1-823d-bb1322a6077f"), new List<NodeData.ParameterData>(), ConnectorPosition.Top, true);

            public static readonly NodeData.ConnectorData child = new NodeData.ConnectorData(Id<TConnector>.Parse("fc409936-c8b3-4591-a1e0-6469f0d7f485"), DomainIDs.AutoComplete.Child.Id, new List<Parameter>());
            public static readonly NodeData.ConnectorData parent = new NodeData.ConnectorData(Id<TConnector>.Parse("d3a27dcf-3344-49e1-8069-a30f2f22cc8f"), DomainIDs.AutoComplete.Parent.Id, new List<Parameter>());
            public static readonly NodeData.ConnectorData next = new NodeData.ConnectorData(Id<TConnector>.Parse("f45bc2bf-d3f2-4074-b5b8-f0c44a9540d9"), DomainIDs.AutoComplete.Next.Id, new List<Parameter>());
            public static readonly NodeData.ConnectorData previous = new NodeData.ConnectorData(Id<TConnector>.Parse("dd97fdd7-703a-4084-b987-4cdc29dc2f16"), DomainIDs.AutoComplete.Previous.Id, new List<Parameter>());

            public static readonly Id<NodeTypeTemp> ZeroOrMore = Id<NodeTypeTemp>.Parse("746f33a5-606f-4c13-90ec-8aabc044c180");
            public static readonly Id<NodeTypeTemp> OneOrMore = Id<NodeTypeTemp>.Parse("b5bedc8a-9a7e-456e-9092-fc7adc65717c");
            public static readonly Id<NodeTypeTemp> ZeroOrOne = Id<NodeTypeTemp>.Parse("e367eeab-3e88-4460-9979-08aff54c1f5d");
            public static readonly Id<NodeTypeTemp> ExactlyOne = Id<NodeTypeTemp>.Parse("638241c9-13d9-4f82-8d9b-6afe0378d394");
            public static readonly Id<NodeTypeTemp> String = Id<NodeTypeTemp>.Parse("3cc5503f-cfd1-4596-8913-1e293cdac3a9");
            public static readonly Id<NodeTypeTemp> EnumerationValue = Id<NodeTypeTemp>.Parse("cf2e2170-7e25-4062-b7c1-6eaa6f4261bc");
            public static readonly Id<NodeTypeTemp> DynamicEnumerationValue = Id<NodeTypeTemp>.Parse("94f22e43-55d1-41c2-805d-7c97e56a3d5b");
            public static readonly Id<NodeTypeTemp> LocalDynamicEnumerationValue = Id<NodeTypeTemp>.Parse("f4776511-b2a3-44d9-8df6-2ab3c566030a");
            public static readonly Id<NodeTypeTemp> Character = Id<NodeTypeTemp>.Parse("8e75d2aa-f408-46f9-bf1b-6be398b62ace");

            public static bool IsAutoCompleteNode(Id<NodeTypeTemp> node)
            {
                return new[] { ZeroOrMore, OneOrMore, ZeroOrOne, ExactlyOne, String, EnumerationValue, DynamicEnumerationValue, LocalDynamicEnumerationValue, Character }.Contains(node);
            }
        }
    }
}
