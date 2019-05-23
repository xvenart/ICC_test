using System;
using System.ComponentModel;
using TankCommon.Enum;
namespace TankCommon
{
    //Класс для настройки BattleCity
    public class TankSettings : ISettings
    {
        //Версия
        public int Version { get; set; }

        //Имя сервера
        public string ServerName { get; set; } = null;

        //Тип сервера (для какой игры)
        public ServerType ServerType { get; set; } = ServerType.BattleCity;

        //Длительность игровой сессии
        public TimeSpan SessionTime { get; set; } = new TimeSpan(0, 2, 0);

        //Время начала сессии
        public DateTime StartSession { get; set; } = DateTime.Now;

        //Время окончания сессии
        public DateTime FinishSession { get; set; } = DateTime.Now + new TimeSpan(0, 2, 0);

        //Скорость игры
        [Description("Коэффициент скорости игры")]
        public int GameSpeed { get; set; } = 1;

        //Скорость танков
        public int TankSpeed { get; set; } = 2;

        //Скорость пуль
        public int BulletSpeed { get; set; } = 4;

        //Количество жизней
        public int CountOfLife { get; set; } = 3;

        //Урон танков
        public int TankDamage { get; set; } = 40;

        //Флаг на изменения
        public bool IsSettingsChanged { get; set; } = false;

        //Конструктор по умолчанию - увеличивает версию
        public TankSettings()
        {
            Version++;
        }

        public void UpdateAll(string serverName, string sessionTime, int gameSpeed, int tankSpeed, int bulletSpeed, int countOfLife, int tankDamage)
        {
            Version++;
            ServerName = (serverName == "") ? null : serverName;
            SessionTime = TimeSpan.TryParse(sessionTime, out var value) ? value : new TimeSpan(0, 2, 0);
            StartSession = DateTime.Now;
            FinishSession = StartSession + SessionTime;
            GameSpeed = gameSpeed;
            TankSpeed = tankSpeed;
            BulletSpeed = bulletSpeed;
            CountOfLife = countOfLife;
            TankDamage = tankDamage;
            IsSettingsChanged = true;
        }

        //Обновление имени сервера
        public void UpdateServerName(string serverName)
        {
            ServerName = serverName;
            IsSettingsChanged = true;
        }

        //Обновление длины сессии
        public void UpdateSessionTime(string sessionTime)
        {
            SessionTime = TimeSpan.TryParse(sessionTime, out var value) ? value : new TimeSpan(0, 2, 0);
            FinishSession = StartSession + SessionTime;
            IsSettingsChanged = true;
        }

        //Обновление скорости/коэффициента игры
        public void UpdateGameSpeed(int gameSpeed)
        {
            GameSpeed = gameSpeed;
            IsSettingsChanged = true;
        }

        //Обновление скорости танков
        public void UpdateTankSpeed(int tankSpeed)
        {
            TankSpeed = tankSpeed;
            IsSettingsChanged = true;
        }

        //Обновление скорости пуль
        public void UpdateBulletSpeed(int bulletSpeed)
        {
            BulletSpeed = bulletSpeed;
            IsSettingsChanged = true;
        }

        //Обновление количества жизней
        public void UpdateCountOfLife(int countOfLife)
        {
            CountOfLife = countOfLife;
            IsSettingsChanged = true;
        }

        //Обновление урона танков
        public void UpdateTankDamage(int tankDamage)
        {
            TankDamage = tankDamage;
            IsSettingsChanged = true;
        }

        /*//Метод для сравнения двух экземпляров настроек
        public bool Equals(TankSettings other)
        {
            if (other == null)
            {
                return false;
            }

            return (ServerName == other.ServerName &&
                SessionTime == other.SessionTime &&
                GameSpeed == other.GameSpeed &&
                TankSpeed == other.TankSpeed &&
                BulletSpeed == other.BulletSpeed &&
                CountOfLife == other.CountOfLife &&
                TankDamage == other.TankDamage);
        }*/
    }
}
