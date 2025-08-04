using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities.Skills;
using ConsoleWorldRPG.Skills;

namespace ConsoleWorldRPG.Systems
{
    public class SkillFusionSystem
    {
        public static Skill FuseSkills(List<BaseSkill> components)
        {
            var id = "custom_" + string.Join("_", components.Select(c => c.Id));
            var name = "Fusion: " + string.Join(" + ", components.Select(c => c.Name));
            var desc = string.Join(" + ", components.Select(c => c.Description));

            float totalScaling = components.Sum(s => s.ScalingFactor);
            int totalMana = components.Sum(s => s.ManaCost) / components.Count;

            string stat = components.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s.StatToScaleFrom))?.StatToScaleFrom ?? "ATK";

            return new Skill
            {
                Id = id,
                Name = name,
                Description = desc,
                ManaCost = totalMana,
                ScalingFactor = totalScaling,
                StatToScaleFrom = stat,
                Target = SkillTarget.SingleEnemy,
                Type = SkillType.Magical,
                MinLevel = components.Min(s => s.RequiredLevel)
            };

        }

    }

}
