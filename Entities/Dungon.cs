
namespace ConsoleWorldRPG.Entities
{
    public class Dungeon
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<int> RoomIds { get; set; } = new();

        public string MapFile => $"Data/{Id}Map.json";

        public bool ContainsRoom(Room room) => RoomIds.Contains(room.Id);
    }

}