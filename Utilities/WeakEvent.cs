using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public abstract class WeakCallback<TParameter>
    {
        public abstract bool Expired { get; }
        public abstract void Execute(TParameter value);
        public static WeakCallback<TParameter> Create<TObject>(TObject obj, Action<TObject, TParameter> c) where TObject : class
        {
            return new WeakCallback<TObject, TParameter>(obj, c);
        }
    }
    public class WeakCallback<TObject, TParameter> : WeakCallback<TParameter> where TObject : class
    {
        public WeakCallback(TObject o, Action<TObject, TParameter> c)
        {
            Target = new WeakReference(o);
            Callback = c;
        }
        WeakReference Target;
        Action<TObject, TParameter> Callback;

        public override bool Expired
        {
            get { return !Target.IsAlive; }
        }

        public override void Execute(TParameter value)
        {
            TObject target = Target.Target as TObject;
            if (target != null)
                Callback(target, value);
        }

        //public static implicit operator Action(WeakCallback<T> a)
        //{
        //    return a.Execute;
        //}
    }

    public class WeakEvent<TParameter>
    {
        private List<WeakCallback<TParameter>> m_callbacks = new List<WeakCallback<TParameter>>();

        public void Register(Action<TParameter> a)
        {
            Register(WeakCallback<TParameter>.Create(this, (obj, val) => a(val)));
        }

        public void Register<THost>(THost host, Action<THost, TParameter> callback) where THost : class
        {
            Register(WeakCallback<TParameter>.Create(host, callback));
        }

        public void Register(WeakCallback<TParameter> a)
        {
            m_callbacks.RemoveAll(b => b.Expired);
            m_callbacks.Add(a);
        }

        public void Deregister(WeakCallback<TParameter> a)
        {
            m_callbacks.RemoveAll(b => b == a || b.Expired);
        }

        public void Execute(TParameter value)
        {
            m_callbacks.RemoveAll(b => b.Expired);
            m_callbacks.ForAll(a => a.Execute(value));
        }
    }
}
