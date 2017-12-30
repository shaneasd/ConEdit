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

        public static Action<TParameter> Handler<TObject>(TObject o, Action<TObject, TParameter> callback) where TObject : class
        {
            var weak = WeakCallback<TParameter>.Create(o, callback);
            return weak.Execute;
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

        public override bool Expired => !Target.IsAlive;

        public override void Execute(TParameter value)
        {
            if (Target.Target is TObject target)
                Callback(target, value);
        }
    }

    public class WeakEvent<TParameter>
    {
        private List<WeakCallback<TParameter>> m_callbacks = new List<WeakCallback<TParameter>>();

        public Action Register(Action<TParameter> a)
        {
            var callback = WeakCallback<TParameter>.Create(this, (obj, val) => a(val));
            Register(callback);
            return () => Deregister(callback);
        }

        public Action Register<THost>(THost host, Action<THost, TParameter> callback) where THost : class
        {
            var callback2 = (WeakCallback<TParameter>.Create(host, callback));
            Register(callback2);
            return () => Deregister(callback2);
        }

        public Action Register(WeakCallback<TParameter> a)
        {
            m_callbacks.RemoveAll(b => b.Expired);
            m_callbacks.Add(a);
            return () => Deregister(a);
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
