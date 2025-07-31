using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Enums;
using ConsoleWorldRPG.Models;
using ConsoleWorldRPG.Skills;

namespace ConsoleWorldRPG.Services
{
    public static class SkillFactory
    {
        private static List<Skill> _skills = new();

        public static void LoadSkills(string path = "Data/skills.json")
        {
            var json = File.ReadAllText(path);
            var skillData = JsonSerializer.Deserialize<List<SkillData>>(json);

            _skills = skillData.Select(d => new Skill
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                Class = Enum.Parse<PlayerClass>(d.Class),
                ManaCost = d.ManaCost,
                Type = Enum.Parse<SkillType>(d.Type),
                Target = Enum.Parse<SkillTarget>(d.Target),
                ScalingFactor = d.ScalingFactor,
                StatToScaleFrom = d.StatToScaleFrom,
                MinLevel = d.MinLevel,
                IsHealing = d.IsHealing
            }).ToList();

        }

        public static List<Skill> GetSkillsFor(Player player)
        {
            return _skills
                .Where(s => s.Class == player.Class && s.MinLevel <= player.Level)
                .ToList();
        }
        public static void UpdateSkills(ref Player player, bool loading = false)
        {
            var unlocked = SkillFactory.GetSkillsFor(player);
            foreach (var skill in unlocked)
            {
                if (!player.Skills.Any(s => s.Id == skill.Id))
                {
                    player.Skills.Add(skill);

                    if (!loading)
                        Console.WriteLine($"✨ New skill learned: {skill.Name}!");
                }

            }

        }

    }

}
