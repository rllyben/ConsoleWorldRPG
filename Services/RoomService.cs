using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using ConsoleWorldRPG.Entities;

namespace ConsoleWorldRPG.Services
{
    public static class RoomService
    {
        public static List<Room> AllRooms { get; private set; } = new List<Room>();
        private static readonly string _filePath = "Data/rooms.json";

        /// <summary>
        /// Loads all rooms from Data/rooms.json
        /// </summary>
        /// <returns>returns an room Dictionary with the room id as Key and the rooms as Values</returns>
        public static Dictionary<int, Room> LoadRooms()
        {
            string test = Path.Combine(Directory.GetCurrentDirectory(), _filePath);
            Debug.WriteLine(Directory.GetCurrentDirectory());
            Debug.WriteLine(_filePath);
            Debug.WriteLine(test);

            if (!File.Exists(_filePath))
                return new();

            string json = File.ReadAllText(_filePath);
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };

            AllRooms = JsonSerializer.Deserialize<List<Room>>(json, options) ?? new();

            // Create lookup map
            var roomMap = AllRooms.ToDictionary(r => r.Id, r => r);

            // Resolve exits
            foreach (var room in roomMap.Values)
            {
                if (room.IsDungeonRoom)
                    room.DungonList.Add(room);

                foreach (var (direction, targetId) in room.ExitIds)
                {
                    if (roomMap.TryGetValue(targetId, out var targetRoom))
                    {
                        room.Exits[direction] = targetRoom;
                        if (targetRoom.DungonId == room.DungonId && room.IsDungeonRoom)
                        {
                            room.DungonList.Add(targetRoom);
                        }

                    }

                }

            }

            return roomMap;
        }
        /// <summary>
        /// gives back a room when given its id
        /// </summary>
        /// <param name="id">room id</param>
        /// <returns>room with that id</returns>
        public static Room GetRoomById(int id)
        {
            return AllRooms.FirstOrDefault(r => r.Id == id);
        }
    }

}