using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleWorldRPG
{
    public class Game
    {
        private bool _isRunning = true;

        public void Start()
        {
            // Initialization logic here
            Console.WriteLine("Game is starting...\n");

            // Main loop
            while (_isRunning)
            {
                Console.Write("> ");
                string input = Console.ReadLine()?.Trim().ToLower();

                if (string.IsNullOrEmpty(input))
                    continue;

                if (input == "exit")
                {
                    _isRunning = false;
                    Console.WriteLine("Thanks for playing!");
                    break;
                }

                // For now, just echo input
                Console.WriteLine($"You typed: {input}");

                // Later: pass input to GameState or Command system
            }

        }

    }

}
