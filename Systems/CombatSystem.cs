using System;
using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Interfaces;
using ConsoleWorldRPG.Items;
using ConsoleWorldRPG.Skills;
using ConsoleWorldRPG.Utils;

namespace ConsoleWorldRPG.Systems
{
    public static class CombatSystem
    {
        private static readonly Random _random = new();

        /// <summary>
        /// Starts an Encounter after the look command if the room has monsters, also handels the End of a fight
        /// </summary>
        public static void StartEncounter(Player player)
        {
            Random random = new Random();
            var encounters = player.CurrentRoom.EncounterableMonsters;

            if (encounters == null || encounters.Count == 0)
            {
                Console.WriteLine("But nothing stirs in the darkness...");
                return;
            }

            Monster monster = player.CurrentRoom.Monsters[random.Next(0, player.CurrentRoom.Monsters.Count)];
            monster.ResetHealth();

            Console.WriteLine($"\nA wild {monster.Name} appears!");
            Console.WriteLine(monster.Description);


            while (player.IsAlive && monster.IsAlive)
            {
                Console.WriteLine($"\nWhat will you do? (attack / cast <skill>)");
                Console.Write("> ");
                var input = Console.ReadLine()?.Trim().ToLower();

                if (input == "attack")
                {
                    BasicAttack(player, monster);
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

                    UseSkill(player, monster, skill);
                }
                else
                {
                    Console.WriteLine("❓ Unknown command. Use 'attack' or 'cast <skill>'");
                    continue;
                }

                // Monster turn
                if (monster.IsAlive)
                    BasicAttack(monster, player);
            }

            if (player.IsAlive)
            {
                Console.WriteLine($"\n✅ You defeated the {monster.Name}!"); player.Experience += monster.Exp;
                player.CheckForLevelup();

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
                            if (player.Inventory.AddItem(drop))
                                Console.WriteLine($"🪶 You found: {drop.Name}");
                            else
                                Console.WriteLine($"❌ Inventory full. Could not take: {drop.Name}");
                        }

                    }

                }

            }
            else
                Console.WriteLine("\n💀 You were defeated...");
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
                "STR" => player.TotalSTR,
                "INT" => player.TotalINT,
                _ => player.TotalPhysicalAttack
            };

            int damage = (int)(baseStat * skill.ScalingFactor);

            player.CurrentMana -= skill.ManaCost;

            Console.WriteLine($"{target.Name} takes {damage} damage from {skill.Name}!");
            target.TakeDamage(damage);
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