public abstract class CombatEntity : ICombatant
{
    public string Name { get; set; }
    public Stats Stats { get; set; } = new();
    public int CurrentHealth { get; set; }

    public int MaxHealth => Stats.MaxHealth;
    public bool IsAlive => CurrentHealth > 0;

    public virtual void TakeDamage(int amount)
    {
        CurrentHealth = Math.Max(0, CurrentHealth - amount);
        Console.WriteLine($"{Name} takes {amount} damage. HP: {CurrentHealth}/{MaxHealth}");
    }

    public virtual int DealPhysicalDamage()
    {
        return Stats.PhysicalAttack;
    }

    public virtual float GetBlockChance()
    {
        return Stats.BlockChance;
    }
}