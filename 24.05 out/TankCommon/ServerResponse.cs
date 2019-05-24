using System;
using TankCommon.Enum;

namespace TankCommon
{
    [Serializable]
    public class ServerResponse
    {
        public ClientCommandType ClientCommand { get; set; }
        public string CommandParameter { get; set; }
    }
}
