using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TankServer
{
    class Game
    {
        public int ID { get; set; }
        public DateTime Duration { get; set; }
        public string Winner { get; set; }
        public int PlayersCount { get; set; }
    }
}
