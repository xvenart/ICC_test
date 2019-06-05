using System;

namespace TankCommon.Objects
{
    public class UpgradeInteractObject : BaseInteractObject
    {
        public DateTime SpawnTime { get; }
        public DateTime DespawnTime { get; }
        public UpgradeType Type { get; set; }

        public UpgradeInteractObject()
        {
        }

        public UpgradeInteractObject(Guid id, Rectangle rectangle, int secondsToDespawn) : base(id, rectangle)
        {
            SpawnTime = DateTime.Now;
            DespawnTime = SpawnTime.AddSeconds(secondsToDespawn);
        }
    }
}
