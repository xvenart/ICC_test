using System;

namespace TankCommon.Objects
{
    public class MaxHpUpgradeObject : UpgradeInteractObject
    {
        public decimal IncreaseHP { get; set; }

        public MaxHpUpgradeObject()
        {
        }

        public MaxHpUpgradeObject(Guid id, Rectangle rectangle, int increaseHP, int secondsToDespawn) : base(id, rectangle, secondsToDespawn)
        {
            IncreaseHP = increaseHP;
            Type = UpgradeType.MaxHp;
        }
    }
}
