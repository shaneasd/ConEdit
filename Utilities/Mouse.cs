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

namespace Utilities
{
    //original code from https://support.microsoft.com/en-us/kb/318804 and https://blogs.msdn.microsoft.com/toub/2006/05/03/low-level-mouse-hook-in-c/
    public static class Mouse
    {
        public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        //Declare the hook handle as an int.
        static IntPtr hHook = IntPtr.Zero;

        //Declare the mouse hook constant.
        //For other hook types, you can obtain these values from Winuser.h in the Microsoft SDK.
        private const int WH_MOUSE_LL = 14;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;

        //Declare MouseHookProcedure as a HookProc type.
        static HookProc MouseHookProcedure;

        //Declare the wrapper managed POINT class.
        [StructLayout(LayoutKind.Sequential)]
        public struct Point
        {
            private int x;
            private int y;

            public int X => x;
            public int Y => y;
        }

        //Declare the wrapper managed MouseHookStruct class.
        [StructLayout(LayoutKind.Sequential)]
        public struct MouseHookStruct
        {
            private Point pt;
            private int hwnd;
            private int wHitTestCode;
            private int dwExtraInfo;

            public Point Point => pt;
        }

        //This is the Import for the SetWindowsHookEx function.
        //Use this function to install a thread-specific hook.
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        //This is the Import for the CallNextHookEx function.
        //Use this function to pass the hook information to the next hook procedure in chain.
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, ThrowOnUnmappableChar = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public static WeakEvent<System.Drawing.Point> MouseDown = new WeakEvent<System.Drawing.Point>();
        public static WeakEvent<System.Drawing.Point> MouseUp = new WeakEvent<System.Drawing.Point>();

        public static IntPtr MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            //Marshall the data from the callback.
            MouseHookStruct MyMouseHookStruct = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));

            if (nCode >= 0)
            {
                switch (wParam.ToInt64())
                {
                    case WM_LBUTTONDOWN:
                        MouseDown.Execute(new System.Drawing.Point(MyMouseHookStruct.Point.X, MyMouseHookStruct.Point.Y));
                        break;
                    case WM_LBUTTONUP:
                        MouseUp.Execute(new System.Drawing.Point(MyMouseHookStruct.Point.X, MyMouseHookStruct.Point.Y));
                        break;
                }
            }
            return CallNextHookEx(hHook, nCode, wParam, lParam);
        }

        static Mouse()
        {
            if (hHook == IntPtr.Zero)
            {
                // Create an instance of HookProc.
                MouseHookProcedure = new HookProc(MouseHookProc);

                using (Process process = Process.GetCurrentProcess())
                using (ProcessModule module = process.MainModule)
                {
                    IntPtr handle = GetModuleHandle(module.ModuleName);
                    if (!Debugger.IsAttached) //This is a pretty depressing necessity which will probably come back to bite me.
                    {
                        //https://stackoverflow.com/questions/9727327/windows-keyboard-hook-hangs-debugger
                        hHook = SetWindowsHookEx(WH_MOUSE_LL, MouseHookProcedure, handle, 0);
                    }
                }

                //If the SetWindowsHookEx function fails.
                //if (hHook == IntPtr.Zero)
                //{
                //    Debug.WriteLine("SetWindowsHookEx Failed");
                //}
            }
        }
    }
}
