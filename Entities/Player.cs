using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Reflection.Emit;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Enums;

namespace ConsoleWorldRPG.Entities
{
    public class Player : CombatEntity
    {
        public PlayerClass Class { get; set; } = PlayerClass.Fighter;
        public int Level { get; set; } = 1;
        public long Experience { get; set; } = 0;
        public long ExpForNextLvl { get; private set; }
        public Room CurrentRoom { get; set; }

    // Add inventory, experience, commands, etc.
    public Player(string name, Stats stats)
    {
        Name = name;
        Stats = stats;
        CurrentHealth = stats.MaxHealth;
        CurrentMana = stats.MaxMana;
        ExpForNextLvl = (long)(Math.Pow(Level, 2)) * 50;
    }

        public void ShowStatus()
        {
            Console.WriteLine($"{Name}'s Health: {CurrentHealth}/{MaxHealth}");
        }
        public void CheckForLevelup()
        {
            while (ExpForNextLvl < Experience)
            {
                LevelUp();
                Experience -= ExpForNextLvl;
                ExpForNextLvl = (long)(Math.Pow(Level, 2)) * 50;
            }

        }
        public void LevelUp()
        {
            var profile = ClassProfile.All[Class];

            Level++;
            Stats.Strength += profile.StatGrowth["STR"];
            Stats.Dexterity += profile.StatGrowth["DEX"];
            Stats.Endurance += profile.StatGrowth["END"];
            Stats.Intelligence += profile.StatGrowth["INT"];
            Stats.Spirit += profile.StatGrowth["SPR"];

            Stats.BaseHealth += profile.HpPerLevel;
            Stats.BaseMana += profile.ManaPerLevel;

            CurrentHealth = Stats.MaxHealth;
            CurrentMana = Stats.MaxMana;

            Console.WriteLine($"\n🎉 You reached level {Level}!");
            Console.WriteLine($"HP: {Stats.MaxHealth}, Mana: {Stats.MaxMana}");
        }

    }

}
