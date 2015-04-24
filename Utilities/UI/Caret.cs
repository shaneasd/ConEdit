using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Utilities.UI
{
    public class Caret : Disposable
    {
        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CreateCaret(IntPtr hWnd, IntPtr hBitmap, int nWidth, int nHeight);

        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCaretPos(int x, int y);

        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyCaret();

        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowCaret(IntPtr hWnd);

        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool HideCaret(IntPtr hWnd);

        private IntPtr m_handle;
        public Caret(Control control, int width, int height)
        {
            m_handle = control.Handle;
            CreateCaret(m_handle, IntPtr.Zero, width, height);
        }

        public void MoveTo(int x, int y)
        {
            SetCaretPos(x, y);
        }

        public void Show() { ShowCaret(m_handle); }
        public void Hide() { HideCaret(m_handle); }

        protected override void Dispose(bool disposing)
        {
            DestroyCaret();
        }
    }
}
