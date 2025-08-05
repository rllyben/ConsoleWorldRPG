using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Enums;
using ConsoleWorldRPG.Items;
using ConsoleWorldRPG.Services;
using ConsoleWorldRPG.Systems;
using ConsoleWorldRPG.Utils;

namespace ConsoleWorldRPG.Entities.NPCs
{
    public class NpcInteractionHandler
    {
        /// <summary>
        /// Handels the go to command and selects the Encounter Method of the choosen NPC
        /// </summary>
        /// <param name="npc">npc name</param>
        public static void InteractWithNpc(string npc, ref Player player)
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
                    InteractWithQuestBoard(player);
                    break;
                default:
                    Console.WriteLine($"'{npc}' doesn’t do anything... yet.");
                    break;
            }

        }
        /// <summary>
        /// handels quest board interaction
        /// </summary>
        /// <param name="player">player character</param>
        public static void InteractWithQuestBoard(Player player)
        {
            var available = QuestManager.GetAvailableForPlayer(player);

            if (available.Count == 0)
            {
                Console.WriteLine("📜 No new quests available.");
                return;
            }

            for (int i = 0; i < available.Count; i++)
            {
                var q = available[i];
                Console.WriteLine($"{i + 1}. {q.Name} - {q.Description}");
            }

            Console.Write("Select quest to accept (0 to cancel): ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice > 0 && choice <= available.Count)
            {
                var quest = available[choice - 1];
                quest.Status = QuestStatus.InProgress; 

                foreach (var key in quest.RequiredKills.Keys)
                    quest.KillProgress[key] = 0;
                foreach (var itemId in quest.RequiredItems.Keys)
                    quest.ItemProgress[itemId] = 0;

                player.ActiveQuests.Add(quest);
                Console.WriteLine($"✔ You accepted the quest: {quest.Name}");
            }

        }
        /// <summary>
        /// Handles the smith encounter
        /// </summary>
        private static void SmithMenu(ref Player player)
        {

            while (true)
            {
                Console.WriteLine("\n🔧 Welcome to the Smith!");
                Console.WriteLine("1. Buy base equipment");
                Console.WriteLine("2. Upgrade equipment");
                Console.WriteLine("3. Craft materials");
                Console.WriteLine("4. Leave");

                Console.Write("Choose an option: ");
                string? choice = Console.ReadLine()?.Trim();

                switch (choice)
                {
                    case "1":
                        ShowSmithStock(player);
                        break;
                    case "2":
                        HandleUpgrade(player);
                        break;
                    case "3":
                        HandleCrafting(player);
                        break;
                    case "4":
                        Console.WriteLine("You leave the smith.");
                        return;
                    default:
                        Console.WriteLine("❌ Invalid option.");
                        break;
                }

            }

        }
        /// <summary>
        /// Handels crafting logic
        /// </summary>
        /// <param name="player">player character</param>
        private static void HandleCrafting(Player player)
        {
            Console.WriteLine("\n🧪 Available Crafting Options:");
            Console.WriteLine("1. Craft Upgrade Stone (requires: 3 iron_ore)");
            Console.WriteLine("0. Cancel");

            Console.Write("Choose: ");
            string? input = Console.ReadLine();
            if (input == "1")
            {
                const string requiredMaterial = "iron_ore";
                const string craftedItemId = "upgrade_stone";
                const int cost = 3;

                if (player.Inventory.Items.Any(i => i.Id == requiredMaterial))
                {
                    int have = player.Inventory.Items.FirstOrDefault(i => i.Id == requiredMaterial).StackSize;

                    if (have < cost)
                    {
                        Console.WriteLine($"❌ You need at least {cost} iron ore to craft an Upgrade Stone.");
                        return;
                    }

                }
                else
                {
                    Console.WriteLine($"❌ You need at least {cost} iron ore to craft an Upgrade Stone.");
                    return;
                }

                var toRemove = player.Inventory.Items
                    .Where(i => i.Id == requiredMaterial)
                    .Take(cost)
                    .ToList();

                foreach (var item in toRemove)
                    player.Inventory.RemoveItem(item);

                if (ItemFactory.TryCreateItem(craftedItemId, out var crafted))
                {
                    player.Inventory.AddItem(crafted, player);
                    Console.WriteLine($"✔ You crafted 1 {crafted.Name}.");
                }
                else
                {
                    Console.WriteLine($"❌ Could not create item '{craftedItemId}'.");
                }

            }

        }
        /// <summary>
        /// Handels upgrade logic
        /// </summary>
        /// <param name="player">player character</param>
        private static void HandleUpgrade(Player player)
        {
            var upgradable = player.Inventory.Items.OfType<EquipmentItem>()
                .Where(e => e.UpgradeLevel < 9).ToList();

            if (!upgradable.Any())
            {
                Console.WriteLine("❌ You have no equipment that can be upgraded.");
                return;
            }

            Console.WriteLine("\n🔧 Select equipment to upgrade:");
            for (int i = 0; i < upgradable.Count; i++)
                Console.WriteLine($"{i + 1}. {upgradable[i].Name} +{upgradable[i].UpgradeLevel}");

            Console.Write("Choice (0 to cancel): ");
            if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > upgradable.Count)
            {
                Console.WriteLine("Cancelled.");
                return;
            }

            var item = upgradable[choice - 1];

            if (item.TryUpgrade(player))
                Console.WriteLine($"🛠 {item.Name} is now +{item.UpgradeLevel}!");
        }
        /// <summary>
        /// prints the smith's stock for character class
        /// </summary>
        /// <param name="player">player character</param>
        private static void ShowSmithStock(Player player)
        {
            var stock = ItemFactory.GetSmithItemsFor(player)
                .OfType<EquipmentItem>().ToList();

            // Add pickaxe and woodcutter axe for everyone
            if (ItemFactory.TryCreateItem("pickaxe", out var pickaxe))
                stock.Add((EquipmentItem)pickaxe);

            if (ItemFactory.TryCreateItem("woodcutter_axe", out var axe))
                stock.Add((EquipmentItem)axe);

            if (stock.Count == 0)
            {
                Console.WriteLine("The smith has nothing for your class.");
                return;
            }

            for (int i = 0; i<stock.Count; i++)
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
        private static void HealerMenu(ref Player player)
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
                    TryBuyItem(ItemFactory.CreateItem("t1_healing_potion"), 100, ref player);
                    break;
                case "3":
                    TryBuyItem(ItemFactory.CreateItem("t1_mana_potion"), 120, ref player);
                    break;
                case "4":
                    SellItem(player);
                    break;
                default:
                    Console.WriteLine("You leave the healer.");
                    break;
            }

        }
        /// <summary>
        /// handels sell item interaction
        /// </summary>
        /// <param name="player">player character</param>
        private static void SellItem(Player player)
        {
            player.Inventory.ListItems();
            Console.Write("Enter the item name to sell: ");
            string? itemName = Console.ReadLine()?.Trim();

            var item = InventoryUtils.ResolveInventoryItem(itemName, player);

            if (item == null)
            {
                Console.WriteLine($"❌ You don't have an item named '{itemName}'.");
                return;
            }

            Console.WriteLine($"You have {item.StackSize}x {item.Name}. Each sells for {item.SellValue} bronze.");
            Console.Write("How many would you like to sell? ");
            if (!int.TryParse(Console.ReadLine(), out int amount) || amount <= 0 || amount > item.StackSize)
            {
                Console.WriteLine("Invalid quantity.");
                return;
            }

            int total = amount * item.SellValue;
            Console.Write($"Sell {amount}x {item.Name} for {total} bronze? (yes/no): ");
            string? confirm = Console.ReadLine()?.Trim().ToLower();

            if (confirm == "yes" || confirm == "y")
            {
                if (player.Inventory.SellItem(itemName, amount, ref player))
                {
                    Console.WriteLine($"🪙 Sold {amount}x {item.Name} for {total} bronze.");
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
        }
        /// <summary>
        /// Checks if the selected item to buy can be bought and adds it to the player's inventory
        /// </summary>
        /// <param name="item"></param>
        /// <param name="cost"></param>
        private static void TryBuyItem(Item item, int cost, ref Player player)
        {
            if (player.Money.TrySpend(cost))
            {
                if (player.Inventory.AddItem(item, player))
                    Console.WriteLine($"🧪 {item.Name} added to inventory.");
                else
                    Console.WriteLine("❌ Inventory full!");
            }
            else
                Console.WriteLine("Not enough money.");
        }
        public static void SkillMasterMenu(Player player)
        {
            Console.WriteLine("\n🔮 Welcome to the Skill Master!");

            var baseSkills = SkillFactory.GetBaseSkillsFor(player);
            if (!baseSkills.Any())
            {
                Console.WriteLine("You don't know any base skills yet.");
                return;
            }

            Console.WriteLine("\nYour base skills:");
            for (int i = 0; i < baseSkills.Count; i++)
                Console.WriteLine($"{i + 1}. {baseSkills[i].Name} ({string.Join(", ", baseSkills[i].ComponentType)})");

            Console.WriteLine("\nType the numbers of 2 or 4 skills to combine, separated by spaces (or '0' to cancel):");
            Console.Write("> ");
            var input = Console.ReadLine()!.Trim();

            if (input == "0") return;

            var indices = input.Split(' ')
                .Select(s => int.TryParse(s, out var i) ? i - 1 : -1)
                .Where(i => i >= 0 && i < baseSkills.Count)
                .ToList();

            if (indices.Count < 2 || indices.Count > 4)
            {
                Console.WriteLine("❌ You must choose 2 or 4 base skills.");
                return;
            }

            var selected = indices.Select(i => baseSkills[i]).ToList();
            var fused = SkillFusionSystem.FuseSkills(selected);

            player.Skills.Add(fused);
            Console.WriteLine($"\n✨ You have created a new skill: {fused.Name}!");
        }

    }

}
