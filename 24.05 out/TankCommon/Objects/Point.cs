using System;

namespace TankCommon.Objects
{
    public class Point
    {
        public decimal Left;
        public decimal Top;

        public int LeftInt => (int)Left;
        public int TopInt => (int)Top;

        public Point()
        {
        }

        public Point(decimal left, decimal top)
        {
            Left = left;
            Top = top;
        }

        public Point(Point p)
        {
            Left = p.Left;
            Top = p.Top;
        }

        public override int GetHashCode()
        {
            return Left.GetHashCode() * Top.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Point pObj))
            {
                return false;
            }

            return Left == pObj.Left && Top == pObj.Top;
        }

        public override string ToString()
        {
            return $"L: {Left}, T: {Top};";
        }
    }
}
