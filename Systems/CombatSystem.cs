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

        public static void Attack(ICombatant attacker, ICombatant defender)
        {
            float atk = attacker.TotalPhysicalAttack;
            float matk = attacker.TotalMagicAttack;
            float def = defender.TotalPhysicalDefense;
            float mdef = defender.TotalMagicDefense;

            Console.WriteLine($"\n{attacker.Name} attacks {defender.Name}!");

            if (!TryHit(attacker, defender))
            {
                Console.WriteLine($"{attacker.Name} missed!");
                return;
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

            int damage = ((int)dmg);
            defender.TakeDamage(damage);
        }

    }

}