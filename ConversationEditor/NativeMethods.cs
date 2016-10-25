using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ConversationEditor
{
    struct IconInfo
    {
        /// <summary>
        /// Specifies whether this structure defines an icon or a cursor.
        /// A value of TRUE specifies an icon; FALSE specifies a cursor
        /// </summary>
        public bool fIcon;
        /// <summary>
        /// The x-coordinate of a cursor's hot spot
        /// </summary>
        public Int32 xHotspot;
        /// <summary>
        /// The y-coordinate of a cursor's hot spot
        /// </summary>
        public Int32 yHotspot;
        /// <summary>
        /// The icon bitmask bitmap
        /// </summary>
        IntPtr hbmMask;
        /// <summary>
        /// A handle to the icon color bitmap.
        /// </summary>
        IntPtr hbmColor;
    }

    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);

        [DllImport("user32.dll")]
        internal static extern IntPtr CreateIconIndirect(ref IconInfo icon);
    }
}
