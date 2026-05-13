using MyriaLib.Services;
using MyriaLib.Systems.Enums;

namespace ConsoleWorldRPG.Entities.NPCs
{
    public static class NpcInteractionHandler
    {
        // ── Entry point ──────────────────────────────────────────────────────────

        public static void InteractWithNpc(string input, Player player)
        {
            var npc = FindNpc(input, player);
            if (npc == null)
            {
                Console.WriteLine($"There's no one named '{input}' here.");
                return;
            }

            Console.WriteLine($"\n🗨  You approach the {npc.Type}.");

            // CN3: offer quest returns first (show regardless of NPC, since GiverNpcId
            // is not yet set in quests.json — TODO: filter by npcId once data is ready)
            OfferQuestReturns(player, npc);

            // Quest acceptance — show quests available from this NPC.
            // Falls back to all available quests while GiverNpcId data is incomplete.
            OfferQuestAcceptance(player, npc);

            // Service-specific menu
            switch (npc.Type)
            {
                case NpcType.Healer:         HealerMenu(player, npc); break;
                case NpcType.Smith:
                case NpcType.Leathersmith:
                case NpcType.Tailor:
                case NpcType.Artificer:
                case NpcType.Enchanter:
                case NpcType.Alchemist:
                case NpcType.Cook:           SmithMenu(player, npc); break;
                case NpcType.Woodcutter:
                case NpcType.Miner:
                case NpcType.Herbalist:      GatheringMasterMenu(player, npc); break;
                case NpcType.Shop:           ShopMenu(player, npc); break;
                case NpcType.SkillMaster:
                    if (npc.Services.Contains("change_class"))
                        ClassMasterMenu(player);
                    else
                        SkillMasterMenu(player);
                    break;
                case NpcType.Villager:
                default:
                    Console.WriteLine($"{npc.Type} has nothing else to offer right now.");
                    break;
            }
        }

        // ── NPC lookup ───────────────────────────────────────────────────────────

        private static Npc? FindNpc(string input, Player player)
        {
            // Try: exact ID, ID-prefix, then NpcType name — all case-insensitive.
            var refs = player.CurrentRoom.NpcRefs;
            if (refs.Count == 0) return null;

            return refs.FirstOrDefault(n =>
                       n.Id.Equals(input, StringComparison.OrdinalIgnoreCase))
                ?? refs.FirstOrDefault(n =>
                       n.Id.StartsWith(input, StringComparison.OrdinalIgnoreCase))
                ?? refs.FirstOrDefault(n =>
                       n.Type.ToString().Equals(input.Replace("_", ""), StringComparison.OrdinalIgnoreCase));
        }

        // ── CN3: Quest return flow ───────────────────────────────────────────────

        private static void OfferQuestReturns(Player player, Npc npc)
        {
            // Filter: quests where ReturnNpcIdResolved matches the current NPC.
            // Currently falls back to all completed quests since GiverNpcId is not
            // set in quests.json yet. Once quest data is updated, switch to
            // QuestManager.GetReturnableForNpc(player, npc.Id).
            var returnable = player.ActiveQuests
                .Where(q => q.Status == QuestStatus.Completed)
                .ToList();

            if (returnable.Count == 0) return;

            Console.WriteLine("\n📜 You have quests ready to return:");
            for (int i = 0; i < returnable.Count; i++)
                Console.WriteLine($"  {i + 1}. {returnable[i].Name}");

            Console.Write("Return a quest? (number / 0 to skip): ");
            if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > returnable.Count)
                return;

            var quest = returnable[choice - 1];

            // CN8: play return dialog before completing
            if (!PlayDialog(quest, player, npc, isReturn: true))
            {
                Console.WriteLine("Quest return cancelled.");
                return;
            }

            quest.GrantRewards(player);

            if (quest.IsRepeatable)
            {
                if (!player.RepeatableQuestRecords.TryGetValue(quest.Id, out var rec))
                {
                    rec = new RepeatRecord();
                    player.RepeatableQuestRecords[quest.Id] = rec;
                }
                if (rec.LastCompletionDate?.Date != DateTime.Today)
                    rec.CompletionsToday = 0;
                rec.TimesCompleted++;
                rec.CompletionsToday++;
                rec.LastCompletionDate = DateTime.Now;
                player.ActiveQuests.Remove(quest);
            }
            else
            {
                quest.Status = QuestStatus.Returned;
                player.CompletedQuests.Add(quest);
                player.ActiveQuests.Remove(quest);
            }

            Console.WriteLine($"✅ Quest '{quest.Name}' returned! Rewards granted.");
        }

        // ── Quest acceptance ─────────────────────────────────────────────────────

        private static void OfferQuestAcceptance(Player player, Npc npc)
        {
            // When GiverNpcId is set in quests.json, switch to:
            // var available = QuestManager.GetAcceptableForNpc(player, npc.Id);
            // CurrentPartyMembers is the full party list (includes self); 0 means solo.
            int partySize = Math.Max(1, ConsoleHubClient.CurrentPartyMembers.Count);
            var available = QuestManager.GetAvailableForPlayer(player, partySize)
                .Where(q => !player.ActiveQuests.Any(a => a.Id == q.Id))
                .ToList();

            if (available.Count == 0) return;

            Console.WriteLine("\n📋 Available quests:");
            for (int i = 0; i < available.Count; i++)
                Console.WriteLine($"  {i + 1}. [{available[i].RequiredLevel}] {available[i].Name} — {available[i].Description}");

            Console.Write("Accept a quest? (number / 0 to skip): ");
            if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > available.Count)
                return;

            var template = available[choice - 1];

            // CN8: play accept dialog; player can still decline at the end
            if (!PlayDialog(template, player, npc, isReturn: false))
            {
                Console.WriteLine("Quest declined.");
                return;
            }

            var quest = template.Clone();
            quest.Status = quest.IsTalkOnly ? QuestStatus.Completed : QuestStatus.InProgress;
            foreach (var key in quest.RequiredKills.Keys) quest.KillProgress[key] = 0;
            foreach (var key in quest.RequiredItems.Keys) quest.ItemProgress[key] = 0;
            quest.GrantAcceptItems(player);
            player.ActiveQuests.Add(quest);

            Console.WriteLine(quest.IsTalkOnly
                ? $"✔ Quest accepted and completed: {quest.Name}"
                : $"✔ Quest accepted: {quest.Name}");
        }

        // ── CN8: Quest dialog ─────────────────────────────────────────────────────

        private static bool PlayDialog(Quest quest, Player player, Npc npc, bool isReturn)
        {
            var lines = isReturn ? quest.ReturnDialog : quest.AcceptDialog;
            if (lines == null || lines.Count == 0)
                lines = new List<DialogLine> { new() { Speaker = "npc", Text = "..." } };

            Console.WriteLine();
            string header = $" {quest.Name} [Lv {quest.RequiredLevel}] ";
            int padLen = Math.Max(0, 60 - header.Length);
            Console.WriteLine("─" + header + new string('─', padLen));

            for (int i = 0; i < lines.Count; i++)
            {
                string speaker = ResolveSpeakerConsole(lines[i].Speaker, player, npc);
                Console.WriteLine($"\n  {speaker}: {lines[i].Text}");

                if (i < lines.Count - 1)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("  [Press Enter to continue]");
                    Console.ResetColor();
                    Console.ReadLine();
                }
            }

            Console.WriteLine("\n" + new string('─', 61));
            string prompt = isReturn
                ? "Return this quest and claim your reward? (y/n): "
                : "Accept this quest? (y/n): ";
            Console.Write(prompt);
            return (Console.ReadLine()?.Trim().ToLower() ?? "") == "y";
        }

        private static string ResolveSpeakerConsole(string speaker, Player player, Npc npc)
        {
            if (speaker == "player") return player.Name;
            if (speaker == "npc")    return npc.Type.ToString();
            if (speaker.StartsWith("npc:"))
            {
                string id = speaker[4..];
                return NpcService.TryGet(id, out var other) && other != null
                    ? other.Type.ToString()
                    : id;
            }
            return speaker;
        }

        // ── Healer ───────────────────────────────────────────────────────────────

        private static void HealerMenu(Player player, Npc npc)
        {
            while (true)
            {
                Console.WriteLine("\n1. Heal (free)");
                if (player.PotionTierAvailable < 2)
                {
                    Console.WriteLine("2. Buy Healing Potion (100 bronze)");
                    Console.WriteLine("3. Buy Mana Potion (120 bronze)");
                }
                Console.WriteLine("4. Sell item");
                Console.WriteLine("0. Leave");
                Console.Write("> ");
                switch (Console.ReadLine()?.Trim())
                {
                    case "1":
                        npc.HealingAction(player);
                        Console.WriteLine("✨ You are fully healed.");
                        break;
                    case "2":
                        TryBuy(player, npc, ItemFactory.CreateItem("t1_healing_potion"), 100);
                        break;
                    case "3":
                        TryBuy(player, npc, ItemFactory.CreateItem("t1_mana_potion"), 120);
                        break;
                    case "4":
                        SellMenu(player, npc);
                        break;
                    case "0":
                        Console.WriteLine("You leave.");
                        return;
                    default:
                        Console.WriteLine("❌ Invalid option.");
                        break;
                }
            }
        }

        // ── Smith / craftsperson ─────────────────────────────────────────────────

        private static void SmithMenu(Player player, Npc npc)
        {
            bool hasJobMaster = npc.Services.Contains("learn_job") && !string.IsNullOrEmpty(npc.MasterJobId);

            while (true)
            {
                Console.WriteLine($"\n🔧 Welcome to the {npc.Type}!");
                Console.WriteLine("1. Buy equipment");
                Console.WriteLine("2. Upgrade equipment");
                Console.WriteLine("3. Craft materials");
                Console.WriteLine("4. Sell item");
                if (hasJobMaster)
                    Console.WriteLine($"5. Job progress ({npc.MasterJobId})");
                Console.WriteLine("0. Leave");
                Console.Write("> ");
                var choice = Console.ReadLine()?.Trim();
                switch (choice)
                {
                    case "1": SmithShopMenu(player, npc); break;
                    case "2": UpgradeMenu(player, npc); break;
                    case "3": CraftMenu(player, npc); break;
                    case "4": SellMenu(player, npc); break;
                    case "5" when hasJobMaster: JobMasterMenu(player, npc.MasterJobId!); break;
                    case "0": Console.WriteLine("You leave the smith."); return;
                    default:  Console.WriteLine("❌ Invalid option."); break;
                }
            }
        }

        private static void SmithShopMenu(Player player, Npc npc)
        {
            var stock = npc.ItemRefs.OfType<EquipmentItem>().ToList();
            if (stock.Count == 0) { Console.WriteLine("Nothing available for your class."); return; }

            while (true)
            {
                for (int i = 0; i < stock.Count; i++)
                    Console.WriteLine($"{i + 1}. {stock[i].Name} — {stock[i].BuyPrice} bronze");
                Console.Write("Buy (number / 0 to cancel): ");

                if (!int.TryParse(Console.ReadLine(), out int choice)) { Console.WriteLine("❌ Enter a number."); continue; }
                if (choice == 0) return;
                if (choice < 1 || choice > stock.Count) { Console.WriteLine("❌ No such item."); continue; }
                TryBuy(player, npc, stock[choice - 1], stock[choice - 1].BuyPrice);
            }
        }

        private static void UpgradeMenu(Player player, Npc npc)
        {
            string jobId       = npc.MasterJobId ?? "blacksmith"; // J16: use NPC's actual job
            int    knowledgeLv = JobXpService.GetLevel(JobManager.GetOrAdd(player, jobId).KnowledgeXp);
            int    maxUpgrade  = JobXpService.GetMaxUpgradeLevel(knowledgeLv); // J19

            var upgradable = player.Inventory.Items.OfType<EquipmentItem>()
                .Where(e => e.UpgradeCategory == npc.UpgradeCategory && e.UpgradeLevel < maxUpgrade)
                .ToList();

            if (upgradable.Count == 0)
            {
                Console.WriteLine($"No equipment eligible for upgrade here (max +{maxUpgrade} at your knowledge level).");
                return;
            }

            while (true)
            {
                Console.WriteLine($"\n🔧 Upgrade (max +{maxUpgrade}):");
                for (int i = 0; i < upgradable.Count; i++)
                    Console.WriteLine($"{i + 1}. {upgradable[i].Name} +{upgradable[i].UpgradeLevel}");
                Console.Write("Choice (0 to cancel): ");

                if (!int.TryParse(Console.ReadLine(), out int choice) || choice == 0) return;
                if (choice < 1 || choice > upgradable.Count) continue;

                var item = upgradable[choice - 1];

                if (ConsoleHubClient.IsConnected)
                {
                    var result = ConsoleHubClient.UpgradeAsync(npc.Id, item.Id).GetAwaiter().GetResult();
                    if (result is null || !result.Success)
                    {
                        Console.WriteLine($"❌ {result?.Reason ?? "Upgrade failed."}");
                        continue;
                    }
                    item.UpgradeLevel = result.UpgradeLevel;
                    if (!string.IsNullOrEmpty(result.JobId) && result.SkillXpGained > 0)
                        JobManager.GrantSkillXp(player, result.JobId, (int)result.SkillXpGained);
                    Console.WriteLine($"🛠 {item.Name} is now +{item.UpgradeLevel}!");
                }
                else
                {
                    // J16: apply skill quality before upgrading
                    float quality = (float)JobXpService.GetSkillMultiplierFromXp(JobManager.GetOrAdd(player, jobId).SkillXp);
                    if (quality > item.CraftQuality) item.CraftQuality = quality;

                    if (item.TryUpgrade(player, maxUpgrade))
                    {
                        JobManager.GrantSkillXp(player, jobId, 25);
                        Console.WriteLine($"🛠 {item.Name} is now +{item.UpgradeLevel}!");
                        upgradable = player.Inventory.Items.OfType<EquipmentItem>()
                            .Where(e => e.UpgradeCategory == npc.UpgradeCategory && e.UpgradeLevel < maxUpgrade)
                            .ToList();
                    }
                    else
                    {
                        Console.WriteLine("❌ Upgrade failed.");
                    }
                }
            }
        }

        private static void CraftMenu(Player player, Npc npc)
        {
            // J20: filter recipes by player's Knowledge level for this NPC's job
            string jobId      = npc.MasterJobId ?? "blacksmith";
            int    knowledge  = JobXpService.GetLevel(JobManager.GetOrAdd(player, jobId).KnowledgeXp);
            var    recipes    = CraftingService.GetRecipes(npc.Id)
                                    .Where(r => r.RequiredKnowledgeLevel <= knowledge)
                                    .ToArray();

            if (recipes.Length == 0)
            {
                Console.WriteLine("No recipes available at your current knowledge level.");
                return;
            }

            while (true)
            {
                Console.WriteLine($"\n🧪 Crafting ({jobId}, knowledge Lv {knowledge}):");
                for (int i = 0; i < recipes.Length; i++)
                {
                    var r = recipes[i];
                    string ingredients = string.Join(", ", r.Ingredients.Select(ing => $"{ing.Amount}x {ing.ItemId}"));
                    Console.WriteLine($"  {i + 1}. {r.OutputId}  [{ingredients}]  (XP: {r.XpReward})");
                }
                Console.WriteLine("  0. Cancel");
                Console.Write("> ");

                var input = Console.ReadLine()?.Trim();
                if (input == "0") return;
                if (!int.TryParse(input, out int choice) || choice < 1 || choice > recipes.Length)
                {
                    Console.WriteLine("❌ Invalid option.");
                    continue;
                }

                var recipe = recipes[choice - 1];

                Console.Write("Quantity (default 1): ");
                string? qInput = Console.ReadLine()?.Trim();
                int qty = string.IsNullOrEmpty(qInput) ? 1 : (int.TryParse(qInput, out int q) && q > 0 ? q : 1);

                if (ConsoleHubClient.IsConnected)
                {
                    var result = ConsoleHubClient.CraftAsync(npc.Id, recipe.OutputId, qty).GetAwaiter().GetResult();
                    if (result is null || !result.Success)
                    {
                        Console.WriteLine($"❌ {result?.Reason ?? "Craft failed."}");
                        continue;
                    }
                    // Apply server delta locally
                    foreach (var ing in recipe.Ingredients)
                    {
                        int remaining = ing.Amount * qty;
                        foreach (var stack in player.Inventory.Items.Where(i => i.Id == ing.ItemId).ToList())
                        {
                            if (remaining <= 0) break;
                            int take = Math.Min(stack.StackSize, remaining);
                            stack.StackSize -= take;
                            remaining -= take;
                            if (stack.StackSize <= 0) player.Inventory.RemoveItem(stack);
                        }
                    }
                    if (result.ItemId != null && ItemFactory.TryCreateItem(result.ItemId, out var serverOut))
                    {
                        serverOut.StackSize = result.Amount;
                        player.Inventory.AddItem(serverOut, player);
                        Console.WriteLine($"✔ Crafted {result.Amount}x {serverOut.Name}.");
                    }
                    if (!string.IsNullOrEmpty(result.JobId) && result.SkillXpGained > 0)
                        JobManager.GrantSkillXp(player, result.JobId, (int)result.SkillXpGained);
                }
                else
                {
                    // Validate ingredients
                    bool hasAll = true;
                    foreach (var ing in recipe.Ingredients)
                    {
                        int owned = player.Inventory.Items.Where(i => i.Id == ing.ItemId).Sum(i => i.StackSize);
                        if (owned < ing.Amount * qty)
                        {
                            Console.WriteLine($"❌ Need {ing.Amount * qty}x {ing.ItemId} (have {owned}).");
                            hasAll = false; break;
                        }
                    }
                    if (!hasAll) continue;

                    // Consume ingredients
                    foreach (var ing in recipe.Ingredients)
                    {
                        int remaining = ing.Amount * qty;
                        foreach (var stack in player.Inventory.Items.Where(i => i.Id == ing.ItemId).ToList())
                        {
                            if (remaining <= 0) break;
                            int take = Math.Min(stack.StackSize, remaining);
                            stack.StackSize -= take;
                            remaining -= take;
                            if (stack.StackSize <= 0) player.Inventory.RemoveItem(stack);
                        }
                    }

                    if (ItemFactory.TryCreateItem(recipe.OutputId, out var output))
                    {
                        // J15: apply skill quality multiplier to equipment output
                        if (output is EquipmentItem crafted)
                        {
                            crafted.CraftQuality = (float)JobXpService.GetSkillMultiplierFromXp(
                                JobManager.GetOrAdd(player, jobId).SkillXp);
                            crafted.Bonuses = crafted.BaseStats.Scale(crafted.CraftQuality);
                        }
                        output.StackSize = qty;
                        player.Inventory.AddItem(output, player);
                        JobManager.GrantSkillXp(player, jobId, recipe.XpReward * qty);
                        Console.WriteLine($"✔ Crafted {qty}x {output.Name}.");
                    }
                    else
                    {
                        Console.WriteLine("❌ Craft failed — item not found.");
                    }
                }
            }
        }

        // ── Gathering master (woodcutter / miner / herbalist) ─────────────────────

        private static void GatheringMasterMenu(Player player, Npc npc)
        {
            bool hasJobMaster = npc.Services.Contains("learn_job") && !string.IsNullOrEmpty(npc.MasterJobId);

            while (true)
            {
                Console.WriteLine($"\n🌿 Welcome to the {npc.Type}!");
                if (npc.Services.Contains("shop_equipment") || npc.Services.Contains("shop_general"))
                    Console.WriteLine("1. Browse stock");
                Console.WriteLine("4. Sell item");
                if (hasJobMaster)
                    Console.WriteLine($"5. Job progress ({npc.MasterJobId})");
                Console.WriteLine("0. Leave");
                Console.Write("> ");
                var choice = Console.ReadLine()?.Trim();
                switch (choice)
                {
                    case "1" when npc.Services.Contains("shop_equipment") || npc.Services.Contains("shop_general"):
                        ShopMenu(player, npc); break;
                    case "4": SellMenu(player, npc); break;
                    case "5" when hasJobMaster: JobMasterMenu(player, npc.MasterJobId!); break;
                    case "0": Console.WriteLine("You leave."); return;
                    default:  Console.WriteLine("❌ Invalid option."); break;
                }
            }
        }

        // ── General shop ─────────────────────────────────────────────────────────

        private static void ShopMenu(Player player, Npc npc)
        {
            var stock = npc.ItemRefs.ToList();
            if (stock.Count == 0) { Console.WriteLine("The shop has nothing in stock."); return; }

            while (true)
            {
                Console.WriteLine("\n🛒 Shop:");
                for (int i = 0; i < stock.Count; i++)
                    Console.WriteLine($"{i + 1}. {stock[i].Name} — {stock[i].BuyPrice} bronze");
                Console.WriteLine("s. Sell item  |  0. Leave");
                Console.Write("> ");
                var input = Console.ReadLine()?.Trim().ToLower();

                if (input == "0") return;
                if (input == "s") { SellMenu(player, npc); continue; }
                if (!int.TryParse(input, out int choice)) { Console.WriteLine("❌ Enter a number."); continue; }
                if (choice < 1 || choice > stock.Count) { Console.WriteLine("❌ No such item."); continue; }
                TryBuy(player, npc, stock[choice - 1], stock[choice - 1].BuyPrice);
            }
        }

        // ── Sell ─────────────────────────────────────────────────────────────────

        private static void SellMenu(Player player, Npc npc)
        {
            while (true)
            {
                player.Inventory.ListItems();
                Console.Write("Enter item name to sell (or 'back'): ");
                var itemName = Console.ReadLine()?.Trim();
                if (itemName is "back" or "0" or null) return;

                var item = InventoryUtils.ResolveInventoryItem(itemName, player);
                if (item == null) { Console.WriteLine($"❌ You don't have '{itemName}'."); continue; }

                Console.WriteLine($"{item.StackSize}x {item.Name} — {item.SellValue} bronze each.");
                Console.Write("How many to sell? ");
                if (!int.TryParse(Console.ReadLine(), out int amount) || amount <= 0 || amount > item.StackSize)
                {
                    Console.WriteLine("❌ Invalid quantity.");
                    continue;
                }

                var result = npc.SellItem(player, item, amount);
                Console.WriteLine(result.Success
                    ? $"🪙 Sold {amount}x {item.Name} for {amount * item.SellValue} bronze."
                    : $"❌ {result.MessageKey}");
            }
        }

        // ── Job master ───────────────────────────────────────────────────────────

        private static void JobMasterMenu(Player player, string jobId)
        {
            var job   = JobManager.GetById(jobId);
            var entry = JobManager.GetOrAdd(player, jobId);

            while (true)
            {
                string jobName = job?.Name ?? jobId;
                string jobType = string.IsNullOrEmpty(job?.Type) ? "" : $" ({job.Type})";
                bool   isActive = player.ActiveJobId == jobId;

                Console.WriteLine($"\n== {jobName}{jobType} ==");
                if (!string.IsNullOrEmpty(job?.Description))
                    Console.WriteLine(job.Description);
                Console.WriteLine();
                Console.WriteLine($"  Skill:     Lv {JobXpService.GetLevel(entry.SkillXp),-3}  {JobXpService.FormatProgress(entry.SkillXp)}");
                Console.WriteLine($"  Knowledge: Lv {JobXpService.GetLevel(entry.KnowledgeXp),-3}  {JobXpService.FormatProgress(entry.KnowledgeXp)}");
                Console.WriteLine($"  Fame:      Lv {JobXpService.GetLevel(entry.FameXp),-3}  {JobXpService.FormatProgress(entry.FameXp)}");
                Console.WriteLine();

                if (isActive)
                {
                    Console.WriteLine("  Status: ACTIVE JOB  (Fame XP and daily Fame ticks apply)");
                    Console.WriteLine("1. Stop working this job");
                }
                else
                {
                    var cd = JobManager.GetCooldownRemaining(player);
                    if (cd > TimeSpan.Zero)
                        Console.WriteLine($"  Switch cooldown: {Printer.FormatCooldown(cd)} remaining");
                    Console.WriteLine($"1. Work as {jobName}");
                }
                Console.WriteLine("0. Leave");
                Console.Write("> ");
                string input = Console.ReadLine()?.Trim() ?? "";
                if (input == "0") return;
                if (input != "1") { Console.WriteLine("❌ Invalid option."); continue; }

                if (isActive)
                {
                    JobManager.SetActiveJob(player, null);
                    Console.WriteLine($"You are no longer working as {jobName}.");
                }
                else
                {
                    if (JobManager.SetActiveJob(player, jobId))
                        Console.WriteLine($"✅ You are now working as {jobName}!");
                    else
                    {
                        var cd = JobManager.GetCooldownRemaining(player);
                        Console.WriteLine($"❌ You must wait {Printer.FormatCooldown(cd)} before switching jobs.");
                    }
                }
            }
        }

        // ── Class master ─────────────────────────────────────────────────────────

        private static void ClassMasterMenu(Player player)
        {
            var allClasses  = Enum.GetValues<PlayerClass>().ToList();
            var raceProfile = RaceProfile.All[player.Race];

            while (true)
            {
                int  activeLvl = ClassManager.GetClassLevel(player, player.Class);
                long activeXp  = ClassManager.GetClassXp(player, player.Class);

                Console.WriteLine($"\n== Class Master ==");
                Console.WriteLine($"Active class: {player.Class}  (Lv {activeLvl}  {JobXpService.FormatProgress(activeXp)})");
                Console.WriteLine();

                for (int i = 0; i < allClasses.Count; i++)
                {
                    var    cls       = allClasses[i];
                    bool   isActive  = cls == player.Class;
                    bool   forbidden = raceProfile.ForbiddenClasses.Contains(cls);
                    int    lvl       = ClassManager.GetClassLevel(player, cls);
                    string progress  = JobXpService.FormatProgress(ClassManager.GetClassXp(player, cls));

                    string tag = isActive ? " [ACTIVE]" : forbidden ? " [FORBIDDEN]" : "";
                    Console.WriteLine($"  {i + 1,2}. {cls,-16}{tag,-12}  Lv {lvl,2}  {progress}");
                }

                Console.WriteLine("   0. Leave");
                Console.Write("> ");
                string input = Console.ReadLine()?.Trim() ?? "";
                if (input == "0") return;

                if (!int.TryParse(input, out int choice) || choice < 1 || choice > allClasses.Count)
                {
                    Console.WriteLine("❌ Invalid option.");
                    continue;
                }

                var chosen = allClasses[choice - 1];

                if (chosen == player.Class)
                {
                    Console.WriteLine("That is already your active class.");
                    continue;
                }

                if (raceProfile.ForbiddenClasses.Contains(chosen))
                {
                    Console.WriteLine($"❌ {chosen} is not available for your race ({player.Race}).");
                    continue;
                }

                if (!ClassManager.CanChangeClass(player))
                {
                    var remaining = ClassManager.GetClassChangeCooldownRemaining(player);
                    int days = (int)Math.Ceiling(remaining.TotalDays);
                    Console.WriteLine($"⏳ Class switch on cooldown — {days} day(s) remaining.");
                    continue;
                }

                ClassManager.SetClass(player, chosen);
                Console.WriteLine($"✅ Class changed to {player.Class}!");
            }
        }

        // ── Skill master ─────────────────────────────────────────────────────────

        public static void SkillMasterMenu(Player player)
        {
            while (true)
            {
                Console.WriteLine("\n🔮 Skill Master");
                Console.WriteLine("1. View skills");
                Console.WriteLine("2. Combine skills");
                Console.WriteLine("3. Manage skill slots");
                Console.WriteLine("4. Skill fusion  (not yet available — base_skills.json missing)");
                Console.WriteLine("0. Leave");
                Console.Write("> ");
                switch (Console.ReadLine()?.Trim())
                {
                    case "1": SkillCommands.ShowSkillsMenu(player); break;
                    case "2": SkillCommands.CombineMenu(player);    break;
                    case "3": SkillCommands.SlotsMenu(player);      break;
                    case "4": Console.WriteLine("Skill fusion requires base_skills.json, which is not yet authored."); break;
                    case "0": return;
                    default:  Console.WriteLine("❌ Invalid option."); break;
                }
            }
        }

        // ── Shared buy helper ─────────────────────────────────────────────────────

        private static void TryBuy(Player player, Npc npc, Item item, int cost)
        {
            var result = npc.BuyItem(player, item);
            Console.WriteLine(result.Success
                ? $"✔ {item.Name} added to inventory."
                : $"❌ {result.MessageKey}");
        }
    }
}
