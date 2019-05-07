using System;

namespace TankCommon.Objects
{
    public class SpectatorObject : BaseInteractObject
    {
        public SpectatorObject()
        {
        }

        public SpectatorObject(Guid id)
            : base(id, new Rectangle(new Point(0, 0), 0, 0))
        {
        }
    }
}
