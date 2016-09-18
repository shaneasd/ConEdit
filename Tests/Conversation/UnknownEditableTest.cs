using Conversation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Tests.Conversation
{
    public static class UnknownEditableTest
    {
        [Test]
        public static void TestAllowConnection()
        {
            //UnknownEditable e;
            //e.AllowConnection;
            //I'm a bit skeptical about the design of this
            //See TODOs on AllowConnection and XmlConversation<TUIRawData, TEditorData>.Deserializer.Read
            Assert.Inconclusive();
        }

        [Test]
        public static void Test()
        {
            var parameters = new UnknownParameter[]
            {
                new UnknownParameter(Id<Parameter>.Parse("96a38b68-de80-4bf4-a0f6-59d9e205ee94"), "asd"),
                new UnknownParameter(Id<Parameter>.Parse("f42bd1a5-520c-46e0-bf0d-5c9504597d88"), "dsa"),
            };
            var id = Id<NodeTemp>.Parse("a9044c22-2860-446d-98bb-7aa10ee7049b");
            var typeId = Id<NodeTypeTemp>.Parse("4ac17752-0cba-47a8-9d39-3e13473c2b45");
            UnknownEditable e = new UnknownEditable(id, typeId, parameters);

            Assert.That(e.Name, Is.EqualTo("Unknown Node"));
            Assert.That(e.NodeTypeId, Is.EqualTo(typeId));
            Assert.That(e.NodeId, Is.EqualTo(id));
            Assert.That(e.Parameters, Is.EqualTo(parameters));
            Assert.That(e.Config, Is.Empty);
            Assert.That(e.Connectors, Is.Empty);

            id = Id<NodeTemp>.Parse("2ccf5949-13f7-440b-8b05-e3441fa55036");
            e.ChangeId(id);
            Assert.That(e.NodeId, Is.EqualTo(id));

            //We can't remove parameters from a node we don't understand at all.
            //Likely either the whole node is lost to us in which case we should delete the whole node
            //or we can recover the definition in which case the unknown parameters would no longer be unknown
            Assert.That(() => e.RemoveUnknownParameter(parameters[0]), Throws.TypeOf<NotSupportedException>());

            var connectorId0 = Id<TConnector>.Parse("7730fc31-1c37-44b9-9fc2-d431a0f2d8b5");
            e.AddConnector(connectorId0);
            Assert.That(e.Connectors.Count(), Is.EqualTo(1));
            Assert.That(e.Connectors.ElementAt(0).Id, Is.EqualTo(connectorId0));

            var e2 = new UnknownEditable(Id<NodeTemp>.Parse("2747661a-6200-4252-a3bd-93f8f52a5615"), typeId, parameters);
            e2.AddConnector(connectorId0);

            int linked = 0;
            e.Linked += () => linked++;
            e.Connectors.First().ConnectTo(e2.Connectors.First(), true);
            Assert.That(linked, Is.EqualTo(1));
        }
    }
}
