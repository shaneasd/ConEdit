using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Reflection;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.IO;

namespace Utilities
{
    public class MyFileLoadException : Exception
    {
        public MyFileLoadException(Exception inner)
            : base("error loading file", inner)
        {
        }
    }

    public static class Util
    {
        public static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

        public static T Clamp<T>(this T val, T min, T max) where T : IComparable
        {
            if (val.CompareTo(min) < 0)
                val = min;
            else if (val.CompareTo(max) > 0)
                val = max;
            return val;
        }

        public static Point Plus(this Point p1, Point p2)
        {
            return new Point(p1.X + p2.X, p1.Y + p2.Y);
        }
        public static PointF Plus(this PointF p1, PointF p2)
        {
            return new PointF(p1.X + p2.X, p1.Y + p2.Y);
        }
        public static PointF Plus(this PointF p1, float x, float y)
        {
            return new PointF(p1.X + x, p1.Y + y);
        }
        public static Point Plus(this Point p, int x, int y)
        {
            return new Point(p.X + x, p.Y + y);
        }
        public static PointF Take(this PointF p1, PointF p2)
        {
            return new PointF(p1.X - p2.X, p1.Y - p2.Y);
        }
        public static Point Take(this Point p1, Point p2)
        {
            return new Point(p1.X - p2.X, p1.Y - p2.Y);
        }
        public static PointF Take(this PointF p1, float x, float y)
        {
            return new PointF(p1.X - x, p1.Y - y);
        }
        public static Point Take(this Point p1, int x, int y)
        {
            return new Point(p1.X - x, p1.Y - y);
        }
        public static Point Negated(this Point p)
        {
            return new Point(-p.X, -p.Y);
        }
        public static PointF Normalised(this Point p)
        {
            float length = (float)Math.Sqrt(p.LengthSquared());
            return new PointF(p.X / length, p.Y / length);
        }
        public static PointF Normalised(this PointF p)
        {
            float length = (float)Math.Sqrt(p.LengthSquared());
            return new PointF(p.X / length, p.Y / length);
        }

        public static float LengthSquared(this PointF p)
        {
            return p.X * p.X + p.Y * p.Y;
        }
        public static int LengthSquared(this Point p)
        {
            return p.X * p.X + p.Y * p.Y;
        }
        public static float Dot(this PointF p1, PointF p2)
        {
            return p1.X * p2.X + p1.Y * p2.Y;
        }

        public static float Cross(this PointF p1, PointF p2)
        {
            return p1.X * p2.Y - p1.Y * p2.X;
        }
        public static float DistanceTo(this PointF p1, PointF p2)
        {
            PointF dif = p1.Take(p2);
            return (float)Math.Sqrt(dif.X * dif.X + dif.Y * dif.Y);
        }
        public static float DistanceTo(this Point p1, Point p2)
        {
            Point dif = p1.Take(p2);
            return (float)Math.Sqrt(dif.X * dif.X + dif.Y * dif.Y);
        }
        public static PointF ScaleBy(this PointF p, float s)
        {
            return new PointF(p.X * s, p.Y * s);
        }

        /// <summary>
        /// Rotates the point 90 degrees anticlockwise about the origin
        /// </summary>
        public static PointF Rot90(this PointF p)
        {
            return new PointF(-p.Y, p.X);
        }

        public static Point Center(this Rectangle r)
        {
            return new Point(r.Left + r.Width / 2, r.Top + r.Height / 2);
        }

        public static PointF Center(this RectangleF r)
        {
            return new PointF(r.Left + r.Width / 2.0f, r.Top + r.Height / 2.0f);
        }

        public static Point LeftCenter(this Rectangle r)
        {
            return new Point(r.Left, r.Top + r.Height / 2);
        }

        public static PointF LeftCenter(this RectangleF r)
        {
            return new PointF(r.Left, r.Top + r.Height / 2.0f);
        }

        public static PointF RightCenter(this RectangleF r)
        {
            return new PointF(r.Right, r.Top + r.Height / 2.0f);
        }

        public static Matrix Inverse(this Matrix m)
        {
            var result = m.Clone();
            result.Invert();
            return result;
        }

        public static Point TransformPoint(this Matrix t, Point p)
        {
            Point[] points = new Point[] { p };
            t.TransformPoints(points);
            return points[0];
        }

        public static PointF TransformPoint(this Matrix t, PointF p)
        {
            PointF[] points = new PointF[] { p };
            t.TransformPoints(points);
            return points[0];
        }

        public static bool IsSet(this Keys a, Keys b)
        {
            return (a & Keys.KeyCode) == b;
        }

        public static void DoWithValue<T>(this Nullable<T> a, Action<T> action) where T : struct
        {
            if (a.HasValue)
                action(a.Value);
        }

        public static Point Round(this PointF p)
        {
            return new Point((int)p.X, (int)p.Y);
        }

        public static Rectangle Round(this RectangleF p)
        {
            return new Rectangle((int)p.X, (int)p.Y, (int)p.Width, (int)p.Height);
        }

        public static RectangleF ToRectangleF(this Rectangle r)
        {
            return new RectangleF(r.X, r.Y, r.Width, r.Height);
        }

        public static FileStream LoadFileStream(string path, FileMode mode, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.None)
        {
            FileStream s = null;
            FileLoadOperation(() => s = new FileStream(path, mode, access, share));
            return s;
        }

        [System.Diagnostics.DebuggerNonUserCode]
        public static FileStream LoadFileStream(FileInfo path, FileMode mode, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.None)
        {
            FileStream s = null;
            try
            {
                FileLoadOperation(() => s = path.Open(mode, access, share));
            }
            catch (MyFileLoadException)
            {
                throw;
            }
            return s;
        }

        public static void FileLoadOperation(Action @do)
        {
            try
            {
                @do();
            }
            //An amalgamation of possible excetions from FileInfo.Open and FileStream ctor
            catch (System.Security.SecurityException e) //The caller does not have the required permission
            { throw new MyFileLoadException(e); }
            catch (System.ArgumentNullException e) //One or more arguments is null
            { throw new MyFileLoadException(e); }
            catch (System.ArgumentException e) //path is empty or contains only white spaces or contains one or more invalid characters. -or-path refers to a non-file device
            { throw new MyFileLoadException(e); }
            catch (System.IO.FileNotFoundException e) //The file is not found
            { throw new MyFileLoadException(e); }
            catch (System.UnauthorizedAccessException e)//The access requested is not permitted by the operating system for the specified path, such as when access is Write or ReadWrite and the file or directory is set for read-only access
            { throw new MyFileLoadException(e); }
            catch (System.IO.DirectoryNotFoundException e) //The specified path is invalid, such as being on an unmapped drive
            { throw new MyFileLoadException(e); }
            catch (System.IO.PathTooLongException e) //The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters
            { throw new MyFileLoadException(e); }
            catch (System.IO.IOException e) //The file is already open; An I/O error, such as specifying FileMode.CreateNew when the file specified by path already exists, occurred. -or-The stream has been closed
            { throw new MyFileLoadException(e); }
            catch (System.NotSupportedException e) //path refers to a non-file device, such as "con:", "com1:", "lpt1:", etc. in a non-NTFS environment
            { throw new MyFileLoadException(e); }
        }
    }
}
