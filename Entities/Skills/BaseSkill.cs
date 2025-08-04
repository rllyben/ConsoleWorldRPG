using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Enums;

namespace ConsoleWorldRPG.Entities.Skills
{
    public class BaseSkill
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<SkillComponentType> ComponentType { get; set; } = new();  // e.g. Fire, AOE, Heal, etc.
        public PlayerClass Class { get; set; }
        public int ManaCost { get; set; }
        public float ScalingFactor { get; set; }
        public string StatToScaleFrom { get; set; }
        public int RequiredLevel { get; set; } = 1;
    }

}
