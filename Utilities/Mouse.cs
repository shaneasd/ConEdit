using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;

namespace Utilities
{
    public static class Mouse
    {
        [DllImport("user32.dll")]
        static extern uint GetMessagePos();

        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;

        public static WeakEvent<System.Drawing.Point> MouseDown { get; } = new WeakEvent<System.Drawing.Point>();
        public static WeakEvent<System.Drawing.Point> MouseUp { get; } = new WeakEvent<System.Drawing.Point>();

        class Filter : System.Windows.Forms.IMessageFilter
        {
            public bool PreFilterMessage(ref Message m)
            {
                uint pos = GetMessagePos();
                short x = (short)(pos & 0x0000ffff);
                short y = (short)((pos & 0xffff0000) >> 16);
                switch (m.Msg)
                {
                    case WM_LBUTTONDOWN:
                        {
                            MouseDown.Execute(new System.Drawing.Point(x, y));
                            break;
                        }
                    case WM_LBUTTONUP:
                        {
                            MouseUp.Execute(new System.Drawing.Point(x, y));
                            break;
                        }
                }
                return false;
            }
        }

        static Mouse()
        {
            System.Windows.Forms.Application.AddMessageFilter(new Filter());
        }
    }
}
