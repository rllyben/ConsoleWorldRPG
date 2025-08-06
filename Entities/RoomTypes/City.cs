namespace ConsoleWorldRPG.Entities.RoomTypes
{
    public class City
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<int> RoomIds { get; set; } = new();
        public bool IsBig { get; set; } = false;
        public string MapFile => $"Data/Maps/{Id}_map.json";

        public bool ContainsRoom(Room room) => RoomIds.Contains(room.Id);
    }

}