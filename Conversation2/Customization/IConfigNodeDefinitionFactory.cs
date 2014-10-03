using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public interface IConfigNodeDefinitionFactory
    {
        IEnumerable<IConfigNodeDefinition> ConfigNodeDefinitions();
    }
}
