using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
            else
            {
                loot.AddRange(GetTypeBasedLoot(monster.Type)); // your existing elemental/beast/spirit logic
            }

            foreach (var entry in monster.UniqueLootTable)
            {
                double rnd = _rand.NextDouble();
                if (rnd <= entry.DropChance)
                {
                    if (ItemFactory.TryCreateItem(entry.ItemId, out var item))
                        loot.Add(item);
                }
                Console.WriteLine(rnd);
            }

            return loot;
        }
        private static List<Item> GetTypeBasedLoot(MonsterType monsterType)
        {
            var loot = new List<Item>();
            switch (monsterType)
            {
                case MonsterType.Spirit:
                    if (_rand.NextDouble() < 0.7) loot.Add(ItemFactory.CreateItem("spirit_dust"));
                    break;
                case MonsterType.Shadow:
                    if (_rand.NextDouble() < 0.7) loot.Add(ItemFactory.CreateItem("shadow_remnant"));
                    break;
                case MonsterType.Elemental:
                    if (_rand.NextDouble() < 0.2) loot.Add(ItemFactory.CreateItem("earth_essence")); // use actual elemental info later
                    if (_rand.NextDouble() < 0.2) loot.Add(ItemFactory.CreateItem("stone_fragment"));
                    if (_rand.NextDouble() < 0.2) loot.Add(ItemFactory.CreateItem("fire_ash"));
                    if (_rand.NextDouble() < 0.2) loot.Add(ItemFactory.CreateItem("wind_whisper"));
                    if (_rand.NextDouble() < 0.2) loot.Add(ItemFactory.CreateItem("water_bead"));
                    break;
                case MonsterType.Beast:
                    if (_rand.NextDouble() < 0.5) loot.Add(ItemFactory.CreateItem("beast_flesh"));
                    if (_rand.NextDouble() < 0.3) loot.Add(ItemFactory.CreateItem("feral_leather"));
                    if (_rand.NextDouble() < 0.1) loot.Add(ItemFactory.CreateItem("beast_fang"));
                    break;
                case MonsterType.Humanoid:
                    if (_rand.NextDouble() < 0.7) loot.Add(ItemFactory.CreateItem("beast_flesh"));
                    break;
                    // more types here...
            }
            return loot;
        }

    }

}
