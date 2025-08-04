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
        public static bool Handle(string input, Player player)
        {
            if (input != "map") return false;

            var positions = MapBuilder.BuildRoomMap(player.CurrentRoom);

            var minX = positions.Values.Min(p => p.x);
            var maxX = positions.Values.Max(p => p.x);
            var minY = positions.Values.Min(p => p.y);
            var maxY = positions.Values.Max(p => p.y);

            Console.WriteLine("\n🗺  World Map:\n");

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    var room = positions.FirstOrDefault(p => p.Value == (x, y)).Key;

                    if (room != null)
                    {
                        if (room.Exits.Keys.Contains("north"))
                            Console.WriteLine("↑");
                        if (room.Exits.Keys.Contains("west"))
                            Console.Write("←");
                        if (room.Exits.Keys.Contains("east"))
                            Console.Write("→");
                        
                        string symbol = room == player.CurrentRoom ? $"[⭐️{room.Name}]" : $"[#{room.Name}]";
                        Console.Write(symbol.PadRight(5));

                        if (room.Exits.Keys.Contains("south"))
                            Console.WriteLine("↓");
                    }
                    else
                    {
                        Console.Write("     ");
                    }

                }
                Console.WriteLine();
            }

            return true;
        }

    }

}
