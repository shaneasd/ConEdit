//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Runtime.InteropServices;
//using System.Diagnostics;
//using System.Threading;
//using System.Globalization;

//namespace Utilities
//{
//    //https://support.microsoft.com/en-us/kb/318804
//    public static class Mouse
//    {
//        public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

//        //Declare the hook handle as an int.
//        static IntPtr hHook = IntPtr.Zero;

//        //Declare the mouse hook constant.
//        //For other hook types, you can obtain these values from Winuser.h in the Microsoft SDK.
//        const int WH_MOUSE = 7;

//        //Declare MouseHookProcedure as a HookProc type.
//        static HookProc MouseHookProcedure;

//        //Declare the wrapper managed POINT class.
//        [StructLayout(LayoutKind.Sequential)]
//        public class POINT
//        {
//            public int x;
//            public int y;
//        }

//        //Declare the wrapper managed MouseHookStruct class.
//        [StructLayout(LayoutKind.Sequential)]
//        public class MouseHookStruct
//        {
//            public POINT pt;
//            public int hwnd;
//            public int wHitTestCode;
//            public int dwExtraInfo;
//        }

//        //This is the Import for the SetWindowsHookEx function.
//        //Use this function to install a thread-specific hook.
//        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
//        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

//        //This is the Import for the CallNextHookEx function.
//        //Use this function to pass the hook information to the next hook procedure in chain.
//        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
//        private static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

//        public static IntPtr MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
//        {
//            //Marshall the data from the callback.
//            MouseHookStruct MyMouseHookStruct = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));

//            if (nCode < 0)
//            {
//                return CallNextHookEx(hHook, nCode, wParam, lParam);
//            }
//            else
//            {
//                //Create a string variable that shows the current mouse coordinates.
//                string strCaption = "x = " +
//                        MyMouseHookStruct.pt.x.ToString("d", CultureInfo.InvariantCulture) +
//                            "  y = " +
//                MyMouseHookStruct.pt.y.ToString("d", CultureInfo.InvariantCulture);
//                Debug.WriteLine(strCaption);
//                return CallNextHookEx(hHook, nCode, wParam, lParam);
//            }
//        }

//        static Mouse()
//        {
//            if (hHook == IntPtr.Zero)
//            {
//                // Create an instance of HookProc.
//                MouseHookProcedure = new HookProc(MouseHookProc);

//                hHook = SetWindowsHookEx(WH_MOUSE,
//                            MouseHookProcedure,
//                            (IntPtr)0,
//                            AppDomain.GetCurrentThreadId());
//                //If the SetWindowsHookEx function fails.
//                if (hHook == IntPtr.Zero)
//                {
//                    Debug.WriteLine("SetWindowsHookEx Failed");
//                    return;
//                }
//            }
//            //else
//            //{
//            //    bool ret = UnhookWindowsHookEx(hHook);
//            //    //If the UnhookWindowsHookEx function fails.
//            //    if (ret == false)
//            //    {
//            //        MessageBox.Show("UnhookWindowsHookEx Failed");
//            //        return;
//            //    }
//            //    hHook = 0;
//            //    button1.Text = "Set Windows Hook";
//            //    this.Text = "Mouse Hook";
//            //}
//        }
//    }
//}
