using System;
using TankCommon;
using TankCommon.Enum;
using TankCommon.Objects;

namespace TankClient
{
    public class TestClient : IClientBot
    {
        protected readonly Random _random = new Random();
        protected Rectangle rectangle;
        protected Map _map;

        protected ClientCommandType SwitchDirection()
        {
            var rndNum = _random.Next(4);
            switch (rndNum)
            {
                case 0:
                    return ClientCommandType.TurnLeft;
                case 1:
                    return ClientCommandType.TurnRight;
                case 2:
                    return ClientCommandType.TurnUp;
                default:
                    return ClientCommandType.TurnDown;
            }
        }

        public ServerResponse Client(int msgCount, ServerRequest request)
        {
            if (request.Map.Cells != null)
            {
                _map = request.Map;
            }
            else if (null == _map)
            {
                return new ServerResponse { ClientCommand = ClientCommandType.UpdateMap };
            }

            _map.InteractObjects = request.Map.InteractObjects;

            var tank = request.Tank;
            if (null == tank)
            {
                return new ServerResponse {ClientCommand = ClientCommandType.None};
            }

            if (!tank.IsMoving)
            {
                return new ServerResponse {ClientCommand = ClientCommandType.Go};
            }

            if (null == rectangle)
            {
                rectangle = tank.Rectangle;
                return new ServerResponse {ClientCommand = SwitchDirection()};
            }

            if (rectangle.LeftCorner.Equals(tank.Rectangle.LeftCorner))
            {
                return new ServerResponse {ClientCommand = SwitchDirection()};
            }

            rectangle = tank.Rectangle;

            var rndNum = _random.NextDouble();

            if (rndNum > 0.9)
            {
                return new ServerResponse {ClientCommand = SwitchDirection()};
            }

            return new ServerResponse {ClientCommand = ClientCommandType.Fire};
        }
    }
}
