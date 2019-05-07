using TankCommon;
using TankCommon.Objects;

namespace TankServer
{
    public class ClientInfo
    {
        public bool IsLogined { get; set; }
        public bool IsSpecator { get; set; }
        public bool IsInQueue { get; set; }

        public bool NeedUpdateMap { get; set; }
        public bool NeedRemove { get; set; }

        public string Nickname { get; set; }
        public string Tag { get; set; }

        public BaseInteractObject InteractObject { get; set; }
        public ServerRequest Request { get; set; }
        public ServerResponse Response { get; set; }
    }
}
