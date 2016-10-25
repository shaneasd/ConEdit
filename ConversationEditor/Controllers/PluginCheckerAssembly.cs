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
    using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGui>;

    internal class PluginAssembly
    {
        public readonly string FileName; //Null if main assembly
        public readonly Assembly Assembly;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFile", Justification = "Can't see an alternative method...")]
        public PluginAssembly(string fileName)
        {
            FileName = fileName;
            FileInfo file = new FileInfo(@".\Plugins\" + fileName);
            Assembly = Assembly.LoadFile(file.FullName);
        }

        public PluginAssembly(Assembly assembly)
        {
            Assembly = assembly;
            FileName = null;
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
            if (FileName != null)
                return object.Equals(FileName, other.FileName);
            else
                return this.Assembly.GetName().Equals(other.Assembly.GetName());
        }

        public override int GetHashCode()
        {
            if (FileName == null)
                return Assembly.GetName().GetHashCode();
            return FileName.GetHashCode();
        }
    }

    internal class ErrorCheckerAssembly
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
                Types.Add(new ErrorCheckerController.ErrorCheckerData(type, true));
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
                        var args = t.GetGenericArguments();
                        if (args.Length == 1)
                        {
                            var constraints = args[0].GetGenericParameterConstraints();
                            if (constraints.Any(c => c.IsAssignableFrom(typeof(ConversationNode))))
                            {
                                var concrete = t.MakeGenericType(typeof(ConversationNode));
                                if (concrete.IsSubclassOf(typeof(ErrorChecker<ConversationNode>)))
                                {
                                    result.Add(concrete);
                                }
                            }
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
