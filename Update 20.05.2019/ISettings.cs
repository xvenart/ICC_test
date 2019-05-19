using System;
using TankCommon.Enum;
namespace TankCommon
{
    public interface ISettings
    {
        string ServerName { get; set; }
        ServerType ServerType { get; set; }
        TimeSpan SessionTime { get; set; }
        decimal GameSpeed { get; set; }
    }
}
