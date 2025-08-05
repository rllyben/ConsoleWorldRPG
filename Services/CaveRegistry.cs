using ConsoleWorldRPG.Entities;
using System.Text.Json;

namespace ConsoleWorldRPG.Services
{
    public static class CaveRegistry
    {
        private static Dictionary<string, Cave> _caves = new();
        /// <summary>
        /// loads all dungons
        /// </summary>
        /// <param name="path">path to an dungon file, default is Data/dungeons.json</param>
        public static void Load(string path = "Data/caves.json")
        {
            var json = File.ReadAllText(path);
            var list = JsonSerializer.Deserialize<List<Cave>>(json);
            _caves = list.ToDictionary(d => d.Id);
        }
        /// <summary>
        /// retruns the dungon a room belongs to
        /// </summary>
        /// <param name="room">a room of the dungeon</param>
        /// <returns>the found dungon or null</returns>
        public static Cave? GetCaveByRoom(Room room)
        {
            return _caves.Values.FirstOrDefault(d => d.ContainsRoom(room));
        }
        /// <summary>
        /// Gets the dungeon by its id
        /// </summary>
        /// <param name="id">the dungon id of the wanted dungeon</param>
        /// <returns>the found dungeon or null</returns>
        public static Cave? GetCaveById(string id) =>
            _caves.TryGetValue(id, out var d) ? d : null;
    }

}