using System;
using TankCommon.Enum;

namespace TankCommon.Objects
{
    public abstract class BaseMovingObject : BaseInteractObject
    {
        public decimal Speed { get; set; }
        public bool IsMoving { get; set; }

        public DirectionType Direction { get; set; }

        public BaseMovingObject()
        {
        }

        protected BaseMovingObject(Guid id, Rectangle rectangle, decimal speed, bool isMoving, DirectionType direction)
            : base(id, rectangle)
        {
            Speed = speed;
            IsMoving = isMoving;
            Direction = direction;
        }
    }
}
