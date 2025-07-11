using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Interfaces;
using System;

namespace ConsoleWorldRPG.Systems
{
    public static class CombatSystem
    {
        private static readonly Random _random = new();

        public static bool TryHit(ICombatant attacker, ICombatant defender)
        {
            float aim = attacker.Stats.HitChance;
            float evasion = defender.Stats.DodgeChance;

            if (aim >= evasion)
            {
                return true; // Guaranteed hit
            }

            float missChance = evasion - aim;
            float roll = (float)_random.NextDouble() * evasion;

            return roll >= missChance; // True = hit, False = miss
        }

        public static void Attack(ICombatant attacker, ICombatant defender)
        {
            Console.WriteLine($"\n{attacker.Name} attacks {defender.Name}!");

            if (!TryHit(attacker, defender))
            {
                Console.WriteLine($"{attacker.Name} missed!");
                return;
            }

            int damage = attacker.DealPhysicalDamage();

            // Check for block
            float blockRoll = (float)_random.NextDouble();
            if (blockRoll < defender.GetBlockChance())
            {
                Console.WriteLine($"{defender.Name} blocked the attack!");
                damage /= 2;
            }

            defender.TakeDamage(damage);
        }

    }

}