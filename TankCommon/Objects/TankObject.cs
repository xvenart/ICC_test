using System;
using TankCommon.Enum;

namespace TankCommon.Objects
{
    public class TankObject : BaseMovingObject
    {
        public string Tag { get; set; }
        public string Nickname { get; set; }
        public decimal MaximumHp { get; set; }
        public decimal Hp { get; set; }
        public decimal Score { get; set; }
        public decimal BulletSpeed { get; set; }
        public decimal Damage { get; set; }

        public TankObject()
        {
        }

        public TankObject(Guid id, Rectangle rectangle, decimal speed, bool isMoving, decimal maximumHp, decimal hp, string nickname, string tag, decimal damage)
            : base(id, rectangle, speed, isMoving, DirectionType.Left)
        {
            MaximumHp = maximumHp;
            Hp = hp;
            Tag = tag;
            Nickname = nickname;
            Score = 0;
            BulletSpeed = 7;
            Damage = damage;
        }
    }
}
