using TankCommon;

namespace TankClient
{
    public interface IClientBot
    {
        ServerResponse Client(int msgCount, ServerRequest request);
    }
}
