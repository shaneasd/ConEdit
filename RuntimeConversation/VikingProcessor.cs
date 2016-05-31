using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace RuntimeConversation
{
    public class NextNodeOption
    {
        public string Text;
        public Viking.Nodes.Node Node;

        public static IEnumerable<NextNodeOption> FromConnector(Viking.Nodes.Connectors.Output output, Func<Id<LocalizedText>, string> localizer)
        {
            var nodes = output.Connections.Select(c => c.Parent);

            var count = nodes.Count();
            if (count == 0)
                throw new Exception("Missing Terminator");
            else if (count == 1)
                return new[] { new NextNodeOption { Text = null, Node = output.Connections.Single().Parent } };
            else
            {
                if (nodes.Any(n => !(n is Viking.Nodes.Option)))
                    throw new Exception("Multiple non-option children");
                var options = nodes.OfType<Viking.Nodes.Option>();
                return options.Select(o => new NextNodeOption { Text = o.Choice_Text.Localized(localizer), Node = o }).ToArray();
            }
        }
    }

    public class VikingProcessor : Viking.IProcessor<IEnumerable<NextNodeOption>>
    {
        Func<Id<LocalizedText>, string> localizer;
        Func<Viking.Types.Character, string> characterName;
        public VikingProcessor(Func<Id<LocalizedText>, string> localizer, Func<Viking.Types.Character, string> characterName)
        {
            this.localizer = localizer;
            this.characterName = characterName;
        }

        public event Action EnteringDialog;
        public event Action ExitingDialog;
        public event Action<string, string, string> PlaySpeech;
        public event Action<Utilities.ReadonlySet<Viking.Types.Personality_Trait>> Approve;
        public event Action<Utilities.ReadonlySet<Viking.Types.Personality_Trait>> Disapprove;

        public IEnumerable<NextNodeOption> ProcessNode(Viking.Nodes.NPC_Speech node)
        {
            PlaySpeech(characterName(node.Speaker), characterName(node.Listener), node.Speech.Localized(localizer));
            return NextNodeOption.FromConnector(node.id179fd9edc5654fb2bf3ebc562c27c940, localizer);
        }

        public IEnumerable<NextNodeOption> ProcessNode(Viking.Nodes.Option node)
        {
            PlaySpeech("", "", node.Choice_Text.Localized(localizer));
            if (node.Approve.Any())
                Approve(node.Approve);
            if (node.Disapprove.Any())
                Disapprove(node.Disapprove);
            return NextNodeOption.FromConnector(node.id2fdfacb3fdb44c4f99a62fa7bc341c79, localizer);
        }

        public IEnumerable<NextNodeOption> ProcessNode(Viking.Nodes.Trigger.Exit_Dialogue node)
        {
            ExitingDialog();
            return NextNodeOption.FromConnector(node.id3dee23b1b1a04457b2f736fdfe67cdf2, localizer);
        }

        public IEnumerable<NextNodeOption> ProcessNode(Viking.Nodes.Trigger.Enter_Dialogue node)
        {
            EnteringDialog();
            return NextNodeOption.FromConnector(node.id3dee23b1b1a04457b2f736fdfe67cdf2, localizer);
        }

        public IEnumerable<NextNodeOption> ProcessNode(Viking.Nodes.Start node)
        {
            return NextNodeOption.FromConnector(node.idb5b1fe0305e14058aecb4012ae91db1f, localizer);
        }

        public IEnumerable<NextNodeOption> ProcessNode(Viking.Nodes.Terminator node)
        {
            yield return new NextNodeOption { Text = "", Node = null };
        }

        public IEnumerable<NextNodeOption> ProcessNode(Viking.Nodes.Condition.Character_Alive node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Viking.Nodes.Condition.Check_Boolean node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Viking.Nodes.Condition.Check_Integer node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Viking.Nodes.Condition.Character_Health node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Viking.Nodes.Condition.Player_Sex node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Viking.Nodes.Condition.Player_Inventory node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Viking.Nodes.Dev.TODO node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Viking.Nodes.Dev.Error node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Viking.Nodes.Dev.Description node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Viking.Nodes.Jumps.Jump_To node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Viking.Nodes.Jumps.Jump_Target node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Viking.Nodes.Metadata.Conversation_Info node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Viking.Nodes.Randomise.Random node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Viking.Nodes.Randomise.Probability node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Viking.Nodes.Trigger.Increment_Integer node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Viking.Nodes.Trigger.Set_Boolean node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Viking.Nodes.Trigger.Set_Integer node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Viking.Nodes.Trigger.Give_Item node)
        {
            throw new NotImplementedException();
        }
    }
}
