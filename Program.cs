namespace ConsoleWorldRPG
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "ConsoleRPG";
            Console.WriteLine($"Welcome to ConsoleRPG {VersionInfo.Current}!");
            Console.WriteLine("Type 'exit' at any time to quit.\n");

            Game game = new Game();
            game.Start();
        }

    }

}
