using System;

namespace TankCommon.Objects
{
    public class BaseInteractObject
    {
        public Guid Id { get; set; }
        public Rectangle Rectangle { get; set; }

        public BaseInteractObject()
        {
        }

        protected BaseInteractObject(Guid id, Rectangle rectangle)
        {
            Id = id;
            Rectangle = rectangle;
        }
    }
}
