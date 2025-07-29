using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Items;
using ConsoleWorldRPG.Services;

namespace ConsoleWorldRPG.Entities.NPCs
{
    public class NpcInteractionHandler
    {
        /// <summary>
        /// Handels the go to command and selects the Encounter Method of the choosen NPC
        /// </summary>
        /// <param name="npc"></param>
        public void InteractWithNpc(string npc, ref Player player)
        {
            if (!player.CurrentRoom.IsCity)
            {
                Console.WriteLine("You can only visit NPCs while in a city.");
                return;
            }

            var found = player.CurrentRoom.Npcs
                .FirstOrDefault(n => n.Equals(npc, StringComparison.OrdinalIgnoreCase));

            if (found == null)
            {
                Console.WriteLine($"There’s no one named '{npc}' here.");
                return;
            }

            switch (npc.ToLower())
            {
                case "healer":
                    HealerMenu(ref player);
                    break;
                case "smith":
                    SmithMenu(ref player);
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
        private void SmithMenu(ref Player player)
        {
            var stock = ItemFactory.GetAllItemsFor(player)
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
                TryBuyItem(stock[choice - 1], stock[choice - 1].BuyPrice, ref player);
            else
                Console.WriteLine("Cancelled.");
        }
        /// <summary>
        /// Handles the encounter wiht the healer
        /// </summary>
        private void HealerMenu(ref Player player)
        {
            Console.WriteLine("\n🧙 You approach the healer.");
            Console.WriteLine("1. Heal (Free)");
            if (player.PotionTierAvailable < 2)
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
                    player.CurrentHealth = player.Stats.MaxHealth;
                    player.CurrentMana = player.Stats.MaxMana;
                    Console.WriteLine("✨ You are fully healed.");
                    break;
                case "2":
                    TryBuyItem(ItemFactory.CreateItem("healing_potion"), 100, ref player);
                    break;
                case "3":
                    TryBuyItem(ItemFactory.CreateItem("mana_potion"), 120, ref player);
                    break;
                case "4":
                    player.Inventory.ListItems();
                    Console.Write("Enter the item name to sell: ");
                    string? itemName = Console.ReadLine()?.Trim();

                    var item = player.Inventory.Items
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
                        if (player.Inventory.SellItem(itemName, amount, out int gained))
                        {
                            player.Money.TryAdd(gained);
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
        private void TryBuyItem(Item item, int cost, ref Player player)
        {
            if (player.Money.TrySpend(cost))
            {
                if (player.Inventory.AddItem(item))
                    Console.WriteLine($"🧪 {item.Name} added to inventory.");
                else
                    Console.WriteLine("❌ Inventory full!");
            }
            else
                Console.WriteLine("Not enough money.");
        }

    }

}
