using System;

namespace TankCommon.Objects
{
    public class BulletSpeedUpgradeObject : UpgradeInteractObject
    {
        public int IncreaseBulletSpeed { get; }

        public BulletSpeedUpgradeObject()
        {
        }

        public BulletSpeedUpgradeObject(Guid id, Rectangle rectangle) : base(id, rectangle)
        {
            IncreaseBulletSpeed = 1;
            Type = UpgradeType.BulletSpeed;
        }
    }
}
