using System;

namespace TankCommon.Objects
{
    public class HealthUpgradeObject : UpgradeInteractObject
    {
        public decimal RestHP { get; }

        public HealthUpgradeObject()
        {
        }

        public HealthUpgradeObject(Guid id, Rectangle rectangle) : base(id, rectangle)
        {
            RestHP = 25;
            Type = UpgradeType.Health;
        }
    }
}
