using System;
using System.ComponentModel;
using TankCommon.Enum;
namespace TankCommon
{
    //Интерфейс настроек игры
    interface ISettings
    {
        string ServerName { get; set; }
        ServerType ServerType { get; set; }
        TimeSpan SessionTime { get; set; }
        decimal GameSpeed { get; set; }
    }
}
