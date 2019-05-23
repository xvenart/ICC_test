using System;
using System.ComponentModel;
using TankCommon.Enum;
namespace TankCommon
{
    //Интерфейс настроек игры
    interface ISettings
    {
        //Имя сервера
        string ServerName { get; set; }

        //тип сервера
        ServerType ServerType { get; set; }

        //Длина сессии
        TimeSpan SessionTime { get; set; }

        //Время начала игровой сессии
        DateTime StartSession { get; set; }

        //Время окончания игровой сессии
        DateTime FinishSession { get; set; }

        //Скорость игры
        [Description("Коэффициент скорости игры")]
        int GameSpeed { get; set; }

        //Количество жизней
        int CountOfLife { get; set; }
    }
}
