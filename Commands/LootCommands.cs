using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Utils;

namespace ConsoleWorldRPG.Commands
{
    public static class LootCommands
    {
        public static bool Handle(string input, Player player)
        {
            if (input == "look corpses")
            {
                ShowCorpses(player);
                return true;
            }

            if (input == "loot corpse")
            {
                LootFirstCorpse(player);
                return true;
            }

            return false;
        }

        private static void ShowCorpses(Player player)
        {
            var corpses = player.CurrentRoom.Corpses;

            if (!corpses.Any())
            {
                Console.WriteLine("There are no corpses here.");
                return;
            }

            foreach (var corpse in corpses)
                corpse.Describe();
        }

        private static void LootFirstCorpse(Player player)
        {
            var corpse = player.CurrentRoom.Corpses
                .FirstOrDefault(c => !c.IsLooted && c.Loot.Any());

            if (corpse == null)
            {
                Console.WriteLine("There are no lootable corpses here.");
                return;
            }

            Console.WriteLine($"You loot the corpse of {corpse.Name}:");

            foreach (var item in corpse.Loot)
            {
                if (player.Inventory.AddItem(item, player))
                {
                    Printer.PrintColoredItemName(item);
                    Console.WriteLine();
                }
                else
                {
                    Console.Write($"  - Could not carry ");
                    Printer.PrintColoredItemName(item);
                    Console.WriteLine("inventory full)");
                    return;
                }

            }

            corpse.Loot.Clear();
            corpse.IsLooted = true;
        }

    }

}
