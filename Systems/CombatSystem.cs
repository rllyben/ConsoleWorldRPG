using System;
using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Enums;
using ConsoleWorldRPG.Interfaces;
using ConsoleWorldRPG.Items;
using ConsoleWorldRPG.Models;
using ConsoleWorldRPG.Services;
using ConsoleWorldRPG.Skills;
using ConsoleWorldRPG.Utils;

namespace ConsoleWorldRPG.Systems
{
    public static class CombatSystem
    {
        private class PendingAction
        {
            public int TurnsRemaining { get; set; }
            public Action Execute { get; set; }
        }

        private static readonly Random _random = new();

        /// <summary>
        /// Starts an Encounter after the look command if the room has monsters, also handels the End of a fight
        /// </summary>
        public static void StartEncounter(Player player)
        {
            Random random = new Random();
            var encounters = player.CurrentRoom.EncounterableMonsters;
            Monster monster = new Monster(-1, "null", new Stats(), "null", 0);

            if (encounters == null || encounters.Count == 0 || player.CurrentRoom.IsCleared)
            {
                Console.WriteLine("But nothing stirs in the darkness...");
                return;
            }

            if (player.CurrentRoom.IsDungeonRoom && player.CurrentRoom.CurrentMonsters.Count < 1)
            {
                if (player.CurrentRoom.IsBossRoom && player.CurrentRoom.Monsters.Count > 0)
                {
                    player.CurrentRoom.CurrentMonsters.Add(player.CurrentRoom.Monsters[0]);
                }
                else
                {
                    for (int count = 0; count < random.Next(1, 11); count++)
                    {
                        player.CurrentRoom.CurrentMonsters.Add(player.CurrentRoom.Monsters[random.Next(0, player.CurrentRoom.Monsters.Count)]);
                    }
                }
                monster = player.CurrentRoom.CurrentMonsters[random.Next(0, player.CurrentRoom.CurrentMonsters.Count)];
                monster.ResetHealth();
            }
            else if (player.CurrentRoom.IsDungeonRoom)
            {
                monster = player.CurrentRoom.CurrentMonsters[random.Next(0, player.CurrentRoom.CurrentMonsters.Count)];
                monster.ResetHealth();
            }
            else
            {
                monster = player.CurrentRoom.Monsters[random.Next(0, player.CurrentRoom.Monsters.Count)];
                monster.ResetHealth();
            }

            Console.WriteLine($"\nA wild {monster.Name} appears!");
            Console.WriteLine(monster.Description);

            PendingAction? activeAction = null;
            int recoveryTurns = 0;

            while (player.IsAlive && monster.IsAlive)
            {
                if (activeAction != null)
                {
                    activeAction.TurnsRemaining--;
                    if (activeAction.TurnsRemaining < 1)
                    {
                        activeAction.Execute();
                        activeAction = null;
                    }
                }
                else if (recoveryTurns > 0)
                    recoveryTurns--;

                if (activeAction == null && recoveryTurns < 1)
                {
                    Console.WriteLine($"\nWhat will you do? (attack / cast <skill> / use <item name>)");
                    Console.Write("> ");
                    var input = Console.ReadLine()?.Trim().ToLower();

                    if (input == "attack")
                    {
                        BasicAttack(player, monster);
                    }
                    else if (input == "cast ?" || input == "cast")
                    {
                        Console.WriteLine("Your Skills:");
                        if (player.Skills.Count < 1)
                            Console.WriteLine("no skills yet");
                        foreach(Skill skill in player.Skills)
                        {
                            Console.WriteLine($"{skill.Name}");
                        }
                        continue;
                    }
                    else if (input.StartsWith("cast "))
                    {

                        var skillName = input.Substring(5).Trim();
                        var skill = player.Skills.FirstOrDefault(s =>
                            s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));

                        if (skill == null)
                        {
                            Console.WriteLine("❌ You don’t know that skill.");
                            continue;
                        }

                        if (player.CurrentMana < skill.ManaCost)
                        {
                            Console.WriteLine("❌ Not enough mana.");
                            continue;
                        }

                        if (skill.CastTime > 0)
                        {
                            activeAction = new PendingAction
                            {
                                TurnsRemaining = skill.CastTime,
                                Execute = () =>
                                {
                                    UseSkill(player, monster, skill);
                                    recoveryTurns = skill.RecoveryTime;
                                }
                            };
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
                        var item = ItemService.GetItemByNameFromInventory(itemName, player.Inventory);
                        if (item != null && item is ConsumableItem)
                        {
                            activeAction = new PendingAction
                            {
                                TurnsRemaining = 1,
                                Execute = () =>
                                {
                                    item.Use(player);
                                    recoveryTurns = 1;
                                    player.Inventory.RemoveItem(item);
                                    Console.WriteLine($"{player.Name} drinks {item.Name}!");
                                }

                            };
                            Console.WriteLine($"{player.Name} begins to drink {item.Name}...");
                        }
                        else if (item != null && item is not ConsumableItem)
                        {
                            Console.WriteLine("you can't use that item in battle!");
                        }
                        else
                        {
                            Console.WriteLine("you don't have that item in your inventory!");
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
                        Console.WriteLine("❓ Unknown command. Use 'attack' , 'cast <skill>' or 'use <item name>'");
                        continue;
                    }

                    // Monster turn
                    if (monster.IsAlive)
                        BasicAttack(monster, player);
                }

            }

            if (player.IsAlive)
            {
                if (player.CurrentRoom.IsDungeonRoom && player.CurrentRoom.CurrentMonsters.Count > 0)
                {
                    player.CurrentRoom.CurrentMonsters.Remove(monster);
                }
                Console.WriteLine($"\n✅ You defeated the {monster.Name}!"); 
                player.Experience += monster.Exp;
                player.CheckForLevelup();
                SkillFactory.UpdateSkills(ref player); 

                foreach (var quest in player.ActiveQuests.Where(q => q.Status == QuestStatus.InProgress))
                {
                    if (quest.RequiredKills.TryGetValue(monster.Id, out int required))
                    {
                        quest.KillProgress[monster.Id]++;
                        int current = quest.KillProgress[monster.Id];
                        Console.WriteLine($"📜 Quest '{quest.Name}': {monster.Name} slain ({current}/{required})");

                        if (quest.KillProgress.All(kp => kp.Value >= quest.RequiredKills[kp.Key]))
                        {
                            quest.Status = QuestStatus.Completed;
                            Console.WriteLine($"✅ Quest '{quest.Name}' is now complete! Return to the quest board.");
                        }

                    }

                }

                var drops = LootGenerator.GetLootFor(monster);

                if (drops.Count > 0)
                {
                    if (monster.DropsCorpse)
                    {
                        var corpse = new Corpse(monster.Name, drops);
                        player.CurrentRoom.Corpses.Add(corpse);
                        Console.WriteLine($"The corpse of {monster.Name} remains. You can loot it.");
                    }
                    else
                    {
                        foreach (var drop in drops)
                        {
                            if (player.Inventory.AddItem(drop, player))
                            {
                                Console.Write($"🪶 You found: ");
                                Printer.PrintColoredItemName(drop);
                                Console.WriteLine();
                            }
                            else
                            {
                                Console.Write($"❌ Inventory full. Could not take: ");
                                Printer.PrintColoredItemName(drop);
                                Console.WriteLine();
                            }
                        }

                    }

                }
                if (player.CurrentRoom.IsDungeonRoom && player.CurrentRoom.CurrentMonsters.Count < 1)
{
                    player.CurrentRoom.IsCleared = true;
                    Console.WriteLine($"✅ The room has been cleared. The path forward is open.");
                }

            }
            else
{
                Console.WriteLine("💀 You were defeated...");

                int respawnRoomId = player.LastHealerRoomId ?? 1;
                Room respawnRoom = GameService.rooms[respawnRoomId];

                player.CurrentRoom = respawnRoom;
                player.CurrentRoomId = respawnRoomId;
                player.CurrentHealth = player.Stats.MaxHealth;
                player.CurrentMana = player.Stats.MaxMana;

                Console.WriteLine($"🌀 You awaken in {respawnRoom.Name}."); 
                player.ApplyDeathXpPenalty();
            }
        }
        public static bool TryHit(ICombatant attacker, ICombatant defender)
        {
            float aim = attacker.TotalAim;
            float evasion = defender.TotalEvasion;

            if (aim >= evasion)
            {
                return true; // Guaranteed hit
            }
            float hitChance = aim / evasion;
            float roll = (float)_random.NextDouble();

            return roll <= hitChance; // True = hit, False = miss
        }
        private static void BasicAttack(ICombatant attacker, ICombatant defender)
        {
            Console.WriteLine($"{attacker.Name} attacks!");

            if (!TryHit(attacker, defender))
            {
                Console.WriteLine($"{attacker.Name} missed!");
                return;
            }

            int damage = CalculateDamage(attacker, defender); // or add magic check
            defender.TakeDamage(damage);
        }
        private static void UseSkill(Player player, Monster target, Skill skill)
        {
            Console.WriteLine($"{player.Name} casts {skill.Name}!");

            int baseStat = skill.StatToScaleFrom.ToUpper() switch
            {
                "ATK" => player.TotalPhysicalAttack,
                "MATK" => player.TotalMagicAttack,
                "SPR" => player.TotalSPR,
                "INT" => player.TotalINT,
                _ => player.TotalPhysicalAttack
            };


            int value = (int)(baseStat * skill.ScalingFactor);
            player.CurrentMana -= skill.ManaCost;

            if (skill.IsHealing)
            {
                int healed = Math.Min(value, player.Stats.MaxHealth - player.CurrentHealth);
                player.CurrentHealth += healed;
                Console.WriteLine($"{player.Name} heals for {healed} HP!");
            }
            else
            {
                Console.WriteLine($"{target.Name} takes {value} damage from {skill.Name}!");
                target.TakeDamage(value);
            }

        }
        public static int CalculateDamage(ICombatant attacker, ICombatant defender)
        {
            float atk = attacker.TotalPhysicalAttack;
            float matk = attacker.TotalMagicAttack;
            float def = defender.TotalPhysicalDefense;
            float mdef = defender.TotalMagicDefense;

            Console.WriteLine($"\n{attacker.Name} attacks {defender.Name}!");

            if (!TryHit(attacker, defender))
            {
                Console.WriteLine($"{attacker.Name} missed!");
                return 0;
            }

            float pdmg = atk * (atk / (atk + def));
            float mdmg = matk * (matk / (matk + mdef));
            float dmg = Math.Max(pdmg, mdmg);
            if (dmg < 1)
                dmg = 1;
            // Check for block
            float blockRoll = (float)_random.NextDouble();
            if (blockRoll < defender.GetBlockChance())
            {
                Console.WriteLine($"{defender.Name} blocked the attack!");
                dmg /= 2;
            }

            return (int)dmg;
        }

    }

}