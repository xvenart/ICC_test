using System;

namespace TankCommon.Objects
{
    public class Rectangle
    {
        public Point LeftCorner;

        public decimal Width;
        public decimal Height;

        public int WidthInt => (int)Math.Ceiling(Width);
        public int HeightInt => (int)Math.Ceiling(Height);

        public Rectangle()
        {
        }

        public Rectangle(Rectangle rectangle)
        {
            LeftCorner = new Point(rectangle.LeftCorner);
            Width = rectangle.Width;
            Height = rectangle.Height;
        }

        public Rectangle(Point leftCorner, decimal width, decimal height)
        {
            LeftCorner = leftCorner;
            Width = width;
            Height = height;
        }

        public bool IsRectangleIntersected(Rectangle rectangle)
        {
            var srcLeft = LeftCorner.LeftInt;
            var srcRight = srcLeft + Width - 1;
            var srcTop = LeftCorner.TopInt;
            var srcDown = srcTop + Height - 1;

            var dstLeft = rectangle.LeftCorner.LeftInt;
            var dstRight = dstLeft + rectangle.Width - 1;
            var dstTop = rectangle.LeftCorner.TopInt;
            var dstDown = dstTop + rectangle.Height - 1;

            if (srcLeft > dstRight || dstLeft > srcRight)
            {
                return false;
            }

            if (srcTop > dstDown || dstTop > srcDown)
            {
                return false;
            }

            return true;
        }

        public bool IsPointInRectange(Point dst)
        {
            var dstLeft = dst.LeftInt;
            var dstTop = dst.TopInt;

            var left = LeftCorner.LeftInt;
            var right = left + Width - 1;
            var top = LeftCorner.TopInt;
            var down = top + Height - 1;

            return dstLeft >= left && dstLeft <= right &&
                   dstTop >= top && dstTop <= down;
        }
    }
}
