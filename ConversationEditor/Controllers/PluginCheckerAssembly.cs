using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Conversation;
using Utilities;
using System.IO;

namespace ConversationEditor
{
    using ConversationNode = Conversation.ConversationNode<Conversation.INodeGUI>;

    public class PluginAssembly
    {
        public readonly string FileName;
        public readonly Assembly Assembly;
        public PluginAssembly(string fileName)
        {
            FileName = fileName;
            FileInfo file = new FileInfo(@".\Plugins\" + fileName);
            Assembly = Assembly.LoadFile(file.FullName);
        }

        public override string ToString()
        {
            return FileName;
        }

        public override bool Equals(object obj)
        {
            var other = obj as PluginAssembly;
            if (other == null)
                return false;
            return object.Equals(FileName, other.FileName);
        }

        public override int GetHashCode()
        {
            return FileName.GetHashCode();
        }
    }

    public class ErrorCheckerAssembly
    {
        public readonly PluginAssembly m_assembly;
        public readonly QuickLookupCollection<ErrorCheckerController.ErrorCheckerData, string> Types = new QuickLookupCollection<ErrorCheckerController.ErrorCheckerData, string>(a => a.SerializeName);

        public ErrorCheckerAssembly(string path)
            : this(new PluginAssembly(path))
        {
        }

        public ErrorCheckerAssembly(PluginAssembly assembly)
        {
            m_assembly = assembly;
            foreach (Type type in LoadAssembly())
            {
                Types.Add(new ErrorCheckerController.ErrorCheckerData(this, type, true));
            }
        }

        private List<Type> LoadAssembly()
        {
            List<Type> result = new List<Type>();
            var allTypes = m_assembly.Assembly.GetExportedTypes();

            //Load error checkers
            foreach (Type t in allTypes)
            {
                try
                {
                    if (t.IsGenericTypeDefinition)
                    {
                        var concrete = t.MakeGenericType(typeof(ConversationNode));
                        if (concrete.IsSubclassOf(typeof(ErrorChecker<ConversationNode>)))
                        {
                            result.Add(concrete);
                        }
                    }
                }
                catch
                {
                }
            }
            return result;
        }

        public void SetEnabled(string name, bool enabled)
        {
            if (Types.ContainsKey(name))
                Types[name].Enabled = enabled;
        }

        public IEnumerable<ErrorChecker<ConversationNode>> GetEnabledErrorCheckers()
        {
            var types = Types.Where(v => v.Enabled).Select(a => a.Type);
            foreach (Type t in types)
            {
                var c = t.GetConstructor(Type.EmptyTypes);
                ErrorChecker<ConversationNode> checker = (ErrorChecker<ConversationNode>)c.Invoke(new object[0]);
                yield return checker;
            }
        }
    }

}
