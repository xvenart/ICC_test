namespace TankGuiObserver2
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Game game = new Game("Battle city v0.1", 1920, 1080, true))
            {
                game.RunGame();
            }

        }
    }
}