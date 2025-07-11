using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleWorldRPG.Entities
{
    public class Player : CombatEntity
    {
        public Room CurrentRoom { get; set; }

    // Add inventory, experience, commands, etc.
    public Player(string name, Stats stats)
    {
        Name = name;
        Stats = stats;
        CurrentHealth = stats.MaxHealth;
    }

        public void ShowStatus()
        {
            Console.WriteLine($"{Name}'s Health: {CurrentHealth}/{MaxHealth}");
        }

    }

}
