using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Enums;
using ConsoleWorldRPG.Items;

namespace ConsoleWorldRPG.Commands
{
    public static class InventoryCommands
    {
        public static bool Handle(string input, Player player)
        {
            if (input.StartsWith("equip "))
            {
                string itemName = input.Substring(6).Trim();
                EquipItem(itemName, player); // to be added later
                return true;
            }
            else if (input.StartsWith("unequip "))
            {
                string slot = input.Substring(8).Trim().ToLower();
                Unequip(player, slot);
                return true;
            }
            else if (input.StartsWith("use "))
            {
                string itemName = input.Substring(4).Trim();
                UseItem(itemName, player);
                return true;
            }
            else if (input.StartsWith("inventory"))
            {
                player.Inventory.ListItems();
                Console.WriteLine($"💰 Money: {player.Money}");
                return true;
            }
            else if (input.StartsWith("search "))
            {
                string term = input.Substring(7).Trim();
                SearchInventory(player, term);
                return true;
            }
            else if (input.StartsWith("show "))
            {
                string itemName = input.Substring(5).Trim();
                ShowItem(player, itemName);
                return true;
            }

            // we'll add unequip here later

            return false;
        }
        /// <summary>
        /// Handles the equipment of an item
        /// </summary>
        /// <param name="itemName">the item NAME</param>
        private static void EquipItem(string itemName, Player player)
        {
            var match = player.Inventory.Items
                .FirstOrDefault(i => i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));

            if (match is not EquipmentItem equipment)
            {
                Console.WriteLine("You can't equip that.");
                return;
            }

            if (!equipment.IsUsableBy(player))
            {
                Console.WriteLine("Your class can't equip that item.");
                return;
            }

            player.Equip(equipment);
            player.Inventory.RemoveItem(equipment);
        }
        private static void Unequip(Player player, string slot)
        {
            EquipmentItem? item = slot switch
            {
                "weapon" => player.WeaponSlot,
                "armor" => player.ArmorSlot,
                "accessory" => player.AccessorySlot,
                _ => null
            };

            if (item == null)
            {
                Console.WriteLine($"❌ No item equipped in '{slot}' slot.");
                return;
            }

            if (player.Inventory.AddItem(item))
            {
                switch (slot)
                {
                    case "weapon": player.WeaponSlot = null; break;
                    case "armor": player.ArmorSlot = null; break;
                    case "accessory": player.AccessorySlot = null; break;
                }

                Console.WriteLine($"✔ Unequipped {item.Name} and returned it to your inventory.");
            }
            else
            {
                Console.WriteLine("❌ Inventory full — cannot unequip.");
            }
        }
        /// <summary>
        /// Handles item usage
        /// </summary>
        /// <param name="itemName">the item NAME</param>
        private static void UseItem(string itemName, Player player)
        {
            var item = player.Inventory.Items
                .FirstOrDefault(i => i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));

            if (item == null)
            {
                Console.WriteLine($"You don't have a '{itemName}' in your inventory.");
                return;
            }

            if (item is ConsumableItem consumable)
            {
                consumable.Use(player);
                player.Inventory.RemoveItem(item);
            }
            else
            {
                Console.WriteLine($"{item.Name} can't be used.");
            }

        }
        private static void ShowItem(Player player, string itemName)
        {
            Item? item = player.Inventory.Items
                .FirstOrDefault(i => i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));

            // also check equipped items
            item ??= player.WeaponSlot;
            if (item?.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase) != true)
                item ??= player.ArmorSlot;
            if (item?.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase) != true)
                item ??= player.AccessorySlot;
            if (item?.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase) != true)

            if (item == null)
            {
                Console.WriteLine($"❌ You don’t have an item named '{itemName}' equipped or in your inventory.");
                return;
            }

            Console.WriteLine($"Name: {item.Name}");
            Console.WriteLine($"Description: {item.Description}");

            if (item is EquipmentItem eq)
            {
                Console.WriteLine("Bonuses:");
                PrintBonus("ATK", eq.BonusATK);
                PrintBonus("DEF", eq.BonusDEF);
                PrintBonus("MATK", eq.BonusMATK);
                PrintBonus("MDEF", eq.BonusMDEF);
                PrintBonus("Aim", eq.BonusAim);
                PrintBonus("Evasion", eq.BonusEvasion);
                PrintBonus("STR", eq.BonusSTR);
                PrintBonus("DEX", eq.BonusDEX);
                PrintBonus("END", eq.BonusEND);
                PrintBonus("INT", eq.BonusINT);
                PrintBonus("SPR", eq.BonusSPR);
                PrintBonus("Crit", eq.BonusCrit);
                PrintBonus("Block", eq.BonusBlock);

                if (eq.AllowedClasses.Any())
                    Console.WriteLine("Usable by: " + string.Join(", ", eq.AllowedClasses));
            }

        }
        private static void SearchInventory(Player player, string term)
        {
            var items = player.Inventory.Items;

            IEnumerable<Item> result = term.ToLower() switch
            {
                "usable" => items.Where(i => i is ConsumableItem),
                "equip" => items.Where(i => i is EquipmentItem),
                "material" => items.Where(i => i is MaterialItem),
                "weapon" => items.OfType<EquipmentItem>().Where(e => e.SlotType == EquipmentType.Weapon),
                "armor" => items.OfType<EquipmentItem>().Where(e => e.SlotType == EquipmentType.Armor),
                "accessory" => items.OfType<EquipmentItem>().Where(e => e.SlotType == EquipmentType.Accessory),
                _ => items.Where(i => i.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
            };

            if (!result.Any())
            {
                Console.WriteLine("🔍 No matching items found.");
                return;
            }

            Console.WriteLine($"🔍 Search results for '{term}':");
            foreach (var item in result)
            {
                Console.WriteLine($"  - {item.Name}");
            }
        }
        private static void PrintBonus(string name, int value)
        {
            if (value != 0)
                Console.WriteLine($"  +{value} {name}");
        }
        private static void PrintBonus(string name, float value)
        {
            if (value != 0)
                Console.WriteLine($"  +{value} {name}%");
        }

    }

}
