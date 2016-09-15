using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Utilities.UI
{
    internal static class NativeMethods
    {
        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CreateCaret(IntPtr hWnd, IntPtr hBitmap, int nWidth, int nHeight);

        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetCaretPos(int x, int y);

        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyCaret();

        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowCaret(IntPtr hWnd);

        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool HideCaret(IntPtr hWnd);
    }

    public class Caret : Disposable
    {
        private IntPtr m_handle;
        public Caret(Control control, int width, int height)
        {
            m_handle = control.Handle;
            NativeMethods.CreateCaret(m_handle, IntPtr.Zero, width, height);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification ="Suits the design")]
        public void MoveTo(int x, int y)
        {
            NativeMethods.SetCaretPos(x, y);
        }

        public void Show() { NativeMethods.ShowCaret(m_handle); }
        public void Hide() { NativeMethods.HideCaret(m_handle); }

        protected override void Dispose(bool disposing)
        {
            NativeMethods.DestroyCaret();
        }
    }
}
