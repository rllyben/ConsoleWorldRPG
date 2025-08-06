using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleWorldRPG.Entities.RoomTypes
{
    public class Forest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<int> RoomIds { get; set; } = new();

        public string MapFile => $"Data/Maps/{Id}_map.json";

        public bool ContainsRoom(Room room) => RoomIds.Contains(room.Id);
    }
}
