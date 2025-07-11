using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Services;
using ConsoleWorldRPG.Systems;

namespace ConsoleWorldRPG
{
    public class Game
    {
        private bool _isRunning = true;
        private Player _player = new Player("Hero", new Stats());
        public void Start()
        {
            // Initialization logic here
            Console.WriteLine("Game is starting...\n");
            GameService.InitializeGame(_player);
            
            _player.CurrentRoom.Describe();
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

            }

        }
        /// <summary>
        /// Handles user input
        /// </summary>
        /// <param name="input"></param>
        private void HandleInput(string input)
        {
            if (input.StartsWith("move "))
            {
                string direction = input.Substring(5).Trim();
                Move(direction);
            }
            else if (input == "look")
            {
                _player.CurrentRoom.Describe();
            }
            else
            {
                Console.WriteLine("Unknown command. Try 'move <direction>' or 'look'.");
            }

        }
        /// <summary>
        /// Moves the player in the specified direction or room
        /// </summary>
        /// <param name="direction"></param>
        private void Move(string direction)
        {
            if (_player.CurrentRoom.Exits.TryGetValue(direction, out Room nextRoom))
            {
                _player.CurrentRoom = nextRoom;
                Console.WriteLine($"\nYou move {direction}.");
                _player.CurrentRoom.Describe();
            }
            else
            {
                Console.WriteLine("You can't go that way.");
            }

        }

    }

}
