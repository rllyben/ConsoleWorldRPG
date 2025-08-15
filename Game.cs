using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Entities.NPCs;
using ConsoleWorldRPG.Enums;
using ConsoleWorldRPG.Items;
using ConsoleWorldRPG.Models;
using ConsoleWorldRPG.Services;
using ConsoleWorldRPG.Systems;
using ConsoleWorldRPG.Utils;
using Microsoft.VisualBasic;

namespace ConsoleWorldRPG
{
    public class Game
    {
        private bool _isRunning = true;
        private Player _player = new Player("Hero", new Stats());
        CommandRouter commandRouter;
        /// <summary>
        /// Handels the Main User interaction and calles the initialatiation of the Game
        /// </summary>
        public void Start()
        {
            // Initialization logic here
            Console.WriteLine("Game is starting...");
            if (GameService.InitializeGame(_player))
            {
                _player = ShowLoginMenu();
                if (_player == null)
                    _isRunning = false;

                if (_isRunning)
                {
                    commandRouter = new CommandRouter(_player);
                    _player.CurrentRoom.Describe();
                }
                // Main loop
                while (_isRunning)
                {
                    Console.Write("> ");
                    string? input = Console.ReadLine()?.Trim();

                    if (string.IsNullOrEmpty(input)) continue;
                    if (input == "exit")
                    {
                        JsonSaveService.SaveCharacter(LoginManager.UserAccount, _player);
                        _isRunning = false;
                        Console.WriteLine("Thanks for playing!");
                    }
                    else if (!commandRouter.HandleCommand(input))
                        Console.WriteLine("❌ Unknown command.");
                }

            }

        }
        public static Player ShowLoginMenu()
        {
            while (true)
            {
                Console.WriteLine("\n==== Login Menu ====");
                Console.WriteLine("1. Login");
                Console.WriteLine("2. Register");
                Console.WriteLine("3. Exit");
                Console.Write("Select an option: ");

                string? input = Console.ReadLine()?.Trim();

                switch (input)
                {
                    case "1":
                        var user = LoginManager.Login();
                        if (user != null)
                            return user;
                        break;

                    case "2":
                        LoginManager.Register();
                        break;

                    case "3":
                        return null;

                    default:
                        Console.WriteLine("❌ Invalid option.");
                        break;
                }

            }

        }

    }

}
