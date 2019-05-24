using System;
using TankCommon.Enum;

namespace TankCommon.Objects
{
    public class BulletObject : BaseMovingObject
    {
        public Guid SourceId { get; set; }
        public decimal DamageHp { get; set; }

        public BulletObject()
        {
        }

        public BulletObject(Guid id, Rectangle rectangle, decimal speed, bool isMoving, DirectionType direction, Guid sourceId, decimal damageHp)
            : base(id, rectangle, speed, isMoving, direction)
        {
            SourceId = sourceId;
            DamageHp = damageHp;
        }
    }
}
