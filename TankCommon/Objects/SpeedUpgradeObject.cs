using System;

namespace TankCommon.Objects
{
    public class SpeedUpgradeObject : UpgradeInteractObject
    {
        public int IncreaseSpeed { get; }

        public SpeedUpgradeObject()
        {
        }

        public SpeedUpgradeObject(Guid id, Rectangle rectangle) : base(id, rectangle)
        {
            IncreaseSpeed = 1;
            Type = UpgradeType.Speed;
        }
    }
}
