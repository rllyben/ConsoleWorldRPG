using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Items;

namespace ConsoleWorldRPG.Interfaces
{
    public interface ICombatant
    {
        public string Name { get; }
        public Stats Stats { get; }

        public int CurrentHealth { get; }
        public int CurrentMana { get; }
        public int MaxHealth { get; }
        public int MaxMana { get; }

        public int TotalPhysicalAttack { get; }
        public int TotalPhysicalDefense { get; }
        public int TotalMagicAttack { get; }
        public int TotalMagicDefense { get; }
        public int TotalAim { get; }
        public int TotalEvasion { get; }
        public int TotalSTR { get; }
        public int TotalDEX { get; }
        public int TotalEND { get; }
        public int TotalINT { get; }
        public int TotalSPR { get; }
        public float CritChance { get; }
        public float BlockChance { get; }

        public bool IsAlive => CurrentHealth > 0;

        void TakeDamage(int amount);
        int DealPhysicalDamage();
        int DefandPhysical();
        float GetBlockChance();
        int GetBonusFromGear(Func<EquipmentItem, int> selector);
    }

}