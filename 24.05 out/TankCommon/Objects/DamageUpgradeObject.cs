using System;

namespace TankCommon.Objects
{
    public class DamageUpgradeObject : UpgradeInteractObject
    {
        public int IncreaseDamage { get; }

        public DamageUpgradeObject()
        {
        }

        public DamageUpgradeObject(Guid id, Rectangle rectangle) : base(id, rectangle)
        {
            IncreaseDamage = 20;
            Type = UpgradeType.Damage;
        }
    }
}
