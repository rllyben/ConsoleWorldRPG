using ConsoleWorldRPG.Entities;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ConsoleWorldRPG.Services
{
    public static class RoomService
    {
        private static readonly string _filePath = "Data\\rooms.json";

        public static Dictionary<string, Room> LoadRooms()
        {
            string test = Path.Combine(Directory.GetCurrentDirectory(), _filePath);
            Debug.WriteLine(Directory.GetCurrentDirectory());
            Debug.WriteLine(_filePath);
            Debug.WriteLine(test);

            if (!File.Exists(_filePath))
                return new();

            string json = File.ReadAllText(_filePath);
            var roomList = JsonSerializer.Deserialize<List<Room>>(json) ?? new();

            // Create lookup map
            var roomMap = roomList.ToDictionary(r => r.Id, r => r);

            // Resolve exits
            foreach (var room in roomMap.Values)
            {
                foreach (var (direction, targetId) in room.ExitIds)
                {
                    if (roomMap.TryGetValue(targetId, out var targetRoom))
                    {
                        room.Exits[direction] = targetRoom;
                    }

                }

            }

            return roomMap;
        }

    }

}