using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Enums;
using ConsoleWorldRPG.Interfaces;

namespace ConsoleWorldRPG.Skills
{
    public enum SkillType { Physical, Magical }
    public enum SkillTarget { SingleEnemy, AllEnemies, Self }

    public class Skill
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CastTime { get; set; } = 0;     // turns before it activates
        public int RecoveryTime { get; set; } = 0; // turns before you can act again
        public PlayerClass Class { get; set; }
        public bool IsHealing { get; set; } = false;
        public int ManaCost { get; set; }
        public SkillType Type { get; set; }
        public SkillTarget Target { get; set; }

        public float ScalingFactor { get; set; } // e.g., 1.5 for 150% of base stat
        public string StatToScaleFrom { get; set; } = "ATK"; // or "MATK", "STR", etc.

        public int MinLevel { get; set; } = 1;

        public Action<Player, ICombatant> Effect { get; set; } // optional logic
    }
}
