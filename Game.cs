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
        private bool _hallFought = false;
        private bool _debug = false;
        private bool _playerExitst = false;
        CommandRouter commandRouter;
        /// <summary>
        /// Handels the Main User interaction and calles the initialatiation of the Game
        /// </summary>
        public void Start()
        {
            // Initialization logic here
            Console.WriteLine("Game is starting...");
            if (GameService.InitializeGame(ref _player, ref _playerExitst))
            {
                commandRouter = new CommandRouter(_player);
                if (!_playerExitst)
                {
                    Console.WriteLine("no player found!");
                    Console.WriteLine();
                    Console.Write("Choose player name:");
                    _player.Name = Console.ReadLine();
                    Console.WriteLine($"Player name is set to {_player.Name}");
                    Console.WriteLine();
                    Console.WriteLine("Choose an player class");
                    Console.WriteLine("Classes:");
                    foreach(PlayerClass klasse in ClassProfile.All.Keys)
                    {
                        Console.WriteLine(klasse);
                    }
                    Console.Write("> ");
                    string input = Console.ReadLine()?.Trim().ToLower();
                    HandleClassSelection(input);
                    Console.WriteLine($"Player Class is set to {_player.Class}");
                    _player.CurrentRoom = GameService.rooms[1];
                }
                _player.CurrentRoom.Describe();

                // Main loop
                while (_isRunning)
                {
                    Console.Write("> ");
                    string? input = Console.ReadLine()?.Trim();

                    if (string.IsNullOrEmpty(input)) continue;
                    if (input == "exit")
                    {
                        GameService.SaveHero(ref _player);
                        _isRunning = false;
                        Console.WriteLine("Thanks for playing!");
                    }
                    if (!commandRouter.HandleCommand(input))
                        Console.WriteLine("❌ Unknown command.");
                }

            }

        }
        /// <summary>
        /// Handles the Class selection of the Player
        /// </summary>
        /// <param name="input"></param>
        private void HandleClassSelection(string input)
        {
            switch (input)
            {
                case "archer": _player.Class = PlayerClass.Archer; break;
                case "arcanmage": _player.Class = PlayerClass.ArcanMage; break;
                case "barbarian": _player.Class = PlayerClass.Barbarian; break;
                case "cleric": _player.Class = PlayerClass.Cleric; break;
                case "druid": _player.Class = PlayerClass.Druid; break;
                case "elementalmage": _player.Class = PlayerClass.ElementalMage; break;
                case "fighter": _player.Class = PlayerClass.Fighter; break;
                case "hunter": _player.Class = PlayerClass.Hunter; break;
                case "knight": _player.Class = PlayerClass.Knight; break;
                case "rogue": _player.Class = PlayerClass.Rogue; break;
                case "soulsknight": _player.Class = PlayerClass.SoulsKnight; break;
            }

        }

    }

}
