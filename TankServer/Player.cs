using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TankServer
{
    class Player
    {
        public int ID { get; set; }
        public string Tag { get; set; }
        public string Nickname { get; set; }
        public decimal MaximumHp { get; set; }
        public decimal Score { get; set; }
        public decimal BulletSpeed { get; set; }
        public decimal Damage { get; set; }
    }
}
