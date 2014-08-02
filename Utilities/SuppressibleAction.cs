using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class SuppressibleAction : IDisposable
    {
        private Action m_action;
        private bool m_needToExecute = false;
        private List<IDisposable> m_actionSuppressors = new List<IDisposable>();
        public SuppressibleAction(Action action)
        {
            m_action = action;
        }

        public bool TryExecute()
        {
            m_needToExecute = true;
            return MaybeExecute();
        }

        private bool MaybeExecute()
        {
            if (m_needToExecute && !Suppressed)
            {
                m_action.Execute();
                m_needToExecute = false;
                return true;
            }
            return false;
        }

        public IDisposable SuppressCallback()
        {
            IDisposable actionSuppressor = new CallbackSuppressor(this);
            m_actionSuppressors.Add(actionSuppressor);
            return actionSuppressor;
        }

        private class CallbackSuppressor : IDisposable
        {
            private SuppressibleAction m_parent;
            public CallbackSuppressor(SuppressibleAction parent)
            {
                m_parent = parent;
            }

            public void Dispose()
            {
                m_parent.m_actionSuppressors.Remove(this);
                m_parent.MaybeExecute();
            }
        }

        public bool Suppressed { get { return m_actionSuppressors.Any(); } }

        public void Dispose()
        {
            //Reset the action so even if any rogue suppressors are leftover they won't trigger the even anymore
            m_action = () => { };
        }
    }
}
