using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Globalization;

namespace Utilities.UI
{
    public interface IFocusProvider
    {
        MyControl LastFocused { get; set; }
    }

    public class FocusProvider : IFocusProvider
    {
        private MyControl m_focussed;
        public FocusProvider(MyControl focussed)
        {
            m_focussed = focussed;
        }

        public MyControl LastFocused
        {
            get { return m_focussed; }
            set { m_focussed = value; }
        }
    }

    public abstract class MyControl : Disposable, IDisposable
    {
        public string Name { get; set; }
        protected MyControl()
        {
            this.GetType().ToString();
        }

        public abstract void MouseDown(MouseEventArgs args);
        public abstract void MouseUp(MouseEventArgs args);
        public abstract void MouseMove(MouseEventArgs args);
        public abstract void MouseClick(MouseEventArgs args);
        public abstract void KeyDown(KeyEventArgs args);
        public abstract void KeyPress(KeyPressEventArgs args);
        public abstract void MouseWheel(MouseEventArgs args);
        public abstract void GotFocus();
        public abstract void LostFocus();
        public abstract void MouseCaptureChanged();
        public abstract void Paint(Graphics g);
        public abstract bool Contains(PointF point);
        //RectangleF Area { get; }
        //SizeF RequestedArea { get; }
        public abstract event Action RequestedAreaChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="focus">Can be null indicating there is no focus concept</param>
        /// <param name="control"></param>
        public void RegisterCallbacks(IFocusProvider focus, Control control)
        {
            MouseEventHandler MouseDown = (a, args) =>
            {
                if (this.Contains(args.Location))
                {
                    if (focus != null)
                    {
                        if (focus.LastFocused != this)
                        {
                            focus.LastFocused.LostFocus();
                            focus.LastFocused = this;
                            this.GotFocus();
                        }
                    }
                    this.MouseDown(args);
                }
            };

            MouseEventHandler MouseUp = (a, args) =>
            {
                if (this.Contains(args.Location))
                {
                    this.MouseUp(args);
                    if (focus != null)
                    {
                        if (focus.LastFocused != this)
                        {
                            focus.LastFocused.LostFocus();
                            focus.LastFocused = this;
                            this.GotFocus();
                        }
                    }
                }
            };
            MouseEventHandler MouseMove = (a, args) =>
            {
                if (this.Contains(args.Location))
                {
                    this.MouseMove(args);
                }
            };
            MouseEventHandler MouseClick = (a, args) =>
            {
                if (this.Contains(args.Location))
                {
                    this.MouseClick(args);
                    if (focus != null)
                    {
                        if (focus.LastFocused != this)
                        {
                            focus.LastFocused.LostFocus();
                            focus.LastFocused = this;
                            this.GotFocus();
                        }
                    }
                }
            };
            KeyPressEventHandler KeyPress = (a, args) => { if (focus == null || this == focus.LastFocused) this.KeyPress(args); };
            KeyEventHandler KeyDown = (a, args) => { if (focus == null || this == focus.LastFocused) this.KeyDown(args); };
            PaintEventHandler Paint = (a, args) => this.Paint(args.Graphics);
            EventHandler GotFocus = (a, args) => { if (focus == null || this == focus.LastFocused) this.GotFocus(); };
            EventHandler LostFocus = (a, args) => { if (focus == null || this == focus.LastFocused) this.LostFocus(); };

            control.MouseDown += MouseDown;
            control.MouseUp += MouseUp;
            control.MouseMove += MouseMove;
            control.MouseClick += MouseClick;
            control.KeyPress += KeyPress;
            control.KeyDown += KeyDown;
            control.Paint += Paint;
            control.GotFocus += GotFocus;
            control.LostFocus += LostFocus;

            PushDisposeActions(() =>
            {
                control.MouseDown -= MouseDown;
                control.MouseUp -= MouseUp;
                control.MouseMove -= MouseMove;
                control.MouseClick -= MouseClick;
                control.KeyPress -= KeyPress;
                control.KeyDown -= KeyDown;
                control.Paint -= Paint;
                control.GotFocus -= GotFocus;
                control.LostFocus -= LostFocus;
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var action in m_disposeActions)
                {
                    action();
                }
                m_disposeActions.Clear();
            }
        }

        private Stack<Action> m_disposeActions = new Stack<Action>();
        protected void PushDisposeActions(Action action)
        {
            m_disposeActions.Push(action);
        }
    }

    public class DummyMyControl : MyControl
    {
        public override void MouseDown(MouseEventArgs args) { }
        public override void MouseUp(MouseEventArgs args) { }
        public override void MouseMove(MouseEventArgs args) { }
        public override void MouseClick(MouseEventArgs args) { }
        public override void KeyDown(KeyEventArgs args) { }
        public override void KeyPress(KeyPressEventArgs args) { }
        public override void MouseWheel(MouseEventArgs args) { }
        public override void GotFocus() { }
        public override void LostFocus() { }
        public override void MouseCaptureChanged() { }
        public override void Paint(Graphics g) { }
        public override event Action RequestedAreaChanged { add { } remove { } }
        public override bool Contains(PointF point) { return false; }

        public static MyControl Instance { get; } = new DummyMyControl();
    }
}
