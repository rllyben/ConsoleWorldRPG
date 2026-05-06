namespace ConsoleWorldRPG.Commands
{
    public static class InventoryCommands
    {
        public static bool Handle(string input, Player player)
        {
            if (input.StartsWith("equip "))
            {
                EquipItem(input.Substring(6).Trim(), player);
                return true;
            }
            else if (input.StartsWith("unequip "))
            {
                Unequip(player, input.Substring(8).Trim().ToLower());
                return true;
            }
            else if (input.StartsWith("use "))
            {
                UseItem(input.Substring(4).Trim(), player);
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
                SearchInventory(player, input.Substring(7).Trim());
                return true;
            }
            else if (input.StartsWith("show "))
            {
                ShowItem(player, input.Substring(5).Trim());
                return true;
            }

            return false;
        }

        private static void EquipItem(string itemName, Player player)
        {
            var match = InventoryUtils.ResolveInventoryItem(itemName, player);
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
                "weapon"    => player.WeaponSlot,
                "armor"     => player.ArmorSlot,
                "accessory" => player.AccessorySlot,
                _           => null
            };

            if (item == null) { Console.WriteLine($"❌ No item equipped in '{slot}' slot."); return; }

            if (player.Inventory.AddItem(item, player))
            {
                switch (slot)
                {
                    case "weapon":    player.WeaponSlot    = null; break;
                    case "armor":     player.ArmorSlot     = null; break;
                    case "accessory": player.AccessorySlot = null; break;
                }
                Console.WriteLine($"✔ Unequipped {item.Name} and returned it to your inventory.");
            }
            else
            {
                Console.WriteLine("❌ Inventory full — cannot unequip.");
            }
        }

        private static void UseItem(string itemName, Player player)
        {
            var item = InventoryUtils.ResolveInventoryItem(itemName, player);
            if (item == null) { Console.WriteLine($"You don't have a '{itemName}' in your inventory."); return; }

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
            var item = InventoryUtils.ResolveInventoryItem(itemName, player)
                ?? (player.WeaponSlot?.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase) == true ? player.WeaponSlot : null)
                ?? (player.ArmorSlot?.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase) == true ? player.ArmorSlot : null)
                ?? (player.AccessorySlot?.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase) == true ? player.AccessorySlot : null);

            if (item == null) { Console.WriteLine($"❌ You don't have an item named '{itemName}'."); return; }

            Console.WriteLine($"Name: {item.Name}");
            Console.WriteLine($"Description: {item.Description}");

            if (item is EquipmentItem eq)
            {
                Console.WriteLine("Bonuses:");
                PrintBonus("ATK",     eq.BonusATK);
                PrintBonus("DEF",     eq.BonusDEF);
                PrintBonus("MATK",    eq.BonusMATK);
                PrintBonus("MDEF",    eq.BonusMDEF);
                PrintBonus("Aim",     eq.BonusAim);
                PrintBonus("Evasion", eq.BonusEvasion);
                PrintBonus("STR",     eq.BonusSTR);
                PrintBonus("DEX",     eq.BonusDEX);
                PrintBonus("END",     eq.BonusEND);
                PrintBonus("INT",     eq.BonusINT);
                PrintBonus("SPR",     eq.BonusSPR);
                PrintBonus("Crit",    eq.BonusCrit);
                PrintBonus("Block",   eq.BonusBlock);

                if (eq.AllowedClasses.Any())
                    Console.WriteLine("Usable by: " + string.Join(", ", eq.AllowedClasses));
            }
        }

        private static void SearchInventory(Player player, string term)
        {
            var items = player.Inventory.Items;
            IEnumerable<Item> result = term.ToLower() switch
            {
                "usable"    => items.Where(i => i is ConsumableItem),
                "equip"     => items.Where(i => i is EquipmentItem),
                "material"  => items.Where(i => i is MaterialItem),
                "weapon"    => items.OfType<EquipmentItem>().Where(e => e.SlotType == EquipmentType.Weapon),
                "armor"     => items.OfType<EquipmentItem>().Where(e => e.SlotType == EquipmentType.Armor),
                "accessory" => items.OfType<EquipmentItem>().Where(e => e.SlotType == EquipmentType.Accessory),
                _           => items.Where(i => i.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
            };

            if (!result.Any()) { Console.WriteLine("🔍 No matching items found."); return; }

            Console.WriteLine($"🔍 Search results for '{term}':");
            foreach (var item in result)
                Console.WriteLine($"  - {item.Name}");
        }

        private static void PrintBonus(string name, int value)
        {
            if (value != 0) Console.WriteLine($"  +{value} {name}");
        }

        private static void PrintBonus(string name, float value)
        {
            if (value != 0) Console.WriteLine($"  +{value}% {name}");
        }

    }

}
