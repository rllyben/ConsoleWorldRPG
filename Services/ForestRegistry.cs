using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Entities.RoomTypes;

namespace ConsoleWorldRPG.Services
{
    public static class ForestRegistry
    {
        private static Dictionary<string, Forest> _caves = new();
        /// <summary>
        /// loads all dungons
        /// </summary>
        /// <param name="path">path to an dungon file, default is Data/dungeons.json</param>
        public static void Load(string path = "Data/forests.json")
        {
            var json = File.ReadAllText(path);
            var list = JsonSerializer.Deserialize<List<Forest>>(json);
            _caves = list.ToDictionary(d => d.Id);
        }
        /// <summary>
        /// retruns the dungon a room belongs to
        /// </summary>
        /// <param name="room">a room of the dungeon</param>
        /// <returns>the found dungon or null</returns>
        public static Forest? GetForestByRoom(Room room)
        {
            return _caves.Values.FirstOrDefault(d => d.ContainsRoom(room));
        }
        /// <summary>
        /// Gets the dungeon by its id
        /// </summary>
        /// <param name="id">the dungon id of the wanted dungeon</param>
        /// <returns>the found dungeon or null</returns>
        public static Forest? GetForestById(string id) =>
            _caves.TryGetValue(id, out var d) ? d : null;
        public static Forest? GetForestByName(string name) =>
            _caves.Values.FirstOrDefault(f => f.Name.ToLower() == name);
    }

}
