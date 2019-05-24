using System;

namespace TankCommon.Objects
{
    public class MaxHpUpgradeObject : UpgradeInteractObject
    {
        public decimal IncreaseHP { get; }

        public MaxHpUpgradeObject()
        {
        }

        public MaxHpUpgradeObject(Guid id, Rectangle rectangle) : base(id, rectangle)
        {
            IncreaseHP = 25;
            Type = UpgradeType.MaxHp;
        }
    }
}
