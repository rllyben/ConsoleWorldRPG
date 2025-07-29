namespace ConsoleWorldRPG.Entities
{
    public class Stats
    {
        public int Strength { get; set; } = 10;
        public int Dexterity { get; set; } = 10;
        public int Endurance { get; set; } = 10;
        public int Intelligence { get; set; } = 10;
        public int Spirit { get; set; } = 10;

        // Flat HP and Mana stats can be modified by class/gear/level
        public int BaseHealth { get; set; } = 30;
        public int BaseMana { get; set; } = 30;

        public int MaxHealth => BaseHealth + Endurance * 5; // Reduced effect from END
        public int MaxMana => BaseMana + Spirit * 5; // Spirit no longer affects this

        public int PhysicalAttack => Strength * 2 + Endurance;
        public int PhysicalDefense => Endurance * 2 + Strength;

        public int MagicAttack => Intelligence * 2 + Spirit;
        public int MagicDefense => Spirit * 2 + Intelligence;
        public int HitChance => Dexterity;
        public int DodgeChance => (int)(Dexterity * 0.85f);
        public float GearBlockBonus { get; set; } = 0f; // To be set via gear

        public Stats Clone()
        {
            return (Stats)this.MemberwiseClone();
        }
    }

}