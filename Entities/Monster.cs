using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleWorldRPG.Entities
{
    public class Monster : CombatEntity
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public long Exp { get; set; }

    public Monster(int id, string name, Stats stats, string description, long exp)
        {
            Id = id;
            Name = name;
            Stats = stats;
            Description = description;
            CurrentHealth = stats.MaxHealth;
            Exp = exp;
        }

        public void ResetHealth()
        {
            CurrentHealth = MaxHealth;
        }

    }

}
