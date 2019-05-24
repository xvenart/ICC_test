using System;
using TankCommon.Objects;

namespace TankCommon
{
    [Serializable]
    public class ServerRequest
    {
        public Map Map { get; set; }
        public TankObject Tank { get; set; }
        public bool IsSettingsChanged { get; set; } = false;
        public TankSettings Settings { get; set; }
    }
}
