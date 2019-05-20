namespace TankCommon
{
    using System;
    using TankCommon.Enum;
    public class TankSettings : ISettings
    {
        public int Version { get; set; }
        public string ServerName { get; set; }
        public ServerType ServerType { get; set; }
        public TimeSpan SessionTime { get; set; }
        public decimal GameSpeed { get; set; }
        public decimal TankSpeed { get; set; }
        public decimal BulletSpeed { get; set; }
        public decimal TankDamage { get; set; }

        public TankSettings()
        {
            Version++;
            ServerName = null;
            ServerType = ServerType.BattleCity;
            SessionTime = DateTime.Now.AddMinutes(2).AddSeconds(1) - DateTime.Now;  
            GameSpeed = 1;
            TankSpeed = 2;
            BulletSpeed = 4;
            TankDamage = 40;
        }

        public void UpdateAll(string _serverName, string _serverType, string _sessionTime, decimal _gameSpeed, decimal _tankSpeed, decimal _bulletSpeed, decimal _tankDamage)
        {
            Version++;
            if (_serverName == "")
            {
                _serverName = null;
            }
            ServerName = _serverName;
            ServerType = (ServerType)System.Enum.Parse(typeof(ServerType), _serverType);
            SessionTime = TimeSpan.Parse(_sessionTime);
            GameSpeed = _gameSpeed;
            TankSpeed = _tankSpeed;
            BulletSpeed = _bulletSpeed;
            TankDamage = _tankDamage;
        }
    }
}
