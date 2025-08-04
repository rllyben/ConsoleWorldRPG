using ConsoleWorldRPG.Entities;
using System.Text.Json;

namespace ConsoleWorldRPG.Services
{
    public static class DungeonRegistry
    {
        private static Dictionary<string, Dungeon> _dungeons = new();
        /// <summary>
        /// loads all dungons
        /// </summary>
        /// <param name="path">path to an dungon file, default is Data/dungeons.json</param>
        public static void Load(string path = "Data/dungeons.json")
        {
            var json = File.ReadAllText(path);
            var list = JsonSerializer.Deserialize<List<Dungeon>>(json);
            _dungeons = list.ToDictionary(d => d.Id);
        }
        /// <summary>
        /// retruns the dungon a room belongs to
        /// </summary>
        /// <param name="room">a room of the dungeon</param>
        /// <returns>the found dungon or null</returns>
        public static Dungeon? GetDungeonByRoom(Room room)
        {
            return _dungeons.Values.FirstOrDefault(d => d.ContainsRoom(room));
        }
        /// <summary>
        /// Gets the dungeon by its id
        /// </summary>
        /// <param name="id">the dungon id of the wanted dungeon</param>
        /// <returns>the found dungeon or null</returns>
        public static Dungeon? GetDungeonById(string id) =>
            _dungeons.TryGetValue(id, out var d) ? d : null;
    }

}