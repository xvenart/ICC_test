namespace TankCommon
{
    using System;
    using TankCommon.Enum;
    interface ISettings
    {
        string ServerName { get; set; }
        ServerType ServerType { get; set; }
        TimeSpan SessionTime { get; set; }
        DateTime StartSession { get; set; }
        DateTime FinishSession { get; set; }
        decimal CoefficientOfSpeed { get; set; }
        decimal GameSpeed { get; set; }
        decimal CountOfLife { get; set; }
    }
}
