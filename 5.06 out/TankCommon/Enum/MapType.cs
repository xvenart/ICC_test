using System.ComponentModel;

namespace TankCommon.Enum
{
    public enum MapType
    {
        [Description("Стандартная карта")]
        Default_map,
        [Description("Пустая карта")]
        Empty_map,
        [Description("Водная карта")]
        Water_map,
        [Description("Рукотворная карта 1")]
        Manual_Map_1,
        [Description("Рукотворная карта 2")]
        Manual_Map_2,
        [Description("Рекламная карта")]
        Promotional,
        [Description("Карта для тестов")]
        Test_Map
    }
}
