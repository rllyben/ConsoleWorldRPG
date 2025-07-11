using ConsoleWorldRPG.Entities;

namespace ConsoleWorldRPG.Interfaces
{
    public interface ICombatant
    {
        string Name { get; }
        Stats Stats { get; }
        int CurrentHealth { get; set; }
        int MaxHealth { get; }
        bool IsAlive => CurrentHealth > 0;

        void TakeDamage(int amount);
        int DealPhysicalDamage();
        float GetBlockChance();
    }

}