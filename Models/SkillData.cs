using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleWorldRPG.Models
{
    public class SkillData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Class { get; set; }
        public int ManaCost { get; set; }
        public string Type { get; set; } // "Physical" or "Magical"
        public string Target { get; set; } // "SingleEnemy", "AllEnemies", "Self"
        public float ScalingFactor { get; set; }
        public string StatToScaleFrom { get; set; }
        public int MinLevel { get; set; }
    }

}
