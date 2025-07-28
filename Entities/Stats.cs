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

        public float CritChance => Dexterity * 0.01f;
        public int HitChance => Dexterity;
        public int DodgeChance => Dexterity;

        public float GearBlockBonus { get; set; } = 0f; // To be set via gear
        public float BlockChance
        {
            get
            {
                float blockRaw = (Endurance * 0.3f) + (Intelligence * 0.2f) + (Strength * 0.1f);
                return MathF.Min(blockRaw * 0.01f + GearBlockBonus, 0.75f);
            }
        }

        public Stats Clone()
        {
            return (Stats)this.MemberwiseClone();
        }
    }

}