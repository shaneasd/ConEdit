using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using Viking.Nodes;
using Viking.Nodes.AI_Barks;
using Viking.Nodes.Audio;
using Viking.Nodes.Character_Sheets;
using Viking.Nodes.Character_Sheets.Character_Appearance;
using Viking.Nodes.Combat;
using Viking.Nodes.Condition;
using Viking.Nodes.Dev;
using Viking.Nodes.Follower;
using Viking.Nodes.Follower.Check_Follower;
using Viking.Nodes.Follower.Edit_Follower;
using Viking.Nodes.Follower.Filter_Conditions;
using Viking.Nodes.Items;
using Viking.Nodes.Items.Crafting;
using Viking.Nodes.Items.Special_Properties;
using Viking.Nodes.Jumps;
using Viking.Nodes.Metadata;
using Viking.Nodes.Option__;
using Viking.Nodes.Option__.Display_Condition;
using Viking.Nodes.Popup;
using Viking.Nodes.Randomise;
using Viking.Nodes.Trigger;
using Viking.Nodes.Trigger.Cinematic;
using Viking.Nodes.Trigger.Quests;

namespace RuntimeConversation
{
    public class NextNodeOption
    {
        public string Text { get; set; }
        public Viking.Nodes.Node Node { get; set; }

        public static IEnumerable<NextNodeOption> FromConnector(Viking.Nodes.Connectors.Connector output, Func<Id<LocalizedText>, string> localizer)
        {
            var nodes = output.Connections.Select(c => c.Parent);

            var count = nodes.Count();
            if (count == 0)
                throw new InvalidOperationException("Missing Terminator");
            else if (count == 1)
                return new[] { new NextNodeOption { Text = null, Node = output.Connections.Single().Parent } };
            else
            {
                if (nodes.Any(n => !(n is Viking.Nodes.Option__.Option_)))
                    throw new InvalidOperationException("Multiple non-option children");
                var options = nodes.OfType<Viking.Nodes.Option__.Option_>();
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

        public Action EnteringDialog { get; set; }
        public Action ExitingDialog { get; set; }
        public Action<string, string> PlaySpeech { get; set; }
        public Action<Utilities.ReadonlySet<Viking.Types.Personality_Trait>> Approve { get; set; }
        public Action<Utilities.ReadonlySet<Viking.Types.Personality_Trait>> Disapprove { get; set; }

        public IEnumerable<NextNodeOption> ProcessNode(Viking.Nodes.NPC_Speech node)
        {
            PlaySpeech(characterName(node.Speaker), node.Speech.Localized(localizer));
            return NextNodeOption.FromConnector(node.id179fd9edc5654fb2bf3ebc562c27c940, localizer);
        }

        public IEnumerable<NextNodeOption> ProcessNode(Viking.Nodes.Option__.Option_ node)
        {
            PlaySpeech("", node.Choice_Text.Localized(localizer));
            //if (node.Approve.Any())
            //    Approve(node.Approve);
            //if (node.Disapprove.Any())
            //    Disapprove(node.Disapprove);
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

        public IEnumerable<NextNodeOption> ProcessNode(Stored_NPC_Speech node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Description node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Injured_Hirdsman_Count node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Follower_Count node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Item_Type_Equipped node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Check_Trait node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Homestead_Upgrade_Level node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Character_Hitpoints node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Item_Equipped node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Player_Inventory node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Time_of_Day node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Encounter_Incapacitated_Hirdsmen node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Encounter_Survivors node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Encounter_Outcome node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Check_Resource node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Check_Local_Integer node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Check_Local_Boolean node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Check_Quest_Goal node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Character_Following node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Character_Status node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Check_Skill node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Check_Primary_Stat node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Check_Terrain_Type node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Check_Boolean node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Check_Integer node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Character_Health node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Player_Sex node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Check_Shift node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Relay node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Comment node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(TODO node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Error node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Jump node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Jump_Target node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Return_Jump_Target node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Return_Jump node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Return node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(RandomEvent_Info node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Conversation_Info node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Character_Definition node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Trade_Info node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Check_Injured_Hirdsmen node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Check_Equipped node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Check_Inventory node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Check_Homestead node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Check_Resource_ node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Check_Character_Following node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Check_Local_Boolean_ node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Check_Local_Integer_ node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Check_Follower_Count node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Check_Goal node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Check_Integer_ node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Check_Boolean_ node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Check_Gender node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Check_Skill_ node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Stat_Check node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Viking.Nodes.Randomise.Random node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Probability node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(AI_On_Spot_Trap node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(AI_On_Panicked node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(AI_On_Begin_Combat node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(AI_On_Select node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(AI_On_Incapacitated node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(AI_On_Do_Crit node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(AI_On_Take_Crit node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Character_Replace_Player node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Injure_Character node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Heal_All_Hirdsmen node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Give_Randomised_Item node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Increase_Reputation node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Decrease_Reputation node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Increase_Morale_by_Personality_Trait node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Decrease_Morale_by_Personality_Trait node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Remove_Status_Effect node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Discover_Location node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Equip_Item node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Edit_Resources node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Give_Status_Effect node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Decrease_Morale_All node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Increase_Morale_All node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Increase_Morale node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Advance_Story_Time node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Set_Story_Time node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Set_Weather node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Add_Target_to_Multi_Goal node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Character_Join_Party node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Character_Toggle_Follow node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Increment_Local_Integer node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Decrease_Morale node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Character_Leave_Scene node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Close_Goal node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Add_Goal node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Add_Quest node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Multi_Goal_Target node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Add_Multi_Goal node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Close_Multi_Goal_Target node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Open_Trade node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Load_Scene node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Set_Local_Boolean node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Set_Local_Integer node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Edit_Karma node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Remove_Item node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Award_XP node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Change_Alliance node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Increment_Integer node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Set_Boolean node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Set_Integer node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Give_Item node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Filter_Huscarls node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Filter_by_Item_Type node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Check_Equipped_ node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Check_Name node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Kill_Follower node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Increase_Morale_ node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Has_Trait node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Decrease_Morale_ node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Injure_Follower node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Clear_Stored_Follower node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Check_Injury node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Filter_by_Injury node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Is_Follower_Stored node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Filter_by_Name node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Check_Skill__ node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Prompt_Follower_Selection node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Gender_ node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Status_ node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Filter_by_Skill node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Filter_by_Gender node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Filter_by_Trait node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Retrieve_Stored_Follower node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Store_Follower_Globally node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Select_Random_Follower node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Check_Stat node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Filter_by_Item node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Input_Box node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Confirmation_Box node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Description_Box node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Initiative node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Victory_Condition_ node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Activate_Encounter node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Battle_Behaviour_ node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Armour_ node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Default_Armour node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Sling node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Helmet node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Talisman_Property__ node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Crafting_Cost node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Weapon_Special_Property node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Armour_Special_Property node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Talisman_ node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Misc_Item node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Axe node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Trap_Item node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Consumable_Item node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Throwable_Item node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Shield node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Bow node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Knife node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Dane_Axe node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Spear node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Sword node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Shane node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Audio_Event node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Major_Choice_Cue node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Character_Custom_Colours node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Random_Configuration node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Mesh_Configuration node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Character_Info node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Appearance node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Camera_Follow_Character node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Wait_For_Continue_Event node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Wait_For_Character_Spawn node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Character_Move_To_Character node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Party_Move_To_Waypoint node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Stored_NPC_Eliminate_Stored_NPC node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Stored_NPC_Eliminate_Character node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Eliminate_Character node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Eliminate_Stored_NPC node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(AI_Group_Leave_Scene node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(AI_Group_Assume_Positions node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(AI_Group_Move_To_Waypoint node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Fade_In node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Fade_Out node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Camera_Effect node)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<NextNodeOption> ProcessNode(Character_Move_To_Waypoint node)
        {
            throw new NotImplementedException();
        }
    }
}
