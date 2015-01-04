using System;
using System.Diagnostics;
using System.Text;
using Windows.Devices.Geolocation;



// From: http://msdn.microsoft.com/en-us/library/bb259689.aspx

namespace PhoneApp2
{


    public class Rectangle
    {
        private int x, y;
        private uint width, height;

        public Rectangle(int x, int y, uint width, uint height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public int X
        {
            get { return x; }
        }

        public int Y
        {
            get { return y; }
        }

        public uint Width
        {
            get { return width; }
        }

        public uint Height
        {
            get { return height; }
        }

        public override bool Equals(Object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Rectangle r = (Rectangle)obj;
            return (x == r.x) && (y == r.y) && (width == r.width) && (height == r.height);
        }
        public override int GetHashCode()
        {
            return x ^ y ^ width.GetHashCode() ^ height.GetHashCode();
        }

        // Returns true if any of 'child' is within 'this' retangle
        //   ------------------
        //  |this   ___________|_______
        //  |      | child             |
        //  |______|                   |
        //         |___________________|
        //
        internal bool Contains(Rectangle child)
        {
            return ((child.x + child.width) >= this.x) &&
                    (child.x <= (this.x + this.width)) &&
                    ((child.y + child.height) >= this.y) &&
                    (child.y <= (this.y + this.height));
        }
    }
}
