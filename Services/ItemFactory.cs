using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Enums;
using ConsoleWorldRPG.Items;
using ConsoleWorldRPG.Models;

namespace ConsoleWorldRPG.Services
{
    public static class ItemFactory
    {
        private static Dictionary<string, GameItem> _itemDefs;

        public static void LoadItems(string path = "Data/items.json")
        {
            string json = File.ReadAllText(path);
            var list = JsonSerializer.Deserialize<List<GameItem>>(json);
            _itemDefs = list.ToDictionary(i => i.Id, i => i);
        }

        public static Item CreateItem(string id)
        {
            if (!_itemDefs.TryGetValue(id, out var def))
                throw new Exception($"Item with ID '{id}' not found.");

            Item item = def.Type switch
            {
                "consumable" => new JsonConsumableItem(def),
                "equipment" => new JsonEquipmentItem(def),
                "material" => new JsonMaterialItem(def),
                _ => throw new Exception($"Unsupported item type: {def.Type}")
            };

            // ✅ Set Rarity here
            if (Enum.TryParse<ItemRarity>(def.Rarity, true, out var parsed))
                item.Rarity = parsed;
            else
                item.Rarity = ItemRarity.Common; // fallback

                item.StackSize = def.StackSize > 0 ? def.StackSize : 1;

            return item;
        }
        public static bool TryCreateItem(string id, out Item item)
        {
            if (_itemDefs.TryGetValue(id, out var def))
            {
                item = CreateItem(id);
                return true;
            }
            item = null!;
            return false;
        }
        public static List<Item> GetAllItemsFor(Player player)
        {
            return _itemDefs.Values
                .Where(def => def.Type == "equipment"
                    && (def.AllowedClasses.Count == 0 || def.AllowedClasses.Contains(player.Class.ToString())))
                .Select(def => CreateItem(def.Id))
                .ToList();
        }
        public static List<Item> GetSmithItemsFor(Player player)
        {
            return _itemDefs.Values
                .Where(def => def.Type == "equipment" && (def.AllowedClasses.Contains(player.Class.ToString()) && def.Rarity == "Common"))
                .Select(def => CreateItem(def.Id))
                .ToList();
        }

    }

}
