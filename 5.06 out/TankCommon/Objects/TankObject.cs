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
        public int MaximumLives { get; set; }
        public int Lives { get; set; }
        public decimal Score { get; set; }
        public decimal BulletSpeed { get; set; }
        public decimal Damage { get; set; }
        public bool IsDead { get; set; }
        public bool IsInvulnerable { get; set; } = false;

        public TankObject()
        {
        }

        public TankObject(Guid id, Rectangle rectangle, decimal speed, bool isMoving, decimal maximumHp, decimal hp, int maximumLives, int lives, string nickname, string tag, decimal damage, decimal bulletSpeed)
            : base(id, rectangle, speed, isMoving, DirectionType.Left)
        {
            MaximumHp = maximumHp;
            Hp = hp;
            MaximumLives = maximumLives;
            Lives = lives;
            Tag = tag;
            Nickname = nickname;
            Score = 0;
            BulletSpeed = bulletSpeed;
            Damage = damage;
            IsDead = false;

        }
    }
}
