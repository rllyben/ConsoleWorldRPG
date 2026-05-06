using LibCombat = MyriaLib.Systems.CombatSystem;

namespace ConsoleWorldRPG.Systems
{
    public static class EncounterRunner
    {
        private static readonly Random _random = Random.Shared;

        public static void StartEncounter(Player player)
        {
            var encounters = player.CurrentRoom.EncounterableMonsters;
            Monster monster;

            if (encounters == null || encounters.Count == 0 || player.CurrentRoom.IsCleared)
            {
                Console.WriteLine("But nothing stirs in the darkness...");
                return;
            }

            if (player.CurrentRoom.IsDungeonRoom && player.CurrentRoom.CurrentMonsters.Count < 1)
            {
                if (player.CurrentRoom.IsBossRoom && player.CurrentRoom.Monsters.Count > 0)
                {
                    player.CurrentRoom.CurrentMonsters.Add(player.CurrentRoom.Monsters[0].Clone());
                }
                else
                {
                    int count = _random.Next(1, 11);
                    for (int i = 0; i < count; i++)
                        player.CurrentRoom.CurrentMonsters.Add(
                            player.CurrentRoom.Monsters[_random.Next(0, player.CurrentRoom.Monsters.Count)].Clone());
                }
                monster = player.CurrentRoom.CurrentMonsters[_random.Next(0, player.CurrentRoom.CurrentMonsters.Count)];
            }
            else if (player.CurrentRoom.IsDungeonRoom)
            {
                monster = player.CurrentRoom.CurrentMonsters[_random.Next(0, player.CurrentRoom.CurrentMonsters.Count)];
            }
            else
            {
                monster = player.CurrentRoom.Monsters[_random.Next(0, player.CurrentRoom.Monsters.Count)].Clone();
            }
            monster.ResetHealth();

            Console.WriteLine($"\nA wild {monster.Name} appears!");
            Console.WriteLine(monster.Description);

            int pendingTurns = 0;
            Action? pendingExecute = null;
            int recoveryTurns = 0;

            while (player.IsAlive && monster.IsAlive)
            {
                if (pendingTurns > 0)
                {
                    pendingTurns--;
                    if (pendingTurns < 1)
                    {
                        pendingExecute?.Invoke();
                        pendingExecute = null;
                    }
                }
                else if (recoveryTurns > 0)
                {
                    recoveryTurns--;
                }

                if (pendingTurns < 1 && pendingExecute == null && recoveryTurns < 1)
                {
                    Console.WriteLine("\nWhat will you do? (attack / cast <skill> / use <item name>)");
                    Console.Write("> ");
                    var input = Console.ReadLine()?.Trim().ToLower() ?? "";

                    if (input == "attack")
                    {
                        BasicAttack(player, monster);
                    }
                    else if (input == "cast ?" || input == "cast")
                    {
                        Console.WriteLine("Your Skills:");
                        if (player.Skills.Count < 1)
                            Console.WriteLine("no skills yet");
                        foreach (var skill in player.Skills)
                            Console.WriteLine($"  {skill.Name}");
                        continue;
                    }
                    else if (input.StartsWith("cast "))
                    {
                        var skillName = input.Substring(5).Trim();
                        var skill = player.Skills.FirstOrDefault(s =>
                            s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));

                        if (skill == null) { Console.WriteLine("❌ You don't know that skill."); continue; }
                        if (player.CurrentMana < skill.ManaCost) { Console.WriteLine("❌ Not enough mana."); continue; }

                        if (skill.CastTime > 0)
                        {
                            pendingTurns = skill.CastTime;
                            pendingExecute = () => { UseSkill(player, monster, skill); recoveryTurns = skill.RecoveryTime; };
                            Console.WriteLine($"{player.Name} begins casting {skill.Name}...");
                        }
                        else
                        {
                            UseSkill(player, monster, skill);
                            recoveryTurns = skill.RecoveryTime;
                        }
                    }
                    else if (input.StartsWith("use "))
                    {
                        string itemName = input.Substring(4);
                        var item = InventoryUtils.ResolveInventoryItem(itemName, player);
                        if (item is ConsumableItem consumable)
                        {
                            pendingTurns = 1;
                            pendingExecute = () =>
                            {
                                consumable.Use(player);
                                player.Inventory.RemoveItem(consumable);
                                Console.WriteLine($"{player.Name} drinks {consumable.Name}!");
                                recoveryTurns = 1;
                            };
                            Console.WriteLine($"{player.Name} begins to drink {item.Name}...");
                        }
                        else if (item != null)
                        {
                            Console.WriteLine("You can't use that item in battle!");
                        }
                        else
                        {
                            Console.WriteLine("You don't have that item in your inventory!");
                            continue;
                        }
                    }
                    else if (input == "list")
                    {
                        player.Inventory.ListItems();
                        continue;
                    }
                    else
                    {
                        Console.WriteLine("❓ Unknown command. Use 'attack', 'cast <skill>' or 'use <item name>'");
                        continue;
                    }

                    if (monster.IsAlive)
                        BasicAttack(monster, player);
                }
            }

            if (player.IsAlive)
            {
                if (player.CurrentRoom.IsDungeonRoom && player.CurrentRoom.CurrentMonsters.Count > 0)
                    player.CurrentRoom.CurrentMonsters.Remove(monster);

                Console.WriteLine($"\n✅ You defeated the {monster.Name}!");
                player.GainXp(monster.Exp);
                SkillFactory.UpdateSkills(player);

                foreach (var quest in player.ActiveQuests.Where(q => q.Status == QuestStatus.InProgress))
                {
                    if (quest.RequiredKills.TryGetValue(monster.Id, out int required))
                    {
                        quest.KillProgress[monster.Id] = quest.KillProgress.GetValueOrDefault(monster.Id) + 1;
                        int current = quest.KillProgress[monster.Id];
                        Console.WriteLine($"📜 Quest '{quest.Name}': {monster.Name} slain ({current}/{required})");

                        if (quest.KillProgress.All(kp => kp.Value >= quest.RequiredKills[kp.Key]))
                        {
                            quest.Status = QuestStatus.Completed;
                            Console.WriteLine($"✅ Quest '{quest.Name}' is now complete!");
                            quest.GrantRewards(player);
                        }
                    }
                }

                var drops = LootGenerator.GetLootFor(monster);
                if (drops.Count > 0)
                {
                    if (monster.DropsCorpse)
                    {
                        player.CurrentRoom.Corpses.Add(new Corpse(monster.Name, drops));
                        Console.WriteLine($"The corpse of {monster.Name} remains. You can loot it.");
                    }
                    else
                    {
                        foreach (var drop in drops)
                        {
                            if (player.Inventory.AddItem(drop, player))
                            {
                                Console.Write("🪶 You found: ");
                                Printer.PrintColoredItemName(drop);
                                Console.WriteLine();
                            }
                            else
                            {
                                Console.Write("❌ Inventory full. Could not take: ");
                                Printer.PrintColoredItemName(drop);
                                Console.WriteLine();
                            }
                        }
                    }
                }

                if (player.CurrentRoom.IsDungeonRoom && player.CurrentRoom.CurrentMonsters.Count < 1)
                {
                    player.CurrentRoom.IsCleared = true;
                    Console.WriteLine("✅ The room has been cleared. The path forward is open.");
                }
            }
            else
            {
                Console.WriteLine("💀 You were defeated...");

                int respawnRoomId = player.LastHealerRoomId ?? 1;
                var respawnRoom = GameService.Rooms[respawnRoomId];

                player.CurrentRoom = respawnRoom;
                player.CurrentRoomId = respawnRoomId;
                player.CurrentHealth = player.MaxHealth;
                player.CurrentMana = player.MaxMana;

                Console.WriteLine($"🌀 You awaken in {respawnRoom.Name}.");
                player.ApplyDeathXpPenalty();
            }
        }

        private static void BasicAttack(ICombatant attacker, ICombatant defender)
        {
            Console.WriteLine($"{attacker.Name} attacks!");
            int damage = LibCombat.CalculateDamage(attacker, defender);
            if (damage == 0)
            {
                Console.WriteLine($"{attacker.Name} missed!");
                return;
            }
            defender.TakeDamage(damage);
        }

        private static void UseSkill(Player player, Monster target, Skill skill)
        {
            Console.WriteLine($"{player.Name} casts {skill.Name}!");

            int baseStat = skill.StatToScaleFrom?.ToUpper() switch
            {
                "ATK"  => player.TotalPhysicalAttack,
                "MATK" => player.TotalMagicAttack,
                "SPR"  => player.TotalSPR,
                "INT"  => player.TotalINT,
                "DEX"  => player.TotalDEX,
                "AIM"  => player.TotalAim * 2,
                "EVA"  => player.TotalEvasion * 2,
                "END"  => player.TotalEND,
                "STR"  => player.TotalSTR,
                _      => player.TotalPhysicalAttack
            };

            int value = (int)(baseStat * skill.ScalingFactor);
            player.CurrentMana -= skill.ManaCost;

            if (skill.IsHealing)
            {
                int healed = Math.Min(value, player.MaxHealth - player.CurrentHealth);
                player.CurrentHealth += healed;
                Console.WriteLine($"{player.Name} heals for {healed} HP!");
            }
            else
            {
                Console.WriteLine($"{target.Name} takes {value} damage from {skill.Name}!");
                target.TakeDamage(value);
            }
        }

    }

}
