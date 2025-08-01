using ConsoleWorldRPG.Entities;
using System.Text.Json;

namespace ConsoleWorldRPG.Services
{
    public static class DungeonRegistry
    {
        private static Dictionary<string, Dungeon> _dungeons = new();

        public static void Load(string path = "Data/dungeons.json")
        {
            var json = File.ReadAllText(path);
            var list = JsonSerializer.Deserialize<List<Dungeon>>(json);
            _dungeons = list.ToDictionary(d => d.Id);
        }

        public static Dungeon? GetDungeonByRoom(Room room)
        {
            return _dungeons.Values.FirstOrDefault(d => d.ContainsRoom(room));
        }

        public static Dungeon? GetDungeonById(string id) =>
            _dungeons.TryGetValue(id, out var d) ? d : null;
    }

}