using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Enums;

namespace ConsoleWorldRPG.Models
{
    public class BaseSkillData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> ComponentType { get; set; } = new();  // e.g. Fire, AOE, Heal, etc.
        public string Class { get; set; }
        public int ManaCost { get; set; }
        public float ScalingFactor { get; set; }
        public string StatToScaleFrom { get; set; }
        public int RequiredLevel { get; set; } = 1;
    }
}
