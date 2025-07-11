using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleWorldRPG.Entities
{
    public class Player
    {
        public string Name { get; set; }
        public int MaxHealth { get; private set; } = 100;
        public int CurrentHealth { get; set; }
        public Room CurrentRoom { get; set; }

        public Player(string name)
        {
            Name = name;
            CurrentHealth = MaxHealth;
        }

        public void ShowStatus()
        {
            Console.WriteLine($"{Name}'s Health: {CurrentHealth}/{MaxHealth}");
        }

    }

}
