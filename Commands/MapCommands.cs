using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Services;

namespace ConsoleWorldRPG.Commands
{
    public static class MapCommands
    {
        /// <summary>
        /// Handels map commands
        /// </summary>
        /// <param name="input">player input</param>
        /// <param name="player">player character</param>
        /// <returns>returns if command found</returns>
        public static bool Handle(string input, Player player)
        {
            if (input != "map") return false;

            Dungeon? dungeon = DungeonRegistry.GetDungeonByRoom(player.CurrentRoom);
            Cave? cave = CaveRegistry.GetCaveByRoom(player.CurrentRoom);

            if (cave == null && dungeon == null)
            {
                string mapFile = "Data/Maps/world_map.json";

                if (!File.Exists(mapFile))
                {
                    Console.WriteLine("🗺 No map found for this area.");
                }
                string map = File.ReadAllText(mapFile);

                PrintMap("World", map, player);
            }
            else if (cave != null)
            {
                CaveMap(cave, player);
            }
            else if (dungeon != null)
            {
                DungonMap(dungeon, player);
            }
            return true;
        }
        private static void CaveMap(Cave cave, Player player)
        {
            string mapFile = cave?.MapFile;

            if (!File.Exists(mapFile))
            {
                Console.WriteLine("🗺 No map found for this area.");
            }
            string map = File.ReadAllText(mapFile);


            PrintMap(cave.Name, map, player);
        }
        private static void DungonMap(Dungeon dungeon, Player player)
        {
            string mapFile = dungeon?.MapFile;

            if (!File.Exists(mapFile))
            {
                Console.WriteLine("🗺 No map found for this area.");
            }
            string map = File.ReadAllText(mapFile);


            PrintMap(dungeon.Name, map, player);
        }
        private static void PrintMap(string rommType, string map, Player player)
        {
            string roomName = player.CurrentRoom.Name;

            if (!string.IsNullOrWhiteSpace(roomName) && map.Contains(roomName))
            {
                map = map.Replace(roomName, roomName + " ⭐", StringComparison.Ordinal);
            }

            Console.WriteLine($"\n📍 Map: {rommType}\n");
            Console.WriteLine(map);
        }

    }

}