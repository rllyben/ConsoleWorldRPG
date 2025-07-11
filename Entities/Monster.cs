using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleWorldRPG.Entities
{
    public class Monster : CombatEntity
    {
        public string Id { get; set; }  // unique ID or name
        public int Attack { get; set; }
        public string Description { get; set; }

    public Monster(string id, string name, Stats stats, string description)
    {
        Id = id;
        Name = name;
        Stats = stats;
        Description = description;
        CurrentHealth = stats.MaxHealth;
    }

        public void ResetHealth()
        {
            CurrentHealth = MaxHealth;
        }

    }

}
