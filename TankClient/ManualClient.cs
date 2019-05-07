using System;
using TankCommon;
using TankCommon.Enum;

namespace TankClient
{
    public class ManualClient : IClientBot
    {
        public virtual ConsoleKey LeftKey => ConsoleKey.A;
        public virtual ConsoleKey RightKey => ConsoleKey.D;
        public virtual ConsoleKey UpKey => ConsoleKey.W;
        public virtual ConsoleKey DownKey => ConsoleKey.S;
        public virtual ConsoleKey FireKey => ConsoleKey.R;
        public virtual ConsoleKey MovingKey => ConsoleKey.E;

        public ServerResponse Client(int msgCount, ServerRequest request)
        {
            var response = new ServerResponse
            {
                ClientCommand = ClientCommandType.None
            };

            var tank = request.Tank;

            if (null == tank)
            {
                return response;
            }

            ClientCommandType? definedCmd = null;

            if (Program.Keys.Count > 0)
            {
                var c = Program.Keys.Peek();

                if (c == UpKey)
                {
                    definedCmd = ClientCommandType.TurnUp;
                }
                else if (c == DownKey)
                {
                    definedCmd = ClientCommandType.TurnDown;
                }
                else if (c == LeftKey)
                {
                    definedCmd = ClientCommandType.TurnLeft;
                }
                else if (c == RightKey)
                {
                    definedCmd = ClientCommandType.TurnRight;
                }
                else if (c == MovingKey)
                {
                    definedCmd = tank.IsMoving ? ClientCommandType.Stop : ClientCommandType.Go;
                }
                else if (c == FireKey)
                {
                    definedCmd = ClientCommandType.Fire;
                }

                Program.Keys.Dequeue();
            }

            response.ClientCommand = definedCmd ?? ClientCommandType.None;
            return response;
        }
    }
}
