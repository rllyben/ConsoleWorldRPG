using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Services;

namespace ConsoleWorldRPG.Commands
{public static class MapCommands
    {
        public static bool Handle(string input, Player player)
        {
            if (input != "map") return false;

            Dungeon? dungeon = DungeonRegistry.GetDungeonByRoom(player.CurrentRoom);
            string mapFile = dungeon?.MapFile ?? "Data/Maps/world_map.json";

            if (!File.Exists(mapFile))
            {
                Console.WriteLine("🗺 No map found for this area.");
                return true;
            }

            string map = File.ReadAllText(mapFile);

            // Insert ⭐ after the current room name (first match only)
            string roomName = player.CurrentRoom.Name;
            if (!string.IsNullOrWhiteSpace(roomName) && map.Contains(roomName))
            {
                map = map.Replace(roomName, roomName + " ⭐", StringComparison.Ordinal);
            }

            Console.WriteLine($"\n📍 Map: {(dungeon?.Name ?? "World")}\n");
            Console.WriteLine(map);
            return true;
        }

    }

}