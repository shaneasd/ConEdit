using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class Rectangle1DF
    {
        public Rectangle1DF(float top, float height)
        {
            Top = top;
            Height = height;
        }

        public float Top { get; set; }

        public float Height { get; set; }

        public float Bottom => Top + Height;
    }
}
