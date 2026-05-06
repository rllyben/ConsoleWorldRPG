namespace ConsoleWorldRPG.Commands
{
    public static class MapCommands
    {
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
                var searchDungeon = DungeonRegistry.GetDungeonByName(mapName);
                var searchCave    = CaveRegistry.GetCaveByName(mapName);
                var searchCity    = CityRegistry.GetCityByName(mapName);
                var searchForest  = ForestRegistry.GetForestByName(mapName);

                if (searchDungeon != null)      PrintAreaMap(searchDungeon.Name, searchDungeon.MapFile, player);
                else if (searchCave != null)    PrintAreaMap(searchCave.Name, searchCave.MapFile, player);
                else if (searchCity != null)    PrintAreaMap(searchCity.Name, searchCity.MapFile, player);
                else if (searchForest != null)  PrintAreaMap(searchForest.Name, searchForest.MapFile, player);
                else Console.WriteLine("🗺 No map found for this area.");
            }
            else if (cave == null && dungeon == null && forest == null &&
                     (city == null || !city.IsBig) || input == "map world")
            {
                PrintAreaMap("World", "Data/Maps/world_map.json", player);
            }
            else if (cave != null)    PrintAreaMap(cave.Name, cave.MapFile, player);
            else if (dungeon != null) PrintAreaMap(dungeon.Name, dungeon.MapFile, player);
            else if (city != null && city.IsBig) PrintAreaMap(city.Name, city.MapFile, player);
            else if (forest != null)  PrintAreaMap(forest.Name, forest.MapFile, player);

            return true;
        }

        private static void PrintAreaMap(string areaName, string? mapFile, Player player)
        {
            if (string.IsNullOrEmpty(mapFile) || !File.Exists(mapFile))
            {
                Console.WriteLine("🗺 No map found for this area.");
                return;
            }

            string map = File.ReadAllText(mapFile);
            string roomName = player.CurrentRoom.Name;
            if (!string.IsNullOrWhiteSpace(roomName) && map.Contains(roomName))
                map = map.Replace(roomName, roomName + " ⭐", StringComparison.Ordinal);

            Console.WriteLine($"\n📍 Map: {areaName}\n");
            Console.WriteLine(map);
        }

    }

}
