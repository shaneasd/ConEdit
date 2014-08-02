using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Utilities
{
    public static class Caret
    {
        [DllImport("User32.dll")]
        public static extern bool CreateCaret(IntPtr hWnd, IntPtr hBitmap, int nWidth, int nHeight);

        [DllImport("User32.dll")]
        public static extern bool SetCaretPos(int x, int y);

        [DllImport("User32.dll")]
        public static extern bool DestroyCaret();

        [DllImport("User32.dll")]
        public static extern bool ShowCaret(IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern bool HideCaret(IntPtr hWnd);
    }
}
