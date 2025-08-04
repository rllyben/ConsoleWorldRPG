using ConsoleWorldRPG.Interfaces;
using ConsoleWorldRPG.Items;

namespace ConsoleWorldRPG.Entities
{
    public abstract class CombatEntity : ICombatant
    {
        public string Name { get; set; }
        public Stats Stats { get; set; } = new();
        public int CurrentHealth { get; set; }
        public int CurrentMana { get; set; }

        public int MaxHealth => Stats.MaxHealth;
        public int MaxMana => Stats.MaxMana;
        public bool IsAlive => CurrentHealth > 0;
        public EquipmentItem? WeaponSlot { get; set; }
        public EquipmentItem? ArmorSlot { get; set; }
        public EquipmentItem? AccessorySlot { get; set; }

        // Optional: effective STR, DEX, etc.
        public int TotalSTR => Stats.Strength + GetBonusFromGear(g => g.BonusSTR);
        public int TotalDEX => Stats.Dexterity + GetBonusFromGear(g => g.BonusDEX);
        public int TotalEND => Stats.Endurance + GetBonusFromGear(g => g.BonusEND);
        public int TotalINT => Stats.Intelligence + GetBonusFromGear(g => g.BonusINT);
        public int TotalSPR => Stats.Spirit + GetBonusFromGear(g => g.BonusSPR);

        public int TotalPhysicalAttack =>
            (TotalSTR * 2 + TotalEND) + GetBonusFromGear(g => g.BonusATK);

        public int TotalPhysicalDefense =>
            (TotalEND * 2 + TotalSTR) + GetBonusFromGear(g => g.BonusDEF);

        public int TotalMagicAttack =>
            (TotalINT * 2 + TotalSPR) + GetBonusFromGear(g => g.BonusMATK);

        public int TotalMagicDefense =>
            (TotalSPR * 2 + TotalINT) + GetBonusFromGear(g => g.BonusMDEF);

        public int TotalAim =>
            (TotalDEX) + GetBonusFromGear(g => g.BonusAim);

        public int TotalEvasion =>
            (int)(TotalDEX * 0.85f) + GetBonusFromGear(g => g.BonusEvasion);
        public float CritChance => GetBonusFromGear(g => g.BonusDEX) * 0.1f + GetBonusFromGear(g => g.BonusCrit);
        public float BlockChance
        {
            get
            {
                float blockRaw = (GetBonusFromGear(g => g.BonusEND) * 0.3f) + (GetBonusFromGear(g => g.BonusINT) * 0.2f) + (GetBonusFromGear(g => g.BonusSTR) * 0.1f) + GetBonusFromGear(g => g.BonusBlock);
                return MathF.Min(blockRaw * 0.01f, 0.75f);
            }
        }

        public virtual void TakeDamage(int amount)
        {
            CurrentHealth = Math.Max(0, CurrentHealth - amount);
            Console.WriteLine($"{Name} takes {amount} damage. HP: {CurrentHealth}/{MaxHealth}");
        }

        public virtual int DealPhysicalDamage()
        {
            return Stats.PhysicalAttack;
        }
        public virtual int DefandPhysical()
        {
            return Stats.PhysicalDefense;
        }
        public virtual float GetBlockChance()
        {
            return BlockChance;
        }
        /// <summary>
        /// returns the bonus from equiped gear
        /// </summary>
        /// <param name="selector">function to select from</param>
        /// <returns>the bonmus as an int</returns>
        public int GetBonusFromGear(Func<EquipmentItem, int> selector)
        {
            int total = 0;
            if (WeaponSlot != null) total += selector(WeaponSlot);
            if (ArmorSlot != null) total += selector(ArmorSlot);
            if (AccessorySlot != null) total += selector(AccessorySlot);
            return total;
        }
        /// <summary>
        /// returns the bonus from equiped gear
        /// </summary>
        /// <param name="selector">function to select from</param>
        /// <returns>the bonmus as an float</returns>
        public float GetBonusFromGear(Func<EquipmentItem, float> selector)
        {
            float total = 0;
            if (WeaponSlot != null) total += selector(WeaponSlot);
            if (ArmorSlot != null) total += selector(ArmorSlot);
            if (AccessorySlot != null) total += selector(AccessorySlot);
            return total;
        }

    }

}