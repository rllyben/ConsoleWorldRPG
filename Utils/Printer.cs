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
                ItemRarity.Common    => ConsoleColor.Gray,
                ItemRarity.Uncommon  => ConsoleColor.Green,
                ItemRarity.Rare      => ConsoleColor.Blue,
                ItemRarity.Epic      => ConsoleColor.Magenta,
                ItemRarity.Unique    => ConsoleColor.Yellow,
                ItemRarity.Legendary => ConsoleColor.DarkYellow,
                _                    => ConsoleColor.Gray
            };
            Console.ForegroundColor = color;
            Console.Write($" - {displayName}");
            Console.ResetColor();
        }

        private static void PrintRainbow(string text)
        {
            var colors = new[]
            {
                ConsoleColor.Red, ConsoleColor.Yellow, ConsoleColor.Green,
                ConsoleColor.Cyan, ConsoleColor.Blue, ConsoleColor.Magenta
            };
            int i = 0;
            foreach (char c in text)
            {
                Console.ForegroundColor = colors[i++ % colors.Length];
                Console.Write(c);
            }
            Console.ResetColor();
        }

        public static void ShowLook(Room room)
        {
            Console.WriteLine($"\n{room.Name}: {room.Description}");

            if (room.Exits.Any())
                Console.WriteLine("Exits: " + string.Join(", ", room.Exits.Keys));

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

            if (room.HasMonsters)
            {
                Console.WriteLine("\nYou see the following Monsters:");
                foreach (Monster monster in room.Monsters)
                    Console.WriteLine($"  - {monster.Name}");
            }
        }

        public static void ShowHelp()
        {
            Console.WriteLine("\nAvailable commands:");
            Console.WriteLine("  move <direction>           - Move to another room (e.g., move north)");
            Console.WriteLine("  look                       - Re-describe the current room");
            Console.WriteLine("  fight                      - Starts an encounter if monsters are present");
            Console.WriteLine("  status                     - Show your current health, mana, stats and equipped items");
            Console.WriteLine("  inventory                  - Show your inventory and moneybag");
            Console.WriteLine("  go to <npc name>           - Go to the specified NPC (e.g., go to healer)");
            Console.WriteLine("  equip <item name>          - Equip the specified item");
            Console.WriteLine("  use <item name>            - Use the specified item (e.g., use simple Healing Potion)");
            Console.WriteLine("  loot                       - Loot the first dropped corpse in the current room");
            Console.WriteLine("  gather <source name>       - Gather material from the given source");
            Console.WriteLine("  map                        - Print the world/dungeon/cave/forest/city map");
            Console.WriteLine("  help                       - Show this help message");
            Console.WriteLine("  logout                     - Save and log out");
            Console.WriteLine("  exit                       - Save and quit the game");
        }

        public static void ShowDebugHelp()
        {
            Console.WriteLine("\nDebug commands:");
            Console.WriteLine("  /set level <level>  - Set the current player level (can only increase)");
            Console.WriteLine("  /skillmaster        - Open the Skill Master menu");
            Console.WriteLine("  /help               - Show this help message");
        }

        public static void ShowStatus(Player player)
        {
            Console.WriteLine($"\n{player.Name}'s Status:");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  Class: {player.Class}");
            Console.ResetColor();
            Console.WriteLine($"  Level: {player.Level}    Exp: {player.Experience}/{player.ExpForNextLvl}\n");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  HP: {player.CurrentHealth}/{player.MaxHealth}");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"  Mana: {player.CurrentMana}/{player.MaxMana}");
            Console.ResetColor();
            Console.WriteLine($"  STR: {player.Stats.Strength} + {player.GetBonusFromGear(g => g.BonusSTR)} from gear");
            Console.WriteLine($"  DEX: {player.Stats.Dexterity} + {player.GetBonusFromGear(g => g.BonusDEX)} from gear");
            Console.WriteLine($"  END: {player.Stats.Endurance} + {player.GetBonusFromGear(g => g.BonusEND)} from gear");
            Console.WriteLine($"  INT: {player.Stats.Intelligence} + {player.GetBonusFromGear(g => g.BonusINT)} from gear");
            Console.WriteLine($"  SPR: {player.Stats.Spirit} + {player.GetBonusFromGear(g => g.BonusSPR)} from gear");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"\n  ATK:  {player.TotalPhysicalAttack  - player.GetBonusFromGear(g => g.BonusATK)}  + {player.GetBonusFromGear(g => g.BonusATK)} from gear");
            Console.WriteLine($"  DEF:  {player.TotalPhysicalDefense - player.GetBonusFromGear(g => g.BonusDEF)}  + {player.GetBonusFromGear(g => g.BonusDEF)} from gear");
            Console.WriteLine($"  MATK: {player.TotalMagicAttack     - player.GetBonusFromGear(g => g.BonusMATK)} + {player.GetBonusFromGear(g => g.BonusMATK)} from gear");
            Console.WriteLine($"  MDEF: {player.TotalMagicDefense    - player.GetBonusFromGear(g => g.BonusMDEF)} + {player.GetBonusFromGear(g => g.BonusMDEF)} from gear");
            Console.WriteLine($"  Aim:  {player.TotalAim     - player.GetBonusFromGear(g => g.BonusAim)}      + {player.GetBonusFromGear(g => g.BonusAim)} from gear");
            Console.WriteLine($"  Eva:  {player.TotalEvasion - player.GetBonusFromGear(g => g.BonusEvasion)} + {player.GetBonusFromGear(g => g.BonusEvasion)} from gear");
            Console.WriteLine($"  Crit: {player.CritChance:P0} | Block: {player.BlockChance:P0}");
            Console.ResetColor();

            Console.WriteLine("\nEquipped:");
            Console.Write("  Weapon:    ");
            if (player.WeaponSlot == null) Console.Write("(none)"); else PrintColoredItemName(player.WeaponSlot);
            Console.WriteLine();
            Console.Write("  Armor:     ");
            if (player.ArmorSlot == null) Console.Write("(none)"); else PrintColoredItemName(player.ArmorSlot);
            Console.WriteLine();
            Console.Write("  Accessory: ");
            if (player.AccessorySlot == null) Console.Write("(none)"); else PrintColoredItemName(player.AccessorySlot);
            Console.WriteLine();
        }

        public static void ShowEquipSuccess(Item item) => Console.WriteLine($"✔ Equipped {item.Name}.");
        public static void ShowEquipFail(string reason) => Console.WriteLine($"❌ Cannot equip: {reason}");

    }

}
