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
        private Random _random = new Random();
        private NpcInteractionHandler _npc = new NpcInteractionHandler();
        private Printer _printer = new Printer();
        /// <summary>
        /// Handels the Main User interaction and calles the initialatiation of the Game
        /// </summary>
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
                    _printer.ShowHelp();
                }
                else if (input == "status")
                {
                    _printer.ShowStatus(_player);
                }
                else if (input == "heal")
                {
                    //Console.WriteLine("You take a break and heal all your wounds");
                    //_player.CurrentHealth = _player.MaxHealth;
                    Console.WriteLine("The 'heal' command is no longer available. Use a potion or visit a healer.");
                }
                else if (input.StartsWith("use "))
                {
                    string itemName = input.Substring(4).Trim();
                    UseItem(itemName);
                }
                else if (input.StartsWith("go to "))
                {
                    string npcName = input.Substring(6).Trim().ToLower();
                    _npc.InteractWithNpc(npcName, ref _player);
                }
                else if (input == "inventory")
                {
                    _player.Inventory.ListItems();
                    Console.WriteLine($"💰 Money: {_player.Money}");
                }
                else if (input.StartsWith("loot corpse"))
                {
                    LootFirstCorpse();
                }
                else if (input == "look corpses")
                {
                    foreach (var c in _player.CurrentRoom.Corpses)
                        c.Describe();
                }
                else if (input.StartsWith("equip "))
                {
                    string itemName = input.Substring(6).Trim();
                    EquipItem(itemName);
                }
                else if (input == "exit")
                {
                    GameService.SaveHero(ref _player);
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
        /// <summary>
        /// Checks if the input is an debug command
        /// </summary>
        /// <param name="input">user input</param>
        /// <returns>if input was an command</returns>
        private bool DebugCommandCheck(string input)
        {
            if (!input.StartsWith("/"))
                return false;
            if (input.StartsWith("/set"))
            {
                if (input.StartsWith("/set level"))
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
            else if (input.StartsWith("/help"))
            {
                _printer.ShowDebugHelp();
            }
            else
            {
                Console.WriteLine("Unknown command!");
            }
            return true;
        }
        /// <summary>
        /// Handles the equipment of an item
        /// </summary>
        /// <param name="itemName">the item NAME</param>
        private void EquipItem(string itemName)
        {
            var match = _player.Inventory.Items
                .FirstOrDefault(i => i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));

            if (match is not EquipmentItem equipment)
            {
                Console.WriteLine("You can't equip that.");
                return;
            }

            if (!equipment.IsUsableBy(_player))
            {
                Console.WriteLine("Your class can't equip that item.");
                return;
            }

            _player.Equip(equipment);
            _player.Inventory.RemoveItem(equipment);
        }
        /// <summary>
        /// Handles item usage
        /// </summary>
        /// <param name="itemName">the item NAME</param>
        private void UseItem(string itemName)
        {
            var item = _player.Inventory.Items
                .FirstOrDefault(i => i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));

            if (item == null)
            {
                Console.WriteLine($"You don't have a '{itemName}' in your inventory.");
                return;
            }

            if (item is ConsumableItem consumable)
            {
                consumable.Use(_player);
                _player.Inventory.RemoveItem(item);
            }
            else
            {
                Console.WriteLine($"{item.Name} can't be used.");
            }

        }
        /// <summary>
        /// Moves the player in the specified direction or room
        /// </summary>
        /// <param name="direction">the direction choosen by the player</param>
        private void Move(string direction)
        {
            if (_player.CurrentRoom.Exits.TryGetValue(direction, out Room nextRoom))
            {
                if (nextRoom.RequirementType != RoomRequirementType.None)
                {
                    if (nextRoom.RequirementType == RoomRequirementType.Level && _player.Level < nextRoom.AccessLevel)
                    {
                        Console.WriteLine($"You must be level {nextRoom.AccessLevel} to enter this area.");
                        return;
                    }

                }
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
        /// <summary>
        /// Starts an Encounter after the look command if the room has monsters, also handels the End of a fight
        /// </summary>
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
                var drops = LootGenerator.GetLootFor(monster);

                if (drops.Count > 0)
                {
                    if (monster.DropsCorpse)
                    {
                        var corpse = new Corpse(monster.Name, drops);
                        _player.CurrentRoom.Corpses.Add(corpse);
                        Console.WriteLine($"The corpse of {monster.Name} remains. You can loot it.");
                    }
                    else
                    {
                        foreach (var drop in drops)
                        {
                            if (_player.Inventory.AddItem(drop))
                                Console.WriteLine($"🪶 You found: {drop.Name}");
                            else
                                Console.WriteLine($"❌ Inventory full. Could not take: {drop.Name}");
                        }
                    }
                }

            }
            else
                Console.WriteLine("\nYou were slain...");
        }
        /// <summary>
        /// Method to loot corpses with items in the current room
        /// </summary>
        private void LootFirstCorpse()
        {
            var corpses = _player.CurrentRoom.Corpses.Where(c => !c.IsLooted && c.Loot.Any()).ToList();

            if (corpses.Count == 0)
            {
                Console.WriteLine("There are no lootable corpses here.");
                return;
            }

            var corpse = corpses[0];
            Console.WriteLine($"You loot the corpse of {corpse.Name}:");

            foreach (var item in corpse.Loot)
            {
                if (_player.Inventory.AddItem(item))
                    Console.WriteLine($"  - {item.Name}");
                else
                    Console.WriteLine($"  - Could not carry {item.Name} (inventory full)");
            }

            corpse.Loot.Clear();
            corpse.IsLooted = true;
        }

    }

}
