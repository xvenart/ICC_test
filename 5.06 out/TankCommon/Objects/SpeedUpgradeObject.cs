using System;

namespace TankCommon.Objects
{
    public class SpeedUpgradeObject : UpgradeInteractObject
    {
        public int IncreaseSpeed { get; set; }

        public SpeedUpgradeObject()
        {
        }

        public SpeedUpgradeObject(Guid id, Rectangle rectangle, int increaseSpeed, int secondsToDespawn) : base(id, rectangle, secondsToDespawn)
        {
            IncreaseSpeed = increaseSpeed;
            Type = UpgradeType.Speed;
        }
    }
}
