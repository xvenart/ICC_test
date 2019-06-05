using System.ComponentModel;

namespace TankCommon.Objects
{
    public class ServerSettings
    {
        [Description("Имя сервера")]
        public string SessionName { get; set; }

        [Description("Порт")]
        public int Port { get; set; } = 2000;

        [Description("Тип шаблона карты")]
        public Enum.MapType MapType { get; set; } = Enum.MapType.Empty_map;

        [Description("Ширина карты")]
        public int Width { get; set; } = 20;

        [Description("Высота карты")]
        public int Height { get; set; } = 20;

        [Description("Максимальное количество клиентов")]
        public uint MaxClientCount { get; set; } = 10;

        [Description("Количество жизней танков")]
        public int CountOfLifes { get; set; } = 5;

        [Description("Время неуязвимости после перерождения")]
        public int TimeOfInvulnerabilityAfterRespawn { get; set; } = 5000;

        [Description("Максимальное количество бонусов на карте")]
        public int MaxCountOfUpgrade { get; set; } = 3;

        [Description("Тип сервера")]
        public Enum.ServerType ServerType { get; set; } = Enum.ServerType.BattleCity;

        [Description("Время до деспауна бонусов")]
        public int SecondsToDespawn { get; set; } = 30;

        [Description("Разрешить несколько клиентов на 1 ip")]
        public bool IsMultipleConnectionAllow { get; set; } = false;

        [Description("Класс настройки темпа игры")]
        public TankSettings TankSettings { get; set; } = new TankSettings();
        [Description("Задержка прощетов на сервере в ms")]
        public int ServerTickRate { get; set; }
        [Description("Задержка отсылки данных клиентам в ms")]
        public int PlayerTickRate { get; set; }
        [Description("Задержка отсылки данных зрителям в ms")]
        public int SpectatorTickRate { get; set; }
    }
}
