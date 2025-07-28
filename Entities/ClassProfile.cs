using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Enums;

namespace ConsoleWorldRPG.Entities
{
    public class ClassProfile
    {
        public PlayerClass Class { get; set; }
        public Dictionary<string, int> StatGrowth { get; set; } = new();
        public int HpPerLevel { get; set; }
        public int ManaPerLevel { get; set; }

        public static Dictionary<PlayerClass, ClassProfile> All => new()
        {
            [PlayerClass.Archer] = new ClassProfile
            {
                Class = PlayerClass.Archer,
                StatGrowth = new()
                {
                    ["STR"] = 3,
                    ["DEX"] = 4,
                    ["END"] = 2,
                    ["INT"] = 1,
                    ["SPR"] = 2
                },
                HpPerLevel = 8,
                ManaPerLevel = 6
            },
            [PlayerClass.Hunter] = new ClassProfile
            {
                Class = PlayerClass.Hunter,
                StatGrowth = new()
                {
                    ["STR"] = 3,
                    ["DEX"] = 5,
                    ["END"] = 1,
                    ["INT"] = 1,
                    ["SPR"] = 2
                },
                HpPerLevel = 7,
                ManaPerLevel = 7
            },
            [PlayerClass.Knight] = new ClassProfile
            {
                Class = PlayerClass.Knight,
                StatGrowth = new()
                {
                    ["STR"] = 2,
                    ["DEX"] = 1,
                    ["END"] = 5,
                    ["INT"] = 1,
                    ["SPR"] = 3
                },
                HpPerLevel = 14,
                ManaPerLevel = 4
            },
            [PlayerClass.Fighter] = new ClassProfile
            {
                Class = PlayerClass.Fighter,
                StatGrowth = new()
                {
                    ["STR"] = 3,
                    ["DEX"] = 3,
                    ["END"] = 3,
                    ["INT"] = 1,
                    ["SPR"] = 3
                },
                HpPerLevel = 10,
                ManaPerLevel = 6
            },
            [PlayerClass.Barbarian] = new ClassProfile
            {
                Class = PlayerClass.Barbarian,
                StatGrowth = new()
                {
                    ["STR"] = 5,
                    ["DEX"] = 2,
                    ["END"] = 2,
                    ["INT"] = 0,
                    ["SPR"] = 1
                },
                HpPerLevel = 12,
                ManaPerLevel = 2
            },
            [PlayerClass.Cleric] = new ClassProfile
            {
                Class = PlayerClass.Cleric,
                StatGrowth = new()
                {
                    ["STR"] = 1,
                    ["DEX"] = 2,
                    ["END"] = 3,
                    ["INT"] = 1,
                    ["SPR"] = 4
                },
                HpPerLevel = 12,
                ManaPerLevel = 8
            },
            [PlayerClass.Rogue] = new ClassProfile
            {
                Class = PlayerClass.Rogue,
                StatGrowth = new()
                {
                    ["STR"] = 4,
                    ["DEX"] = 5,
                    ["END"] = 1,
                    ["INT"] = 1,
                    ["SPR"] = 1
                },
                HpPerLevel = 8,
                ManaPerLevel = 4
            },
            [PlayerClass.ElementalMage] = new ClassProfile
            {
                Class = PlayerClass.ElementalMage,
                StatGrowth = new()
                {
                    ["STR"] = 1,
                    ["DEX"] = 3,
                    ["END"] = 1,
                    ["INT"] = 4,
                    ["SPR"] = 4
                },
                HpPerLevel = 8,
                ManaPerLevel = 10
            },
            [PlayerClass.ArcanMage] = new ClassProfile
            {
                Class = PlayerClass.ArcanMage,
                StatGrowth = new()
                {
                    ["STR"] = 0,
                    ["DEX"] = 3,
                    ["END"] = 1,
                    ["INT"] = 6,
                    ["SPR"] = 3
                },
                HpPerLevel = 6,
                ManaPerLevel = 12
            },
            [PlayerClass.Druid] = new ClassProfile
            {
                Class = PlayerClass.Druid,
                StatGrowth = new()
                {
                    ["STR"] = 1,
                    ["DEX"] = 2,
                    ["END"] = 1,
                    ["INT"] = 4,
                    ["SPR"] = 5
                },
                HpPerLevel = 8,
                ManaPerLevel = 10
            },
            [PlayerClass.SoulsKnight] = new ClassProfile
            {
                Class = PlayerClass.SoulsKnight,
                StatGrowth = new()
                {
                    ["STR"] = 3,
                    ["DEX"] = 1,
                    ["END"] = 4,
                    ["INT"] = 3,
                    ["SPR"] = 4
                },
                HpPerLevel = 12,
                ManaPerLevel = 8
            }

        };

    }

}
