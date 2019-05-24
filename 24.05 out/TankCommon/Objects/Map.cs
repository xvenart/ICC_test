using System;
using System.Collections.Generic;
using TankCommon.Enum;

namespace TankCommon.Objects
{
    public class Map
    {
        public CellMapType[,] Cells;
        public List<BaseInteractObject> InteractObjects;

        public int MapWidth;
        public int MapHeight;

        public int CellWidth;
        public int CellHeight;

        public CellMapType this[int top, int left]
        {
            get
            {
                if (left < 0 || left >= MapWidth)
                {
                    throw new ArgumentOutOfRangeException(nameof(left));
                }

                if (top < 0 || top >= MapHeight)
                {
                    throw new ArgumentOutOfRangeException(nameof(top));
                }

                return Cells[top, left];
            }
        }

        public Map()
        {
        }

        public Map(CellMapType[,] cells)
        {
            Cells = cells;

            if (cells != null)
            {
                MapWidth = Cells.GetLength(1);
                MapHeight = Cells.GetLength(0);
            }

            CellWidth = Constants.CellWidth;
            CellHeight = Constants.CellHeight;

            InteractObjects = new List<BaseInteractObject>();
        }

        public Map(Map map, List<BaseInteractObject> interactObjects)
            : this(null == map ? null : (CellMapType[,])map.Cells.Clone())
        {
            InteractObjects = new List<BaseInteractObject>(interactObjects);
        }
    }
}
