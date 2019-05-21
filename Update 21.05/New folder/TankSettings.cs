namespace TankCommon
{
    using System;
    using TankCommon.Enum;
    public class TankSettings : ISettings
    {
        public int Version { get; set; }
        public string ServerName { get; set; } = null;
        public ServerType ServerType { get; set; } = ServerType.BattleCity;
        public TimeSpan SessionTime { get; set; } = new TimeSpan(0, 2, 0);
        public DateTime StartSession { get; set; } = DateTime.Now;
        public DateTime FinishSession { get; set; } = DateTime.Now + new TimeSpan(0, 2, 0);
        public decimal GameSpeed { get; set; } = 1;
        public decimal TankDamage { get; set; } = 40;
        public decimal CoefficientOfSpeed { get; set; } = 1;
        public decimal CountOfLife { get; set; } = 3;

        public TankSettings()
        {
            Version++;
        }

        public void UpdateAll(string serverName, string serverType, string sessionTime, decimal gameSpeed, decimal tankDamage, decimal coefficientOfSpeed, decimal countOfLife)
        {
            Version++;
            if (serverName == "")
            {
                serverName = null;
            }
            ServerName = serverName;
            ServerType = (ServerType)System.Enum.Parse(typeof(ServerType), serverType);
            SessionTime = TimeSpan.TryParse(sessionTime, out var value) ? value : new TimeSpan(0, 2, 0);
            StartSession = DateTime.Now;
            FinishSession = StartSession + SessionTime;
            GameSpeed = gameSpeed;
            TankDamage = tankDamage;
            CoefficientOfSpeed = coefficientOfSpeed;
            CountOfLife = countOfLife;
        }
    }
}
