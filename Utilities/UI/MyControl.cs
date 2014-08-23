using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Utilities
{
    public interface IFocusProvider
    {
        MyControl LastFocused { get; set; }
    }

    public abstract class MyControl : IDisposable
    {
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

        //public void RegisterCallbacks(Action<MouseEventArgs> mouseDown, Action<MouseEventArgs> mouseUp, Action<MouseEventArgs> mouseMove, Action<MouseEventArgs> mouseClick,
        //                              Action<KeyEventArgs> keyDown, Action<KeyPressEventArgs> keyPress, Action<MouseEventArgs> mouseWheel,
        //                              Action mouseCaptureChanged, Action<Graphics> paint, Func<PointF, bool> contains,
        //                              Func<MyControl> lastFocussed, Control control)
        public void RegisterCallbacks(IFocusProvider focus, Control control)
        {
            MouseEventHandler MouseDown = (a, args) =>
            {
                if (this.Contains(args.Location))
                {
                    this.MouseDown(args);
                    if (focus.LastFocused != this)
                    {
                        focus.LastFocused.LostFocus();
                        focus.LastFocused = this;
                        this.GotFocus();
                    }
                }
            };

            MouseEventHandler MouseUp = (a, args) =>
            {
                if (this.Contains(args.Location))
                {
                    this.MouseUp(args);
                    if (focus.LastFocused != this)
                    {
                        focus.LastFocused.LostFocus();
                        focus.LastFocused = this;
                        this.GotFocus();
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
                    if (focus.LastFocused != this)
                    {
                        focus.LastFocused.LostFocus();
                        focus.LastFocused = this;
                        this.GotFocus();
                    }
                }
            };
            KeyPressEventHandler KeyPress = (a, args) => { if (this == focus.LastFocused) this.KeyPress(args); };
            KeyEventHandler KeyDown = (a, args) => { if (this == focus.LastFocused)this.KeyDown(args); };
            PaintEventHandler Paint = (a, args) => this.Paint(args.Graphics);
            EventHandler GotFocus = (a, args) => { if (this == focus.LastFocused)this.GotFocus(); };
            EventHandler LostFocus = (a, args) => { if (this == focus.LastFocused)this.LostFocus(); };

            control.MouseDown += MouseDown;
            control.MouseUp += MouseUp;
            control.MouseMove += MouseMove;
            control.MouseClick += MouseClick;
            control.KeyPress += KeyPress;
            control.KeyDown += KeyDown;
            control.Paint += Paint;
            control.GotFocus += GotFocus;
            control.LostFocus += LostFocus;

            m_disposeActions.Push(() =>
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

        public virtual void Dispose()
        {
            foreach (var action in m_disposeActions)
            {
                action();
            }
            m_disposeActions.Clear();
        }

        protected Stack<Action> m_disposeActions = new Stack<Action>();
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

        public static readonly MyControl Instance = new DummyMyControl();
    }
}
