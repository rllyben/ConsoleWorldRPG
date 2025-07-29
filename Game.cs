using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Enums;
using ConsoleWorldRPG.Items;
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
        private Random _random = new Random();
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
                    ShowHelp();
                }
                else if (input == "status")
                {
                    ShowStatus();
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
                    InteractWithNpc(npcName);
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
        /// Handels the go to command and selects the Encounter Method of the choosen NPC
        /// </summary>
        /// <param name="npc"></param>
        private void InteractWithNpc(string npc)
        {
            if (!_player.CurrentRoom.IsCity)
            {
                Console.WriteLine("You can only visit NPCs while in a city.");
                return;
            }

            var found = _player.CurrentRoom.Npcs
                .FirstOrDefault(n => n.Equals(npc, StringComparison.OrdinalIgnoreCase));

            if (found == null)
            {
                Console.WriteLine($"There’s no one named '{npc}' here.");
                return;
            }

            switch (npc.ToLower())
            {
                case "healer":
                    HealerMenu();
                    break;
                case "smith":
                    SmithMenu();
                    break;
                case "quest board":
                case "questboard":
                    Console.WriteLine("You read the quest board, but it’s currently empty.");
                    break;
                default:
                    Console.WriteLine($"'{npc}' doesn’t do anything... yet.");
                    break;
            }
        }
        /// <summary>
        /// Handles the smith encounter
        /// </summary>
        private void SmithMenu()
        {
            var stock = ItemFactory.GetAllItemsFor(_player)
                .OfType<EquipmentItem>().ToList();

            if (stock.Count == 0)
            {
                Console.WriteLine("The smith has nothing for your class.");
                return;
            }

            for (int i = 0; i < stock.Count; i++)
                Console.WriteLine($"{i + 1}. {stock[i].Name} - {stock[i].BuyPrice} bronze");

            Console.Write("Choose item to buy (0 to cancel): ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= stock.Count)
                TryBuyItem(stock[choice - 1], stock[choice - 1].BuyPrice);
            else
                Console.WriteLine("Cancelled.");
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
        /// Handles the encounter wiht the healer
        /// </summary>
        private void HealerMenu()
        {
            Console.WriteLine("\n🧙 You approach the healer.");
            Console.WriteLine("1. Heal (Free)");
            if (_player.PotionTierAvailable < 2)
            {
                Console.WriteLine("2. Buy simple Healing Potion (100 bronze)");
                Console.WriteLine("3. Buy simple Mana Potion (120 bronze)");
            }
            Console.WriteLine("4. Sell Item");
            Console.WriteLine("5. Leave");

            Console.Write("Choice: ");
            string? choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    _player.CurrentHealth = _player.Stats.MaxHealth;
                    _player.CurrentMana = _player.Stats.MaxMana;
                    Console.WriteLine("✨ You are fully healed.");
                    break;
                case "2":
                    TryBuyItem(ItemFactory.CreateItem("healing_potion"), 100);
                    break;
                case "3":
                    TryBuyItem(ItemFactory.CreateItem("mana_potion"), 120);
                    break;
                case "4":
                    _player.Inventory.ListItems();
                    Console.Write("Enter the item name to sell: ");
                    string? itemName = Console.ReadLine()?.Trim();

                    var item = _player.Inventory.Items
                        .FirstOrDefault(i => i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));

                    if (item == null)
                    {
                        Console.WriteLine($"❌ You don't have an item named '{itemName}'.");
                        break;
                    }

                    Console.WriteLine($"You have {item.StackSize}x {item.Name}. Each sells for {item.SellValue} bronze.");
                    Console.Write("How many would you like to sell? ");
                    if (!int.TryParse(Console.ReadLine(), out int amount) || amount <= 0 || amount > item.StackSize)
                    {
                        Console.WriteLine("Invalid quantity.");
                        break;
                    }

                    int total = amount * item.SellValue;
                    Console.Write($"Sell {amount}x {item.Name} for {total} bronze? (yes/no): ");
                    string? confirm = Console.ReadLine()?.Trim().ToLower();

                    if (confirm == "yes" || confirm == "y")
                    {
                        if (_player.Inventory.SellItem(itemName, amount, out int gained))
                        {
                            _player.Money.TryAdd(gained);
                            Console.WriteLine($"🪙 Sold {amount}x {item.Name} for {gained} bronze.");
                        }
                        else
                        {
                            Console.WriteLine("❌ Something went wrong while selling.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Cancelled.");
                    }
                    break;
                default:
                    Console.WriteLine("You leave the healer.");
                    break;
            }

        }
        /// <summary>
        /// Checks if the selected item to buy can be bought and adds it to the player's inventory
        /// </summary>
        /// <param name="item"></param>
        /// <param name="cost"></param>
        private void TryBuyItem(Item item, int cost)
        {
            if (_player.Money.TrySpend(cost))
            {
                if (_player.Inventory.AddItem(item))
                    Console.WriteLine($"🧪 {item.Name} added to inventory.");
                else
                    Console.WriteLine("❌ Inventory full!");
            }
            else
                Console.WriteLine("Not enough money.");
        }
        /// <summary>
        /// Saves the current state of the hero
        /// </summary>
        private void SaveHero()
        {
            _player.CurrentRoomId = _player.CurrentRoom.Id;
            string filePath = $"player_hero.json";
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new ItemConverter() },
                PropertyNameCaseInsensitive = true
            };
            string jsonData = JsonSerializer.Serialize(_player, options);
            File.WriteAllText(filePath, jsonData);
            Console.WriteLine($"Hero saved to {filePath}");
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
                Console.WriteLine("Unknown command!");
            }
            return true;
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
        /// Prints the Help information into the Console
        /// </summary>
        private void ShowHelp()
        {
            Console.WriteLine("\nAvailable commands:");
            Console.WriteLine("  move <direction> - Move to another room (e.g., move north)");
            Console.WriteLine("  look             - Re-describe the current room");
            Console.WriteLine("  status           - Show your current health, mana, and stats");
            Console.WriteLine("  help             - Show this help message");
            Console.WriteLine("  exit             - Quit the game");
        }
        /// <summary>
        /// Prints the Status for the current Hero
        /// </summary>
        private void ShowStatus()
        {
            Console.WriteLine($"\n{_player.Name}'s Status:");
            Console.WriteLine($"  Level: {_player.Level}    Exp: {_player.Experience}/{_player.ExpForNextLvl}");
            Console.WriteLine($"  HP: {_player.CurrentHealth}/{_player.Stats.MaxHealth}");
            Console.WriteLine($"  Mana: {_player.CurrentMana}/{_player.Stats.MaxMana}");
            Console.WriteLine($"  STR: {_player.Stats.Strength} + {_player.GetBonusFromGear(g => g.BonusSTR)} from gear");
            Console.WriteLine($"  DEX: {_player.Stats.Dexterity} + {_player.GetBonusFromGear(g => g.BonusDEX)} from gear");
            Console.WriteLine($"  END: {_player.Stats.Endurance} + {_player.GetBonusFromGear(g => g.BonusEND)} from gear");
            Console.WriteLine($"  INT: {_player.Stats.Intelligence} + {_player.GetBonusFromGear(g => g.BonusINT)} from gear");
            Console.WriteLine($"  SPR: {_player.Stats.Spirit} + {_player.GetBonusFromGear(g => g.BonusSPR)} from gear");
            Console.WriteLine("\n");
            Console.WriteLine($"  ATK: {_player.TotalPhysicalAttack}");
            Console.WriteLine($"  DEF: {_player.TotalPhysicalDefense}");
            Console.WriteLine($"  MATK: {_player.TotalMagicAttack}");
            Console.WriteLine($"  MDEF: {_player.TotalMagicDefense}");
            Console.WriteLine($"  Crit: {_player.CritChance:P0}");
            Console.WriteLine($"  Block: {_player.BlockChance:P0}");
            Console.WriteLine("");
            Console.WriteLine("\nEquipped:");
            Console.WriteLine($"  Weapon:   {_player.WeaponSlot?.Name ?? "(none)"}");
            Console.WriteLine($"  Armor:    {_player.ArmorSlot?.Name ?? "(none)"}");
            Console.WriteLine($"  Accessory:{_player.AccessorySlot?.Name ?? "(none)"}"); 
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
