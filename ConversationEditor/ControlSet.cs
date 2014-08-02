using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.Windows.Forms;
using System.Drawing;

namespace ConversationEditor
{
    //TODO: unfocus control when it is removed from the list
    public class ControlSet<T> : IMyControl where T : class, IMyControl
    {
        public readonly CallbackList<T> Controls;
        public ControlSet(params T[] controls)
        {
            Controls = new CallbackList<T>(controls);
            foreach (var c in Controls)
            {
                c.RequestedAreaChanged += OnRequestedAreaChanged;
            }
            Controls.Clearing += () => { foreach (var c in Controls) c.RequestedAreaChanged -= OnRequestedAreaChanged; };
            Controls.Inserting += (i, c) => c.RequestedAreaChanged += OnRequestedAreaChanged;
            Controls.Removing -= (i, c) => c.RequestedAreaChanged -= OnRequestedAreaChanged;

            Controls.Inserting += (i, c) => { if (m_focusedControl >= i) m_focusedControl++; };
            Controls.Removing += (i, c) => { if (m_focusedControl < i) m_focusedControl--; };
        }

        public T this[int index]
        {
            get
            {
                return Controls[index];
            }
        }

        int m_focusedControl = -1;
        private T FocusedControl { get { return m_focusedControl != -1 && m_focusedControl < Controls.Count ? Controls[m_focusedControl] : null; } }

        public void GotFocus()
        {
            if (FocusedControl != null)
                FocusedControl.GotFocus();
        }

        public void LostFocus()
        {
            if (FocusedControl != null)
                FocusedControl.LostFocus();
        }

        public void ForwardFocus()
        {
            m_focusedControl = 0;
            FocusedControl.GotFocus();
        }

        public void BackwardFocus()
        {
            m_focusedControl = Controls.Count - 1;
            FocusedControl.GotFocus();
        }

        public IMyControl MousedControl(MouseEventArgs args)
        {
            foreach (T control in Controls)
                if (control.Contains(args.Location))
                    return control;
            return DummyMyControl.Instance;
        }

        public void MouseUp(MouseEventArgs args)
        {
            MousedControl(args).MouseUp(args);
        }

        public void MouseMove(MouseEventArgs args)
        {
            MousedControl(args).MouseMove(args);
        }

        public void MouseClick(MouseEventArgs args)
        {
            MousedControl(args).MouseClick(args);
        }

        public void MouseWheel(MouseEventArgs args)
        {
            MousedControl(args).MouseWheel(args);
        }

        public void KeyPress(KeyPressEventArgs args)
        {
            if (FocusedControl != null)
                FocusedControl.KeyPress(args);
        }

        public void KeyDown(KeyEventArgs args)
        {
            if (FocusedControl != null)
                FocusedControl.KeyDown(args);
        }

        public bool ProcessTabKey(bool forward)
        {
            if (FocusedControl != null)
                FocusedControl.LostFocus();
            m_focusedControl += forward ? 1 : -1;
            if (FocusedControl != null)
                FocusedControl.GotFocus();
            else
            {
                m_focusedControl = -1;
                return false;
            }
            return true;
        }

        public void MouseDown(MouseEventArgs args)
        {
            var control = MousedControl(args);
            FocusOn(control);
            control.MouseDown(args);
        }

        public void MouseCaptureChanged()
        {
            if (FocusedControl != null)
                FocusedControl.MouseCaptureChanged();
        }

        //public System.Drawing.RectangleF Area
        //{
        //    get
        //    {
        //        if (!Controls.Any())
        //            return RectangleF.Empty;

        //        return Controls.Skip(1).Aggregate(Controls.First().Area, (r, c) => RectangleF.Union(r, c.Area), a => a);
        //    }
        //}

        public void Paint(Graphics g)
        {
            Controls.ForAll(c => c.Paint(g));
        }

        public void RegisterCallbacks(Control host)
        {
            host.MouseDown += (a, args) => MouseDown(args);
            host.MouseUp += (a, args) => MouseUp(args);
            host.MouseMove += (a, args) => MouseMove(args);
            host.MouseClick += (a, args) => MouseClick(args);
            host.MouseWheel += (a, args) => MouseWheel(args);
            host.KeyPress += (a, args) => KeyPress(args);
            host.KeyDown += (a, args) => KeyDown(args);
            host.Paint += (a, args) => Paint(args.Graphics);
            host.MouseCaptureChanged += (a, b) => MouseCaptureChanged();
            host.GotFocus += (a, b) => GotFocus();
            host.LostFocus += (a, b) => LostFocus();
        }

        private void OnRequestedAreaChanged() { RequestedAreaChanged.Execute(); }

        public event Action RequestedAreaChanged;

        //public SizeF RequestedArea
        //{
        //    get
        //    {
        //        SizeF size = SizeF.Empty;
        //        //foreach (var c in Controls)
        //        //{
        //        //    size.Width = Math.Max(c.Area.X + c.RequestedArea.Width - Area.X, size.Width);
        //        //    size.Height = Math.Max(c.Area.Y + c.RequestedArea.Height - Area.Y, size.Height);
        //        //}
        //        return size;
        //    }
        //}

        public bool Contains(PointF point)
        {
            return Controls.Any(c => c.Contains(point));
        }

        public void FocusOn(IMyControl control)
        {
            if (Controls.IndexOf(control) != m_focusedControl)
            {
                if (m_focusedControl != -1)
                    Controls[m_focusedControl].LostFocus();

                m_focusedControl = Controls.IndexOf(control);
                control.GotFocus();
            }
        }
    }

    public class ControlSet : ControlSet<IMyControl>
    {
        public ControlSet(params IMyControl[] controls)
            : base(controls)
        {
        }
    }

    public class SizedControlSet : ControlSet
    {
        private Func<SizeF> m_requestedSize;

        public SizedControlSet(Func<SizeF> requestedSize, params IMyControl[] controls)
            : base(controls)
        {
            m_requestedSize = requestedSize;
        }

        public SizeF RequestedSize { get { return m_requestedSize(); } }
    }
}
