using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;

namespace ConsoleWorldRPG.Utils
{
    public class Printer
    {
        /// <summary>
        /// Prints the Help information into the Console
        /// </summary>
        public void ShowHelp()
        {
            Console.WriteLine("\nAvailable commands:");
            Console.WriteLine("  move <direction>   - Move to another room (e.g., move north)");
            Console.WriteLine("  look               - Re-describe the current room or start an encounter");
            Console.WriteLine("  status             - Show your current health, mana, stats and equipt items");
            Console.WriteLine("  inventory          - Show your inventory and moneybag");
            Console.WriteLine("  go to <npc name>   - Go to the specified NPC (e.g., go to healer)");
            Console.WriteLine("  equip <item name>  - Equips the specified item and puts the currently equiped one back into the inventory (e.g., equip Staff)");
            Console.WriteLine("  use <item name>    - Uses the specified item (e.g., use simple Healing Potion)");
            Console.WriteLine("  loot corpse        - loots the first dropped corpse in the current room");
            Console.WriteLine("  look corpses       - Show all current corpses that arent looted in the current room");
            Console.WriteLine("  heal               - (LAGACY: not in use anymoe!) Heals your character to full HP");
            Console.WriteLine("  help               - Show this help message");
            Console.WriteLine("  exit               - Save and Quit the game");
        }
        public void ShowDebugHelp()
        {
            Console.WriteLine("\nAvailable commands:");
            Console.WriteLine("  /set level <level>  - sets the current player level to the specified level (the level can't be smaller than the current one)");
            Console.WriteLine("  /help               - Show this help message");
        }
        /// <summary>
        /// Prints the Status for the current Hero
        /// </summary>
        public void ShowStatus(Player _player)
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

    }

}
