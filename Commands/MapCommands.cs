using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Entities.RoomTypes;
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
            if (!input.StartsWith("map")) return false;

            Dungeon? dungeon = DungeonRegistry.GetDungeonByRoom(player.CurrentRoom);
            Cave? cave = CaveRegistry.GetCaveByRoom(player.CurrentRoom);
            City? city = CityRegistry.GetCityByRoom(player.CurrentRoom);
            Forest? forest = ForestRegistry.GetForestByRoom(player.CurrentRoom);

            if (input.StartsWith("map ") && input != "map world")
            {
                string mapName = input.Substring(4);
                Dungeon? searchDungeon = DungeonRegistry.GetDungeonByName(mapName);
                Cave? searchCave = CaveRegistry.GetCaveByName(mapName);
                City? searchCity = CityRegistry.GetCityByName(mapName);
                Forest? searchForest = ForestRegistry.GetForestByName(mapName);

                if (searchDungeon != null)
                    DungonMap(searchDungeon, player);
                else if (searchCave != null)
                    CaveMap(searchCave, player);
                else if (searchCity != null)
                    CityMap(searchCity, player);
                else if (searchForest != null)
                    ForestMap(searchForest, player);
                else
                    Console.WriteLine("🗺 No map found for this area.");
            }
            else if (cave == null && dungeon == null && forest == null && (city == null || city != null && !city.IsBig) || input == "map world")
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
                CaveMap(cave, player);
            else if (dungeon != null)
                DungonMap(dungeon, player);
            else if (city != null && city.IsBig)
                CityMap(city, player);
            else if (forest != null) 
                ForestMap(forest, player);
            return true;
        }
        private static void ForestMap(Forest forest, Player player)
        {
            string mapFile = forest?.MapFile;
            string map = "";
            if (!File.Exists(mapFile))
            {
                Console.WriteLine("🗺 No map found for this area.");
            }
            else
                map = File.ReadAllText(mapFile);

            PrintMap(forest.Name, map, player);
        }
        private static void CityMap(City city, Player player)
        {
            string mapFile = city?.MapFile;
            string map = "";
            if (!File.Exists(mapFile))
            {
                Console.WriteLine("🗺 No map found for this area.");
            }
            else
                map = File.ReadAllText(mapFile);

            PrintMap(city.Name, map, player);
        }
        private static void CaveMap(Cave cave, Player player)
        {
            string mapFile = cave?.MapFile;
            string map = "";
            if (!File.Exists(mapFile))
            {
                Console.WriteLine("🗺 No map found for this area.");
            }
            else
                map = File.ReadAllText(mapFile);

            PrintMap(cave.Name, map, player);
        }
        private static void DungonMap(Dungeon dungeon, Player player)
        {
            string mapFile = dungeon?.MapFile;
            string map = "";
            if (!File.Exists(mapFile))
            {
                Console.WriteLine("🗺 No map found for this area.");
            }
            else
                map = File.ReadAllText(mapFile);

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