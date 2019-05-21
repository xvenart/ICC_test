using System;

namespace TankClient
{
    public class ManualClient2 : ManualClient
    {
        public override ConsoleKey UpKey => ConsoleKey.UpArrow;
        public override ConsoleKey DownKey => ConsoleKey.DownArrow;
        public override ConsoleKey LeftKey => ConsoleKey.LeftArrow;
        public override ConsoleKey RightKey => ConsoleKey.RightArrow;
        public override ConsoleKey MovingKey => ConsoleKey.NumPad0;
        public override ConsoleKey FireKey => ConsoleKey.Decimal;
    }
}
