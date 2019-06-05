using System;
using System.Threading;
using System.Threading.Tasks;

namespace AdminPanel.Entity
{
    public class ServerEntity
    {
        public int Id { get; set; }
        public uint Port { get; set; }
        public Task Task { get; set; }
        public TankServer.Server Server { get; set; }
    }
}
