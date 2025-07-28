using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Enums;
using ConsoleWorldRPG.Services;
using ConsoleWorldRPG.Systems;
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
        public void Start()
        {
            // Initialization logic here
            Console.WriteLine("Game is starting...");
            if (GameService.InitializeGame(ref _player, ref _playerExitst))
            {
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
                }
                _player.CurrentRoom.Describe();
                // Main loop
                while (_isRunning)
                {
                    Console.Write("> ");
                    string input = Console.ReadLine()?.Trim().ToLower();

                    if (string.IsNullOrEmpty(input))
                        continue;

                    HandleInput(input);
                }

            }

        }
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
        /// <summary>
        /// Handles user input
        /// </summary>
        /// <param name="input"></param>
        private void HandleInput(string input)
        {
            bool debug = false;
            if (_debug)
                debug = DebugCommandCheck(input);
            if (!debug)
            {
                if (input.StartsWith("move "))
                {
                    string direction = input.Substring(5).Trim();
                    Move(direction);
                }
                else if (input == "look")
                {
                    if (_player.CurrentRoom.HasMonsters && !_hallFought)
                    {
                        Console.WriteLine("A shadow moves in the corner...");
                        StartEncounter();
                        //_hallFought = true;
                    }
                    else if (_player.CurrentRoom.HasMonsters && _hallFought)
                    {
                        _player.CurrentRoom.Describe();
                        Console.WriteLine("The room seems to be empty...");
                    }
                    else
                        _player.CurrentRoom.Describe();
                }
                else if (input == "help")
                {
                    ShowHelp();
                }
                else if (input == "status")
                {
                    ShowStatus();
                }
                else if (input == "heal")
                {
                    Console.WriteLine("You take a break and heal all your wounds");
                    _player.CurrentHealth = _player.MaxHealth;
                }
                else if (input == "exit")
                {
                    SaveHero();
                    _isRunning = false;
                    Console.WriteLine("Thanks for playing!");
                }
                else if (input == "/debug")
                    _debug = true;
                else
                {
                    Console.WriteLine("Unknown command. Try \"help\".");
                }

            }

        }
        private void SaveHero()
        {
            string filePath = $"player_hero.json";
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            string jsonData = JsonSerializer.Serialize(_player, options);
            File.WriteAllText(filePath, jsonData);
            Console.WriteLine($"Hero saved to {filePath}");
        }
        private bool DebugCommandCheck(string input)
        {
            if (!input.StartsWith("/"))
                return false;
            if (input.Contains("level"))
            {
                if (input.Contains("set"))
                {
                    int newLevel = 0;
                    if (int.TryParse(input.Substring(10), out newLevel))
                    {
                        while (_player.Level < newLevel)
                        {
                            _player.LevelUp();
                        }
                        if (_player.Level > newLevel)
                            Console.WriteLine("player level can only be increased!");
                    }

                }

            }
            else
            {
                Console.WriteLine("Unknown command. Try \"help\".");
            }
            return true;
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
                _hallFought = false;
            }
            else
            {
                Console.WriteLine("You can't go that way.");
            }

        }
        private void ShowHelp()
        {
            Console.WriteLine("\nAvailable commands:");
            Console.WriteLine("  move <direction> - Move to another room (e.g., move north)");
            Console.WriteLine("  look             - Re-describe the current room");
            Console.WriteLine("  status           - Show your current health, mana, and stats");
            Console.WriteLine("  help             - Show this help message");
            Console.WriteLine("  exit             - Quit the game");
        }
        private void ShowStatus()
        {
            Console.WriteLine($"\n{_player.Name}'s Status:");
            Console.WriteLine($"  Level: {_player.Level}    Exp: {_player.Experience}/{_player.ExpForNextLvl}");
            Console.WriteLine($"  HP: {_player.CurrentHealth}/{_player.Stats.MaxHealth}");
            Console.WriteLine($"  Mana: {_player.CurrentMana}/{_player.Stats.MaxMana}");
            Console.WriteLine($"  STR: {_player.Stats.Strength}");
            Console.WriteLine($"  DEX: {_player.Stats.Dexterity}");
            Console.WriteLine($"  END: {_player.Stats.Endurance}");
            Console.WriteLine($"  INT: {_player.Stats.Intelligence}");
            Console.WriteLine($"  SPR: {_player.Stats.Spirit}");
            Console.WriteLine("\n");
            Console.WriteLine($"  ATK: {_player.Stats.PhysicalAttack}");
            Console.WriteLine($"  DEF: {_player.Stats.PhysicalDefense}");
            Console.WriteLine($"  MATK: {_player.Stats.MagicAttack}");
            Console.WriteLine($"  MDEF: {_player.Stats.MagicDefense}");
            Console.WriteLine($"  Crit: {_player.Stats.CritChance:P0}");
            Console.WriteLine($"  Block: {_player.Stats.BlockChance:P0}");
        }
        private void StartEncounter()
        {
            Random random = new Random();
            var encounters = _player.CurrentRoom.EncounterableMonsters;

            if (encounters == null || encounters.Count == 0)
            {
                Console.WriteLine("But nothing stirs in the darkness...");
                return;
            }

            Monster monster = _player.CurrentRoom.Monsters[random.Next(0, _player.CurrentRoom.Monsters.Count)];
            monster.ResetHealth();

            Console.WriteLine($"\nA wild {monster.Name} appears!");
            Console.WriteLine(monster.Description);

            while (_player.IsAlive && monster.IsAlive)
            {
                CombatSystem.Attack(_player, monster);
                if (!monster.IsAlive) break;

                CombatSystem.Attack(monster, _player);
            }

            if (_player.IsAlive)
            {
                Console.WriteLine($"\nYou defeated the {monster.Name}!");
                _player.Experience += monster.Exp;
                _player.CheckForLevelup();
            }
            else
                Console.WriteLine("\nYou were slain...");
        }

    }

}
