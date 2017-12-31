using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    /// <summary>
    /// Provides a utility to wrap a callback in a way that allows delaying its execution.
    /// This is achieved using the 'using' mechanism in C# to ensure the callback is restored.
    /// Once the callback is restored, if it would have executed, it then executes.
    /// This is useful for suppressing callbacks that update state when it is known that further updates are expected
    /// </summary>
    public class SuppressibleAction
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

        public bool Suppressed => m_actionSuppressors.Any();

        public void Dispose()
        {
            m_action = () => { };
        }
    }
}
