using TankCommon;

namespace TrukhinaClient
{
    public interface IClientBot
    {
        ServerResponse Client(int msgCount, ServerRequest request);
    }
}