using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Enums;
using ConsoleWorldRPG.Items;
using ConsoleWorldRPG.Services;

namespace ConsoleWorldRPG.Systems
{
    public static class LootGenerator
    {
        private static Random _rand = new();

        public static List<Item> GetLootFor(Monster monster)
        {
            var loot = new List<Item>();

            if (monster.LootTable.Any())
            {
                loot.AddRange(monster.LootTable); // optional override
                return loot;
            }

            switch (monster.Type)
            {
                case MonsterType.Spirit:
                    if (_rand.NextDouble() < 0.7) loot.Add(ItemFactory.CreateItem("spirit_dust"));
                    break;
                case MonsterType.Shadow:
                    if (_rand.NextDouble() < 0.7) loot.Add(ItemFactory.CreateItem("spirit_dust"));
                    break;
                case MonsterType.Elemental:
                    if (_rand.NextDouble() < 0.5) loot.Add(ItemFactory.CreateItem("fire_ash")); // use actual elemental info later
                    break;
                case MonsterType.Beast:
                    if (_rand.NextDouble() < 0.5) loot.Add(ItemFactory.CreateItem("leather"));
                    if (_rand.NextDouble() < 0.3) loot.Add(ItemFactory.CreateItem("fang"));
                    break;
                case MonsterType.Humanoid:
                    if (_rand.NextDouble() < 0.5) loot.Add(ItemFactory.CreateItem("leather"));
                    if (_rand.NextDouble() < 0.3) loot.Add(ItemFactory.CreateItem("fang"));
                    break;
                    // more types here...
            }

            return loot;
        }

    }

}
