using System;

namespace TankCommon.Objects
{
    public class HealthObject : BaseInteractObject
    {
        public decimal RestHP { get; set; }
        public DateTime SpawnTime { get; set; }
        public DateTime DespawnTime { get; set; }

        public HealthObject()
        {
        }

        public HealthObject(Guid id, Rectangle rectangle) : base(id, rectangle)
        {
            RestHP = 25;
            SpawnTime = DateTime.Now;
            DespawnTime = SpawnTime.AddSeconds(30);
        }
    }
}
