namespace ConsoleWorldRPG
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            string version = File.ReadAllText("version.txt").Trim();
            Console.Title = "ConsoleRPG";
            Console.WriteLine($"Welcome to ConsoleRPG {version}!");
            Console.WriteLine("Type 'exit' at any time to quit.\n");

            Game game = new Game();
            game.Start();
        }

    }

}
