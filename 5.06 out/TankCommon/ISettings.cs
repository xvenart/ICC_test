using System;
using TankCommon.Enum;
using System.ComponentModel;

namespace TankCommon
{
    public interface ISettings
    {
        [Description("Начало игровой сессии")]
        DateTime StartSesison { get; set; }

        [Description("Конец игровой сессии")]
        DateTime FinishSesison { get; set; }

        [Description("Скорость игры")]
        int GameSpeed { get; set; }

        [Description("Время действия бонусов (ms)")]
        int TimeOfActionUpgrades { get; set; }

        [Description("Шанс появления бонусов")]
        double ChanceSpawnUpgrades { get; set; }

        [Description("Скорость танков")]
        decimal TankSpeed { get; set; } 

        [Description("Скорость пуль")]
        decimal BulletSpeed { get; set; } 

        [Description("Урон танков")]
        decimal TankDamage { get; set; } 

        [Description("Максимальный HP танков")]
        int TankMaxHP { get; set; } 

        [Description("Показатели бонуса увеличения скорости пуль")]
        int IncreaseBulletSpeed { get; set; } 

        [Description("Показатели бонуса увеличения урона танков")]
        int IncreaseDamage { get; set; }

        [Description("Показатели бонуса лечения")]
        int RestHP { get; set; } 

        [Description("Показатели бонуса неуязвимости (ms)")]
        int TimeOfInvulnerability { get; set; } 

        [Description("Показатели бонуса увеличения максимального количества HP")]
        int IncreaseHP { get; set; }

        [Description("Показатели бонуса увеличения скорости танка")]
        int IncreaseSpeed { get; set; }
    }
}
