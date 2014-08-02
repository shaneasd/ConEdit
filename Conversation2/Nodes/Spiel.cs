using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public class Spiel : Node
    {
        private IList<Parameter> m_parameters;
        public Spiel(IDataSource datasource, uint id) :base(id, EditableType.Spiel)
        {
            m_parameters = datasource.SpielParameters.ToList();
        }

        public override IEnumerable<Parameter> Parameters
        {
            get { return m_parameters; }
        }

        public override IEnumerable<Parameter> Constants
        {
            get { return Enumerable.Empty<Parameter>(); }
        }

        public override string Name
        {
            get { return "Spiel"; }
        }
    }
}
