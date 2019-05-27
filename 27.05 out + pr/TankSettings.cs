using System;
using System.ComponentModel;
using TankCommon.Enum;

namespace TankCommon.Objects
{    
    public class TankSettings : ISettings
    {
        [Description("Скорость игры")]
        public int GameSpeed { get; set; } = 1;

        [Description("Время действия бонусов (ms)")]
        public int TimeOfActionUpgrades { get; set; } = 5000;

        [Description("Шанс появления бонусов")]
        public double ChanceSpawnUpgrades { get; set; } = 0.995; 

        [Description("Скорость танков")]
        public decimal TankSpeed { get; set; } = 2;

        [Description("Скорость пуль")]
        public decimal BulletSpeed { get; set; } = 4;

        [Description("Урон танков")]
        public decimal TankDamage { get; set; } = 40;

        [Description("Максимальный HP танков")]
        public int TankMaxHP { get; set; } = 125;

        [Description("Показатели бонуса увеличения скорости пуль")]
        public int IncreaseBulletSpeed { get; set; } = 1;

        [Description("Показатели бонуса увеличения урона танков")]
        public int IncreaseDamage { get; set; } = 20;

        [Description("Показатели бонуса лечения")]
        public int RestHP { get; set; } = 25;

        [Description("Показатели бонуса неуязвимости (ms)")]
        public int TimeOfInvulnerability { get; set; } = 5000;

        [Description("Показатели бонуса увеличения максимального количества HP")]
        public int IncreaseHP { get; set; } = 125;

        [Description("Показатели бонуса увеличения скорости танка")]
        public int IncreaseSpeed { get; set; } = 1;
    }
}
