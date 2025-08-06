using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Enums;
using ConsoleWorldRPG.Items;

namespace ConsoleWorldRPG.Utils
{
    public class Printer
    {
        public static void ShowInvalidCommand()
        {
            Console.WriteLine("❌ Unknown command. Type 'help' to see available commands.");
        }
        public static void PrintColoredItemName(Item item)
        {
            string displayName = item is EquipmentItem eq && eq.UpgradeLevel > 0
                                 ? $"{item.Name} +{eq.UpgradeLevel}"
                                 : item.Name;
            if (item.Rarity == ItemRarity.Godly)
            {
                PrintRainbow(displayName);
                return;
            }
            var color = item.Rarity switch
            {
                ItemRarity.Common => ConsoleColor.Gray,
                ItemRarity.Uncommon => ConsoleColor.Green,
                ItemRarity.Rare => ConsoleColor.Blue,
                ItemRarity.Epic => ConsoleColor.Magenta,
                ItemRarity.Unique => ConsoleColor.Yellow,
                ItemRarity.Legendary => ConsoleColor.DarkYellow,
                _ => ConsoleColor.Gray
            };

            Console.ForegroundColor = color;
            Console.Write($" - {displayName}");
            Console.ResetColor();
        }
        private static void PrintRainbow(string text)
        {
            var colors = new[]
            {
                ConsoleColor.Red,
                ConsoleColor.Yellow,
                ConsoleColor.Green,
                ConsoleColor.Cyan,
                ConsoleColor.Blue,
                ConsoleColor.Magenta
            };

            int i = 0;
            foreach (char c in text)
            {
                Console.ForegroundColor = colors[i % colors.Length];
                Console.Write(c);
                i++;
            }

            Console.ResetColor();
        }
        public static void ShowEquipSuccess(Item item)
        {
            Console.WriteLine($"✔ Equipped {item.Name}.");
        }
        public static void ShowEquipFail(string reason)
        {
            Console.WriteLine($"❌ Cannot equip: {reason}");
        }
        public static void ShowLook(Room room)
        {
            Console.WriteLine($"\n{room.Name}: {room.Description}");

            if (room.Exits.Any())
            {
                Console.WriteLine("Exits: " + string.Join(", ", room.Exits.Keys));
            }

            if (room.IsCity && room.Npcs.Count > 0)
            {
                Console.WriteLine("You see the following people:");
                foreach (var npc in room.Npcs)
                    Console.WriteLine($"  - {npc}");
            }

            if (room.Corpses.Any())
            {
                Console.WriteLine("There are corpses here:");
                foreach (var corpse in room.Corpses)
                    corpse.Describe();
            }

            if (room.GatheringSpots.Any())
            {
                Console.WriteLine("\nYou notice the following gathering spots:");
                foreach (var spot in room.GatheringSpots)
                    Console.WriteLine($"  - {spot.Name}: {spot.Description}");
            }

        }
        /// <summary>
        /// Prints the Help information into the Console
        /// </summary>
        public static void ShowHelp()
        {
            Console.WriteLine("\nAvailable commands:");
            Console.WriteLine("  move <direction>           - Move to another room (e.g., move north)");
            Console.WriteLine("  look                       - Re-describe the current room");
            Console.WriteLine("  fight                      - Starts an encounter if monsters are present");
            Console.WriteLine("  status                     - Show your current health, mana, stats and equipt items");
            Console.WriteLine("  inventory                  - Show your inventory and moneybag");
            Console.WriteLine("  go to <npc name>           - Go to the specified NPC (e.g., go to healer)");
            Console.WriteLine("  equip <item name>          - Equips the specified item and puts the currently equiped one back into the inventory (e.g., equip Staff)");
            Console.WriteLine("  use <item name>            - Uses the specified item (e.g., use simple Healing Potion)");
            Console.WriteLine("  loot                       - loots the first dropped corpse in the current room");
            Console.WriteLine("  look corpses || LAGECY ||  - (LAGACY: not in use anymoe!) Show all current corpses that arent looted in the current room");
            Console.WriteLine("  gather <source name>       - Gathers material from the given source");
            Console.WriteLine("  map                        - Prints  the World-, Dungon-, Cave-, Forest- or City- map can have world or the name of the general room (eg. Dungon name) as a suffix");
            Console.WriteLine("  heal ||LAGACY||            - (LAGACY: not in use anymoe!) Heals your character to full HP");
            Console.WriteLine("  help                       - Show this help message");
            Console.WriteLine("  exit                       - Save and Quit the game");
        }
        public static void ShowDebugHelp()
        {
            Console.WriteLine("\nAvailable commands:");
            Console.WriteLine("  /set level <level>  - sets the current player level to the specified level (the level can't be smaller than the current one)");
            Console.WriteLine("  /help               - Show this help message");
        }
        /// <summary>
        /// Prints the Status for the current Hero
        /// </summary>
        public static void ShowStatus(Player _player)
        {
            Console.WriteLine($"\n{_player.Name}'s Status:");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  Class: {_player.Class}");
            Console.ResetColor();
            Console.WriteLine($"  Level: {_player.Level}    Exp: {_player.Experience}/{_player.ExpForNextLvl}\n");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  HP: {_player.CurrentHealth}/{_player.Stats.MaxHealth}");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"  Mana: {_player.CurrentMana}/{_player.Stats.MaxMana}");
            Console.ResetColor();
            Console.WriteLine($"  STR: {_player.Stats.Strength} + {_player.GetBonusFromGear(g => g.BonusSTR)} from gear");
            Console.WriteLine($"  DEX: {_player.Stats.Dexterity} + {_player.GetBonusFromGear(g => g.BonusDEX)} from gear");
            Console.WriteLine($"  END: {_player.Stats.Endurance} + {_player.GetBonusFromGear(g => g.BonusEND)} from gear");
            Console.WriteLine($"  INT: {_player.Stats.Intelligence} + {_player.GetBonusFromGear(g => g.BonusINT)} from gear");
            Console.WriteLine($"  SPR: {_player.Stats.Spirit} + {_player.GetBonusFromGear(g => g.BonusSPR)} from gear");
            Console.WriteLine("\n");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"  ATK: {_player.TotalPhysicalAttack - _player.GetBonusFromGear(g => g.BonusATK)} + {_player.GetBonusFromGear(g => g.BonusATK)} from gear");
            Console.WriteLine($"  DEF: {_player.TotalPhysicalDefense - _player.GetBonusFromGear(g => g.BonusDEF)} + {_player.GetBonusFromGear(g => g.BonusDEF)} from gear");
            Console.WriteLine($"  MATK: {_player.TotalMagicAttack - _player.GetBonusFromGear(g => g.BonusMATK)} + {_player.GetBonusFromGear(g => g.BonusMATK)} from gear");
            Console.WriteLine($"  MDEF: {_player.TotalMagicDefense - _player.GetBonusFromGear(g => g.BonusMDEF)} + {_player.GetBonusFromGear(g => g.BonusMDEF)} from gear");
            Console.WriteLine($"  Aim: {_player.TotalAim - _player.GetBonusFromGear(g => g.BonusAim)} + {_player.GetBonusFromGear(g => g.BonusAim)} from gear");
            Console.WriteLine($"  Evation: {_player.TotalEvasion - _player.GetBonusFromGear(g => g.BonusEvasion)} + {_player.GetBonusFromGear(g => g.BonusEvasion)} from gear");
            Console.WriteLine($"  Crit: {_player.CritChance:P0} | {_player.GetBonusFromGear(g => g.BonusCrit)} from gear");
            Console.WriteLine($"  Block: {_player.BlockChance:P0} | {_player.GetBonusFromGear(g => g.BonusBlock)} from gear");
            Console.ResetColor();
            Console.WriteLine("");
            Console.WriteLine("\nEquipped:");
            Console.Write($"  Weapon:   ");
            if (_player.WeaponSlot == null)
                Console.Write("(none)");
            else
                PrintColoredItemName( _player.WeaponSlot );
            Console.WriteLine();
            Console.Write($"  Armor:   ");
            if (_player.ArmorSlot == null)
                Console.Write("(none)");
            else
                PrintColoredItemName(_player.ArmorSlot);
            Console.WriteLine();
            Console.Write($"  Accessory:   ");
            if (_player.AccessorySlot == null)
                Console.Write("(none)");
            else
                PrintColoredItemName(_player.AccessorySlot);
            Console.WriteLine();
        }

    }

}
