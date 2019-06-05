using System;

namespace TankCommon.Objects
{
    public class HealthUpgradeObject : UpgradeInteractObject
    {
        public decimal RestHP { get; set; }

        public HealthUpgradeObject()
        {
        }

        public HealthUpgradeObject(Guid id, Rectangle rectangle, int restHP, int secondsToDespawn) : base(id, rectangle, secondsToDespawn)
        {
            RestHP = restHP;
            Type = UpgradeType.Health;
        }
    }
}
