namespace ConsoleWorldRPG.Commands
{
    public static class MovementCommands
    {
        public static bool Handle(string input, Player player)
        {
            if (input == "look")
            {
                Printer.ShowLook(player.CurrentRoom);
                return true;
            }

            if (input.StartsWith("move "))
            {
                string dir = input.Substring(5).Trim();
                if (dir.Length < 2)
                {
                    dir = dir switch
                    {
                        "n" => "north",
                        "e" => "east",
                        "s" => "south",
                        "w" => "west",
                        _   => dir
                    };
                }
                Move(player, dir);
                return true;
            }

            return false;
        }

        private static void Move(Player player, string direction)
        {
            if (!player.CurrentRoom.Exits.TryGetValue(direction, out Room nextRoom))
            {
                Console.WriteLine("You can't go that way.");
                return;
            }

            if (nextRoom.IsDungeonRoom && nextRoom.Monsters.Count == 0)
                nextRoom.IsCleared = true;

            if (player.CurrentRoom.IsDungeonRoom && !player.CurrentRoom.IsCleared)
            {
                Console.WriteLine("🚪 You cannot continue until all enemies in this room are defeated.");
                return;
            }
            else if (player.CurrentRoom.IsDungeonRoom && !nextRoom.IsDungeonRoom)
            {
                Console.WriteLine("🌀 You feel the dungeon magic reset behind you...");
                int dungId = player.CurrentRoom.DungonId;
                foreach (var dungeonRoom in GameService.Rooms.Values.Where(r => r.DungonId == dungId))
                {
                    dungeonRoom.IsCleared = false;
                    dungeonRoom.Corpses = new();
                }
            }

            if (nextRoom.RequirementType == RoomRequirementType.Level && player.Level < nextRoom.AccessLevel)
            {
                Console.WriteLine($"You must be level {nextRoom.AccessLevel} to enter this area.");
                return;
            }

            if (nextRoom.IsCity && nextRoom.Npcs.Contains("Healer"))
                player.LastHealerRoomId = nextRoom.Id;

            player.CurrentRoom = nextRoom;
            _ = ConsoleHubClient.JoinRoomAsync(player.CurrentRoom.Id);
            Console.WriteLine($"\nYou move {direction}.");
            player.CurrentRoom.Describe();
        }

    }

}
