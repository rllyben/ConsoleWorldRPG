namespace ConsoleWorldRPG.Systems
{
    public static class EncounterRunner
    {
        private static readonly Random _random = Random.Shared;

        // Console-side English templates for CombatEncounter log keys.
        // When a new key is added to CombatEncounter, add its template here.
        private static readonly Dictionary<string, string> _logTemplates = new()
        {
            ["pg.fight.log.hit"]       = "  {0} deals {1} damage.",
            ["pg.fight.log.miss"]      = "  {0} misses!",
            ["pg.fight.log.skillHit"]  = "  {0} uses {1} for {2} damage!",
            ["pg.fight.log.heal"]      = "  {0} heals {1} HP.",
            ["pg.fight.log.enemyHit"]  = "  {0} hits you for {1} damage!",
            ["pg.fight.log.enemyMiss"] = "  {0} missed!",
            ["pg.fight.log.beginCast"] = "  {0} begins casting {1}...",
            ["pg.fight.log.nomana"]    = "  Not enough mana!",
            ["pg.fight.log.usedItem"]  = "  {0} uses {1}.",
        };

        public static void StartEncounter(Player player)
        {
            if (player.CurrentRoom.EncounterableMonsters is not { Count: > 0 }
                || player.CurrentRoom.IsCleared)
            {
                Console.WriteLine("But nothing stirs in the darkness...");
                return;
            }

            if (ConsoleHubClient.IsConnected)
            {
                RunOnlineCombat(player);
                return;
            }

            var monster = SelectMonster(player);
            Console.WriteLine($"\nA wild {monster.Name} appears!");
            Console.WriteLine(monster.Description);

            var encounter = new MyriaLib.Systems.CombatEncounter(player, monster);
            encounter.MonsterKilled += (_, e) => PrintQuestProgress(player, monster);

            int logIndex = 1; // skip the "pg.fight.log.start" entry added by the constructor
            RunCombatLoop(encounter, player, monster, ref logIndex);

            if (player.IsAlive)
            {
                Console.WriteLine($"\n✅ You defeated the {monster.Name}!");
                PrintDrops(encounter);
            }
            else
            {
                HandleDeath(player);
            }
        }

        // ── Online combat ────────────────────────────────────────────────────────

        private static void RunOnlineCombat(Player player)
        {
            var start = ConsoleHubClient.StartCombatAsync(player.CurrentRoom.Id)
                            .GetAwaiter().GetResult();
            if (start is null || !start.Success)
            {
                Console.WriteLine($"❌ Could not start encounter: {start?.Reason ?? "server unavailable"}");
                return;
            }

            string monsterName = start.MonsterName ?? "???";
            Console.WriteLine($"\nA wild {monsterName} appears!");

            string phase     = "PlayerTurn";
            int monsterHp    = start.MonsterHp;
            int monsterMaxHp = start.MonsterMaxHp;

            while (true)
            {
                if (phase is "Casting" or "Recovery")
                {
                    Console.WriteLine($"\n[{phase}...] Press Enter to continue.");
                    Console.ReadLine();
                    var adv = ConsoleHubClient.PlayerAttackAsync().GetAwaiter().GetResult();
                    if (adv is null) break;
                    FlushOnlineLog(adv.LogEntries);
                    player.CurrentHealth = adv.PlayerHp;
                    monsterHp = adv.MonsterHp;
                    phase     = adv.Phase;
                    if (!adv.Finished) continue;
                    if (adv.PlayerWon) OnlineWin(player, monsterName, adv);
                    else               OnlineDeath(player);
                    return;
                }

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\n[HP: {player.CurrentHealth}/{player.MaxHealth} | MP: {player.CurrentMana}/{player.MaxMana}]");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{monsterName} — HP: {monsterHp}/{monsterMaxHp}");
                Console.ResetColor();
                Console.WriteLine("What will you do? (attack / cast <skill> / list)");
                Console.Write("> ");

                var input = Console.ReadLine()?.Trim().ToLower() ?? "";
                CombatTurnResult? result = null;

                if (input == "attack")
                {
                    result = ConsoleHubClient.PlayerAttackAsync().GetAwaiter().GetResult();
                }
                else if (input.StartsWith("cast "))
                {
                    string skillName = input[5..].Trim();
                    var skill = player.Skills.FirstOrDefault(s =>
                        s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));
                    if (skill == null) { Console.WriteLine("❌ You don't know that skill."); continue; }
                    result = ConsoleHubClient.PlayerCastSkillAsync(skill.Id).GetAwaiter().GetResult();
                }
                else if (input is "cast" or "cast ?")
                {
                    PrintSkillList(player);
                    continue;
                }
                else if (input == "list")
                {
                    player.Inventory.ListItems();
                    continue;
                }
                else
                {
                    Console.WriteLine("❓ Use 'attack', 'cast <skill>', or 'list'.");
                    continue;
                }

                if (result is null || !result.Success)
                {
                    Console.WriteLine("❌ Action failed.");
                    continue;
                }

                FlushOnlineLog(result.LogEntries);
                player.CurrentHealth = result.PlayerHp;
                monsterHp = result.MonsterHp;
                phase     = result.Phase;

                if (!result.Finished) continue;
                if (result.PlayerWon) OnlineWin(player, monsterName, result);
                else                  OnlineDeath(player);
                return;
            }
        }

        private static void FlushOnlineLog(List<CombatLogMessage> entries)
        {
            foreach (var entry in entries)
                if (_logTemplates.TryGetValue(entry.Key, out var template))
                    Console.WriteLine(string.Format(template, entry.Args));
        }

        private static void OnlineWin(Player player, string monsterName, CombatTurnResult result)
        {
            Console.WriteLine($"\n✅ You defeated the {monsterName}!");
            if (result.XpGained > 0)
            {
                player.GainXp(result.XpGained);
                Console.WriteLine($"⭐ +{result.XpGained} XP");
            }
            var drops = new List<(string name, int count)>();
            foreach (var id in result.LootItemIds)
                if (ItemFactory.TryCreateItem(id, out var item) && player.Inventory.AddItem(item, player))
                    drops.Add((item.Name, item.StackSize));
            if (drops.Count > 0)
            {
                Console.WriteLine("You found:");
                foreach (var (name, count) in drops)
                    Console.WriteLine($"  🪶 {count}x {name}");
            }
        }

        private static void OnlineDeath(Player player)
        {
            Console.WriteLine("\n💀 You were defeated...");
            int respawnRoomId   = player.LastHealerRoomId ?? 1;
            var respawnRoom     = GameService.Rooms[respawnRoomId];
            player.CurrentRoom  = respawnRoom;
            player.CurrentRoomId = respawnRoomId;
            player.CurrentHealth = player.MaxHealth;
            player.CurrentMana   = player.MaxMana;
            Console.WriteLine($"🌀 You awaken in {respawnRoom.Name}.");
        }

        // ── Monster selection ────────────────────────────────────────────────────

        private static Monster SelectMonster(Player player)
        {
            var room = player.CurrentRoom;

            if (room.IsDungeonRoom && room.CurrentMonsters.Count < 1)
            {
                if (room.IsBossRoom && room.Monsters.Count > 0)
                {
                    room.CurrentMonsters.Add(room.Monsters[0].Clone());
                }
                else
                {
                    int count = _random.Next(1, 11);
                    for (int i = 0; i < count; i++)
                        room.CurrentMonsters.Add(room.Monsters[_random.Next(0, room.Monsters.Count)].Clone());
                }
                return room.CurrentMonsters[_random.Next(0, room.CurrentMonsters.Count)];
            }

            if (room.IsDungeonRoom)
                return room.CurrentMonsters[_random.Next(0, room.CurrentMonsters.Count)];

            return room.Monsters[_random.Next(0, room.Monsters.Count)].Clone();
        }

        // ── Combat loop ──────────────────────────────────────────────────────────

        private static void RunCombatLoop(
            MyriaLib.Systems.CombatEncounter encounter,
            Player player,
            Monster monster,
            ref int logIndex)
        {
            while (encounter.Phase != CombatPhase.Finished)
            {
                switch (encounter.Phase)
                {
                    case CombatPhase.Casting:
                        Console.WriteLine($"\nCasting... ({encounter.TurnsUntilActionExecutes} turn(s) remaining)");
                        Console.WriteLine("[Press Enter to continue]");
                        Console.ReadLine();
                        encounter.Tick();
                        FlushLog(encounter, ref logIndex);
                        break;

                    case CombatPhase.Recovery:
                        Console.WriteLine($"\nRecovering... ({encounter.RecoveryTurnsRemaining} turn(s) remaining)");
                        Console.WriteLine("[Press Enter to continue]");
                        Console.ReadLine();
                        encounter.Tick();
                        FlushLog(encounter, ref logIndex);
                        break;

                    case CombatPhase.PlayerTurn:
                        PrintStatus(player, monster);
                        if (!HandlePlayerTurn(encounter, player, monster, ref logIndex))
                            continue; // unknown or list command — re-prompt without advancing
                        break;
                }
            }
        }

        private static void PrintStatus(Player player, Monster monster)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n[HP: {player.CurrentHealth}/{player.MaxHealth} | MP: {player.CurrentMana}/{player.MaxMana}]");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{monster.Name} — HP: {monster.CurrentHealth}/{monster.MaxHealth}");
            Console.ResetColor();
            Console.WriteLine("What will you do? (attack / cast <skill> / use <item> / list)");
            Console.Write("> ");
        }

        private static bool HandlePlayerTurn(
            MyriaLib.Systems.CombatEncounter encounter,
            Player player,
            Monster monster,
            ref int logIndex)
        {
            var input = Console.ReadLine()?.Trim().ToLower() ?? "";

            if (input == "attack")
            {
                encounter.PlayerAttack();
                FlushLog(encounter, ref logIndex);
                return true;
            }

            if (input is "cast" or "cast ?")
            {
                PrintSkillList(player);
                return false;
            }

            if (input.StartsWith("cast "))
            {
                return TryCast(encounter, player, input[5..].Trim(), ref logIndex);
            }

            if (input.StartsWith("use "))
            {
                return TryUseItem(encounter, player, input[4..].Trim(), ref logIndex);
            }

            if (input == "list")
            {
                player.Inventory.ListItems();
                return false;
            }

            Console.WriteLine("❓ Use 'attack', 'cast <skill>', 'use <item>', or 'list'.");
            return false;
        }

        private static bool TryCast(
            MyriaLib.Systems.CombatEncounter encounter,
            Player player,
            string skillName,
            ref int logIndex)
        {
            var skill = player.Skills.FirstOrDefault(s =>
                s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));

            if (skill == null) { Console.WriteLine("❌ You don't know that skill."); return false; }
            if (player.CurrentMana < skill.ManaCost) { Console.WriteLine("❌ Not enough mana."); return false; }

            bool ok = encounter.PlayerBeginCast(skill);
            if (!ok) return false;

            FlushLog(encounter, ref logIndex);
            return true;
        }

        private static bool TryUseItem(
            MyriaLib.Systems.CombatEncounter encounter,
            Player player,
            string itemName,
            ref int logIndex)
        {
            var item = InventoryUtils.ResolveInventoryItem(itemName, player);

            if (item is ConsumableItem consumable)
            {
                encounter.PlayerUseItem(consumable);
                FlushLog(encounter, ref logIndex);
                return true;
            }

            if (item != null)
                Console.WriteLine("You can't use that item in battle!");
            else
                Console.WriteLine("You don't have that item in your inventory!");
            return false;
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private static void FlushLog(MyriaLib.Systems.CombatEncounter encounter, ref int logIndex)
        {
            while (logIndex < encounter.Log.Count)
            {
                var entry = encounter.Log[logIndex++];
                if (_logTemplates.TryGetValue(entry.Key, out var template))
                    Console.WriteLine(string.Format(template, entry.Args));
            }
        }

        private static void PrintSkillList(Player player)
        {
            Console.WriteLine("Your skills:");
            if (player.Skills.Count == 0) { Console.WriteLine("  (none)"); return; }
            foreach (var s in player.Skills)
                Console.WriteLine($"  {s.Name} — MP: {s.ManaCost}, Cast: {s.CastTime}t");
        }

        private static void PrintQuestProgress(Player player, Monster monster)
        {
            foreach (var quest in player.ActiveQuests
                         .Where(q => q.Status == QuestStatus.InProgress
                                  && q.RequiredKills.ContainsKey(monster.Id)))
            {
                int required = quest.RequiredKills[monster.Id];
                int current  = quest.KillProgress.GetValueOrDefault(monster.Id, 0);
                Console.WriteLine($"📜 Quest '{quest.Name}': {monster.Name} ({current}/{required})");
            }

            foreach (var quest in player.ActiveQuests
                         .Where(q => q.Status == QuestStatus.Completed
                                  && q.RequiredKills.ContainsKey(monster.Id)))
            {
                Console.WriteLine($"✅ Quest '{quest.Name}' complete! Return to an NPC to claim your reward.");
            }
        }

        private static void PrintDrops(MyriaLib.Systems.CombatEncounter encounter)
        {
            var drops = encounter.GetDropNames();
            if (drops.Count > 0)
            {
                Console.WriteLine("You found:");
                foreach (var (name, count) in drops)
                    Console.WriteLine($"  🪶 {count}x {name}");
            }
            if (encounter.InventoryFull)
                Console.WriteLine("❌ Inventory was full; some items could not be picked up.");
        }

        private static void HandleDeath(Player player)
        {
            Console.WriteLine("\n💀 You were defeated...");

            int respawnRoomId   = player.LastHealerRoomId ?? 1;
            var respawnRoom     = GameService.Rooms[respawnRoomId];
            player.CurrentRoom  = respawnRoom;
            player.CurrentRoomId = respawnRoomId;
            player.CurrentHealth = player.MaxHealth;
            player.CurrentMana   = player.MaxMana;

            Console.WriteLine($"🌀 You awaken in {respawnRoom.Name}.");
            player.ApplyDeathXpPenalty();
        }
    }
}
