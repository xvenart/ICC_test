using System.ComponentModel;

namespace TankCommon.Objects
{
    public class ServerSettings
    {
        [Description("Имя сервера")]
        public string ServerName { get; set; }

        [Description("Порт")]
        public int Port { get; set; } = 1000;

        [Description("Тип шаблона карты")]
        public Enum.MapType MapType { get; set; }

        [Description("Ширина карты")]
        public int Width { get; set; } = 20;

        [Description("Высота карты")]
        public int Height { get; set; } = 20;

        [Description("Максимальное количество клиентов")]
        public uint MaxClientCount { get; set; } = 10;

        [Description("Количество жизней танков")]
        public int CountOfLifes { get; set; } = 5;

        [Description("Время неуязвимости после перерождения")]
        public int TimeOfInvulnerabilityAfterRespawn { get; set; } = 5;

        [Description("Максимальное количество бонусов на карте")]
        public int MaxCountOfUpgrade { get; set; } = 3;

        [Description("Тип сервера")]
        public Enum.ServerType ServerType { get; set; }

        [Description("Класс настройки темпа игры")]
        public TankSettings TankSettings { get; set; } = new TankSettings();
    }
}
