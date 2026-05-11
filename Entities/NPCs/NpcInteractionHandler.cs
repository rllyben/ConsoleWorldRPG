namespace ConsoleWorldRPG.Entities.NPCs
{
    public class NpcInteractionHandler
    {
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
                Console.WriteLine($"There's no one named '{npc}' here.");
                return;
            }

            switch (npc.ToLower())
            {
                case "h":
                case "healer":
                    HealerMenu(ref player);
                    break;
                case "s":
                case "smith":
                    SmithMenu(ref player);
                    break;
                case "q":
                case "quest board":
                case "questboard":
                    InteractWithQuestBoard(player);
                    break;
                default:
                    Console.WriteLine($"'{npc}' doesn't do anything... yet.");
                    break;
            }
        }

        public static void InteractWithQuestBoard(Player player)
        {
            var available = QuestManager.GetAvailableForPlayer(player);

            if (available.Count == 0) { Console.WriteLine("📜 No new quests available."); return; }

            while (true)
            {
                if (available.Count < 1) return;
                for (int i = 0; i < available.Count; i++)
                {
                    var q = available[i];
                    Console.WriteLine($"{i + 1}. {q.Name} - {q.Description}");
                }

                Console.Write("Select quest to accept (0 to cancel): ");
                if (!int.TryParse(Console.ReadLine(), out int choice)) { Console.WriteLine("Wrong input type! Please only write a number."); continue; }

                if (choice == 0) return;
                if (choice < 1 || choice > available.Count) { Console.WriteLine("That quest doesn't exist."); continue; }

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
                    case "1": ShowSmithStock(player); break;
                    case "2": HandleUpgrade(player); break;
                    case "3": HandleCrafting(player); break;
                    case "4": Console.WriteLine("You leave the smith."); return;
                    default: Console.WriteLine("❌ Invalid option."); break;
                }
            }
        }

        private static void HandleCrafting(Player player)
        {
            while (true)
            {
                Console.WriteLine("\n🧪 Available Crafting Options:");
                Console.WriteLine("1. Craft Upgrade Stone (requires: 3 iron_ore)");
                Console.WriteLine("0. Cancel");
                Console.Write("Choose: ");
                string? input = Console.ReadLine();

                if (input == "0") return;
                if (input != "1") continue;

                const string requiredMaterial = "iron_ore";
                const string craftedItemId = "upgrade_stone";
                const int cost = 3;

                var oreStack = player.Inventory.Items.FirstOrDefault(i => i.Id == requiredMaterial);
                if (oreStack == null || oreStack.StackSize < cost)
                {
                    Console.WriteLine($"❌ You need at least {cost} iron ore to craft an Upgrade Stone.");
                    continue;
                }

                if (oreStack.StackSize == cost)
                    player.Inventory.RemoveItem(oreStack);
                else
                    oreStack.StackSize -= cost;

                if (ItemFactory.TryCreateItem(craftedItemId, out var crafted))
                {
                    player.Inventory.AddItem(crafted, player);
                    JobManager.GrantSkillXp(player, "blacksmith", 20);
                    Console.WriteLine($"✔ You crafted 1 {crafted.Name}.");
                }
                else
                {
                    Console.WriteLine($"❌ Could not create item '{craftedItemId}'.");
                }
            }
        }

        private static void HandleUpgrade(Player player)
        {
            var upgradable = player.Inventory.Items.OfType<EquipmentItem>()
                .Where(e => e.UpgradeLevel < 9).ToList();

            if (!upgradable.Any()) { Console.WriteLine("❌ You have no equipment that can be upgraded."); return; }

            while (true)
            {
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
                {
                    JobManager.GrantSkillXp(player, "blacksmith", 25);
                    Console.WriteLine($"🛠 {item.Name} is now +{item.UpgradeLevel}!");
                }
            }
        }

        private static void ShowSmithStock(Player player)
        {
            var stock = ItemFactory.GetSmithItemsFor(player).OfType<EquipmentItem>().ToList();

            if (ItemFactory.TryCreateItem("pickaxe", out var pickaxe))       stock.Add((EquipmentItem)pickaxe);
            if (ItemFactory.TryCreateItem("woodcutter_axe", out var axe))    stock.Add((EquipmentItem)axe);

            if (stock.Count == 0) { Console.WriteLine("The smith has nothing for your class."); return; }

            while (true)
            {
                for (int i = 0; i < stock.Count; i++)
                    Console.WriteLine($"{i + 1}. {stock[i].Name} - {stock[i].BuyPrice} bronze");

                Console.Write("Choose item to buy (0 to cancel): ");
                if (!int.TryParse(Console.ReadLine(), out int choice))
                {
                    Console.WriteLine("❌ Wrong input type! Please only write a number.");
                    continue;
                }
                if (choice == 0) { Console.WriteLine("Cancelled."); return; }
                if (choice < 1 || choice > stock.Count) { Console.WriteLine("❌ The smith does not sell such item."); continue; }

                TryBuyItem(stock[choice - 1], stock[choice - 1].BuyPrice, ref player);
            }
        }

        private static void HealerMenu(ref Player player)
        {
            Console.WriteLine("\n🧙 You approach the healer.");
            while (true)
            {
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
                        player.CurrentHealth = player.MaxHealth;
                        player.CurrentMana = player.MaxMana;
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
                    case "5":
                        Console.WriteLine("You leave the healer.");
                        return;
                    default:
                        Console.WriteLine("❌ Wrong input.");
                        break;
                }
            }
        }

        private static void SellItem(Player player)
        {
            while (true)
            {
                player.Inventory.ListItems();
                Console.Write("Enter the item name to sell or type back to exit: ");
                string? itemName = Console.ReadLine()?.Trim();

                if (itemName is "back" or "exit" or "0") return;

                var item = InventoryUtils.ResolveInventoryItem(itemName, player);
                if (item == null) { Console.WriteLine($"❌ You don't have an item named '{itemName}'."); continue; }

                Console.WriteLine($"You have {item.StackSize}x {item.Name}. Each sells for {item.SellValue} bronze.");
                Console.Write("How many would you like to sell? ");

                if (!int.TryParse(Console.ReadLine(), out int amount) || amount <= 0 || amount > item.StackSize)
                {
                    Console.WriteLine("❌ Invalid quantity.");
                    continue;
                }

                int total = amount * item.SellValue;
                Console.Write($"Sell {amount}x {item.Name} for {total} bronze? (yes/no): ");
                string? confirm = Console.ReadLine()?.Trim().ToLower();

                if (confirm is "yes" or "y")
                {
                    if (player.Inventory.SellItem(item.Name, amount, ref player))
                        Console.WriteLine($"🪙 Sold {amount}x {item.Name} for {total} bronze.");
                    else
                        Console.WriteLine("❌ Something went wrong while selling.");
                }
                else
                {
                    Console.WriteLine("Cancelled.");
                }
            }
        }

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
            {
                Console.WriteLine("❌ Not enough money.");
            }
        }

        public static void SkillMasterMenu(Player player)
        {
            Console.WriteLine("\n🔮 Skill fusion is not yet available in this version.");
        }

    }

}
