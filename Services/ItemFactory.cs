using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Items;
using ConsoleWorldRPG.Models;

namespace ConsoleWorldRPG.Services
{
    public static class ItemFactory
    {
        private static Dictionary<string, GameItem> _itemDefs;

        public static void LoadItems(string path = "items.json")
        {
            string json = File.ReadAllText(path);
            var list = JsonSerializer.Deserialize<List<GameItem>>(json);
            _itemDefs = list.ToDictionary(i => i.Id, i => i);
        }

        public static Item CreateItem(string id)
        {
            if (!_itemDefs.TryGetValue(id, out var def))
                throw new Exception($"Item with ID '{id}' not found.");

            return def.Type switch
            {
                "consumable" => new JsonConsumableItem(def),
                "equipment" => new JsonEquipmentItem(def),
                "raw_material" => new JsonMaterialItem(def),
                _ => throw new Exception($"Unsupported item type: {def.Type}")
            };
        }

        public static List<Item> GetAllItemsFor(Player player)
        {
            return _itemDefs.Values
                .Where(def => def.Type == "equipment"
                    && (def.AllowedClasses.Count == 0 || def.AllowedClasses.Contains(player.Class.ToString())))
                .Select(def => CreateItem(def.Id))
                .ToList();
        }

    }

}
