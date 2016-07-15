using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Conversation;
using Conversation.Serialization;
using System.Diagnostics;

namespace ConversationEditor
{
    internal interface IAutoCompletePattern
    {
        IEnumerable<string> AutoCompleteSuggestions(IParameter p, string s, Func<ParameterType, DynamicEnumParameter.Source> enumSource);
    }

    public static class AutoCompletePattern
    {
        private static bool HasParent(IEditable node)
        {
            var parentConnector = node.Connectors.Where(c => c.ID == DomainIDs.AutoComplete.parent.Id).SingleOrDefault();
            if (parentConnector == null)
                return false;
            else
                return parentConnector.Connections.Any();
        }

        private static bool HasPrevious(IEditable node)
        {
            var previousConnector = node.Connectors.Where(c => c.ID == DomainIDs.AutoComplete.previous.Id).SingleOrDefault();
            if (previousConnector == null)
                return false;
            else
                return previousConnector.Connections.Any();
        }

        private static IEnumerable<IEditable> Children(IEditable node)
        {
            var childConnector = node.Connectors.Where(c => c.ID == DomainIDs.AutoComplete.child.Id).SingleOrDefault();
            if (childConnector == null)
                return Enumerable.Empty<IEditable>();
            else
                return childConnector.Connections.Select(c => c.Parent);
        }

        private static IEnumerable<IEditable> Next(IEditable node)
        {
            var nextConnector = node.Connectors.Where(c => c.ID == DomainIDs.AutoComplete.next.Id).SingleOrDefault();
            if (nextConnector == null)
                return Enumerable.Empty<IEditable>();
            else
                return nextConnector.Connections.Select(c => c.Parent);
        }

        internal static IEnumerable<IAutoCompletePattern> Generate(XmlGraphData<NodeUIData, ConversationEditorData> nodeData, DomainDomain source)
        {
            List<IAutoCompletePattern> result = new List<IAutoCompletePattern>();
            try
            {
                //TODO: Before we start processing, make sure there are no cycles which would cause us to recurse forever

                var startNodes = nodeData.Nodes.Select(n => n.GraphData).Where(n => !HasParent(n) && !HasPrevious(n));
                foreach (var startNode in startNodes)
                {
                    result.Add(Process(startNode, source));
                }
            }
            catch //TODO: Figure out which sort of errors we need to handle
            {
                throw;
            }
            return result;
        }

        private static Node Process(IEditable node, DomainDomain source)
        {
            if (node == null)
                return null;

            Node[] children = Children(node).Select(n => Process(n, source)).ToArray();
            Node[] nexts = Next(node).Select(n => Process(n, source)).ToArray();

            //Node child = (children.Length == 0) ? null : children.Length == 1 ? children[0] : new OneOf() { Next = null, Options = children };
            //Node next = (nexts.Length == 0) ? null : nexts.Length == 1 ? nexts[0] : new OneOf() { Next = null, Options = nexts };

            if (node.NodeTypeId == DomainIDs.AutoComplete.ExactlyOne)
            {
                return new Count() { Min = 1, Max = 1, Subjects = children, Next = nexts };
            }
            else if (node.NodeTypeId == DomainIDs.AutoComplete.OneOrMore)
            {
                return new Count() { Min = 1, Max = int.MaxValue, Subjects = children, Next = nexts };
            }
            else if (node.NodeTypeId == DomainIDs.AutoComplete.ZeroOrMore)
            {
                return new Count() { Min = 0, Max = int.MaxValue, Subjects = children, Next = nexts };
            }
            else if (node.NodeTypeId == DomainIDs.AutoComplete.ZeroOrOne)
            {
                return new Count() { Min = 0, Max = 1, Subjects = children, Next = nexts };
            }
            else if (node.NodeTypeId == DomainIDs.AutoComplete.String)
            {
                return new String() { Next = nexts, Value = node.Parameters.Single().ValueAsString() }; //TODO: Be less lazy and get the parameter by Id rather than assuming there's only one
            }
            else if (node.NodeTypeId == DomainIDs.AutoComplete.Character)
            {
                var result = Character.Make(node.Parameters.Single().ValueAsString());
                result.Next = nexts;
                return result;
            }
            else if (node.NodeTypeId == DomainIDs.AutoComplete.EnumerationValue)
            {
                var result = EnumerationValue.Make(node.Parameters.Single() as IEnumParameter, source);
                result.Next = nexts;
                return result;
            }
            else if (node.NodeTypeId == DomainIDs.AutoComplete.DynamicEnumerationValue)
            {
                return new DynamicEnumerationValue(node.Parameters.Single() as IEnumParameter) { Next = nexts };
            }
            else if (node.NodeTypeId == DomainIDs.AutoComplete.LocalDynamicEnumerationValue)
            {
                return new DynamicEnumerationValue(node.Parameters.Single() as IEnumParameter) { Next = nexts };
            }
            else
                return null;//TODO: Not sure what effect this would have
        }

        struct Match
        {
            string m_matched;
            string m_remaining;
            public string Matched { get { return m_matched; } set { m_matched = value; } }
            public string Remaining { get { return m_remaining; } set { m_remaining = value; } }
        }

        private abstract class Node : IAutoCompletePattern
        {
            public Node[] Next;

            //For debugging purposes
            IEnumerable<string> IAutoCompletePattern.AutoCompleteSuggestions(IParameter p, string s, Func<ParameterType, DynamicEnumParameter.Source> enumSource)
            {
                var result = new List<string>();
                foreach (var a in AutoCompleteSuggestions(p, new Match { Matched = "", Remaining = s }, enumSource))
                    result.Add(a);
                return result;
                //return AutoCompleteSuggestions(p, s).ToArray();
            }

            public IEnumerable<string> AutoCompleteSuggestions(IParameter p, Match s, Func<ParameterType, DynamicEnumParameter.Source> enumSource)
            {
                //Return any values for this node that will match outright
                foreach (var a in OwnAutoCompleteSuggestions(p, s, enumSource))
                    yield return a;

                //Take our chunk out and pass what's left onto the next nodes in the sequence to try and match or make suggestions
                foreach (var match in OwnMatches(p, s.Remaining, enumSource))
                {
                    foreach (var n in Next)
                        foreach (var a in n.AutoCompleteSuggestions(p, match, enumSource))
                            yield return s.Matched + a;
                }
            }

            public IEnumerable<string> OwnAutoCompleteSuggestions(IParameter p, Match s, Func<ParameterType, DynamicEnumParameter.Source> enumSource)
            {
                return OwnAutoCompleteSuggestionsImpl(p, s, enumSource).Select(a => s.Matched + a);
            }
            public abstract IEnumerable<string> OwnAutoCompleteSuggestionsImpl(IParameter p, Match s, Func<ParameterType, DynamicEnumParameter.Source> enumSource);

            /// <summary>
            /// Calculate all the ways in which this node (including its children) can match the input string
            /// </summary>
            /// <param name="p">The parameter we're trying to autocomplete</param>
            /// <param name="s">The string we're trying to match against this node and its children</param>
            /// <returns>collection of all remaining strings after match has been removed i.e. s = match + result.
            /// Thus, an empty string would indicate a perfect match.</returns>
            public IEnumerable<Match> Matches(IParameter p, Match s, Func<ParameterType, DynamicEnumParameter.Source> enumSource)
            {
                if (Next.Any())
                    return OwnMatches(p, s.Remaining, enumSource).SelectMany(m => Next.SelectMany(n => n.Matches(p, m, enumSource).Select(mm => new Match { Matched = s.Matched + mm.Matched, Remaining = mm.Remaining })));
                else
                    return OwnMatches(p, s.Remaining, enumSource).Select(mm => new Match { Matched = s.Matched + mm.Matched, Remaining = mm.Remaining });
            }

            /// <summary>
            /// Calculate all the ways in which this node (excluding subsequent nodes in its sequence) can match the input string
            /// </summary>
            /// <param name="p">The parameter we're trying to autocomplete</param>
            /// <param name="s">The string we're trying to match against this node</param>
            /// <returns>collection of all remaining strings after match has been removed i.e. s = match + result.
            /// Thus, an empty string would indicate a perfect match.</returns>
            public abstract IEnumerable<Match> OwnMatches(IParameter p, string s, Func<ParameterType, DynamicEnumParameter.Source> enumSource);

            //public virtual IEnumerable<string> AutoCompleteSuggestions(IParameter p, string s)
            //{
            //    //TODO: Implement this properly
            //    return AutoCompleteSuggestionsDemo(s);
            //}
        }

        private class Count : Node
        {
            public int Min;
            public int Max;
            public Node[] Subjects;

            public override IEnumerable<Match> OwnMatches(IParameter p, string s, Func<ParameterType, DynamicEnumParameter.Source> enumSource)
            {
                var input = new Match { Matched = "", Remaining = s };
                if (Min == 0)
                    yield return input;
                List<Match> test = new List<Match> { input };
                List<Match> next = new List<Match>();
                for (int i = 1; i <= Max && test.Any(); i++)
                {
                    foreach (var t in test)
                    {
                        foreach (var subject in Subjects)
                        {
                            next.AddRange(subject.Matches(p, t, enumSource));
                        }
                    }

                    if (i >= Min)
                        foreach (var n in next)
                            yield return n;

                    Utilities.Util.Swap(ref test, ref next);
                    next.Clear();
                }
            }

            public override IEnumerable<string> OwnAutoCompleteSuggestionsImpl(IParameter p, Match s, Func<ParameterType, DynamicEnumParameter.Source> enumSource)
            {
                s.Matched = "";
                return Subjects.SelectMany(subject => subject.AutoCompleteSuggestions(p, s, enumSource));
            }
        }
        private class String : Node
        {
            public string Value;

            public override IEnumerable<string> OwnAutoCompleteSuggestionsImpl(IParameter p, Match s, Func<ParameterType, DynamicEnumParameter.Source> enumSource)
            {
                if (s.Remaining != Value) //If the whole string matches we don't need to suggest it (indeed doing so will result in a repeated string in the suggestion)
                {
                    if (Value.StartsWith(s.Remaining))
                        yield return Value;
                }
            }

            //public override IEnumerable<string> AutoCompleteSuggestions(IParameter p, string s)
            //{
            //    if (Value.StartsWith(s))
            //        yield return Value;
            //    else if (Value == s)
            //        foreach (var v in Next.SelectMany(node => node.AutoCompleteSuggestions(p, s.Substring(s.Length))))
            //            yield return v;
            //}

            public override IEnumerable<Match> OwnMatches(IParameter p, string s, Func<ParameterType, DynamicEnumParameter.Source> enumSource)
            {
                if (s.StartsWith(Value))
                {
                    yield return new Match { Matched = Value, Remaining = s.Substring(Value.Length) };
                }
            }
        }

        private static class Character
        {
            internal static Node Make(string v)
            {
                return new Count { Min = 1, Max = 1, Subjects = v.Distinct().Select(o => new String { Value = o.ToString(), Next = new Node[0] }).ToArray() };
            }
        }

        private class DynamicEnumerationValue : Node
        {
            ParameterType Type;
            public DynamicEnumerationValue(IEnumParameter parameter)
            {
                Type = ParameterType.Basic.FromGuid(parameter.Value);
            }

            public override IEnumerable<string> OwnAutoCompleteSuggestionsImpl(IParameter p, Match s, Func<ParameterType, DynamicEnumParameter.Source> enumSource)
            {
                var source = enumSource(Type);
                foreach (var option in source.Options)
                {
                    if (s.Remaining != option) //If the whole string matches we don't need to suggest it (indeed doing so will result in a repeated string in the suggestion)
                    {
                        if (option.StartsWith(s.Remaining))
                            yield return option;
                    }
                }
            }

            public override IEnumerable<Match> OwnMatches(IParameter p, string s, Func<ParameterType, DynamicEnumParameter.Source> enumSource)
            {
                var source = enumSource(Type);
                foreach (var option in source.Options)
                {
                    if (s.StartsWith(option))
                    {
                        yield return new Match { Matched = option, Remaining = s.Substring(option.Length) };
                    }
                }
            }
        }

        private static class EnumerationValue
        {
            internal static Node Make(IEnumParameter parameter, DomainDomain source)
            {
                var options = source.GetEnumOptions(parameter.Value).Select(o => o.Name);
                return new Count { Min = 1, Max = 1, Subjects = options.Select(o => new String { Value = o, Next = new Node[0] }).ToArray() };
            }

            internal static Node MakeDynamic(IEnumParameter parameter, DomainDomain source, ConversationDataSource conversationSource, object newSourceId)
            {
                var enumsource = conversationSource.GetSource(ParameterType.Basic.FromGuid(parameter.Value), newSourceId);
                var options = enumsource.Options;
                return new Count { Min = 1, Max = 1, Subjects = options.Select(o => new String { Value = o, Next = new Node[0] }).ToArray() };
            }

            //internal DynamicEnumParameter.Source GetSource(IDynamicEnumParameter parameter, object newSourceID)
            //{
            //    if (parameter.Local)
            //        return m_types.GetLocalDynamicEnumSource(parameter.TypeId, newSourceID);
            //    else
            //        return m_types.GetDynamicEnumSource(parameter.TypeId);
            //}
        }
    }
}
