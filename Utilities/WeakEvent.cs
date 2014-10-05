using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public abstract class WeakCallback
    {
        public abstract bool Expired { get; }
        public abstract void Execute();
    }
    public class WeakCallback<T> : WeakCallback where T : class
    {
        public WeakCallback(T o, Action<T> c)
        {
            Target = new WeakReference(o);
            Callback = c;
        }
        WeakReference Target;
        Action<T> Callback;

        public override bool Expired
        {
            get { return !Target.IsAlive; }
        }

        public override void Execute()
        {
            T target = Target.Target as T;
            if (target != null)
                Callback(target);
        }

        public static implicit operator Action(WeakCallback<T> a)
        {
            return a.Execute;
        }
    }

    public class WeakEvent
    {
        private List<WeakCallback> m_callbacks = new List<WeakCallback>();
        public void Register(WeakCallback a)
        {
            m_callbacks.RemoveAll(b => b.Expired);
            m_callbacks.Add(a);
        }

        public void Deregister(WeakCallback a)
        {
            m_callbacks.RemoveAll(b => b == a || b.Expired);
        }

        public void Execute()
        {
            m_callbacks.RemoveAll(b => b.Expired);
            m_callbacks.ForAll(a => a.Execute());
        }
    }
}
