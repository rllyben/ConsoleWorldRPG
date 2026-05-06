namespace ConsoleWorldRPG.Commands
{
    public static class GatherCommands
    {
        public static bool Handle(string input, Player player)
        {
            if (!input.StartsWith("gather ")) return false;

            if (player.CurrentRoom.GathersRemaining < 1 ||
                (player.RoomGatheringStatus.TryGetValue(player.CurrentRoom.Id, out var lastGather)
                 && lastGather == DateTime.Now.Date))
            {
                if (player.CurrentRoom.GathersRemaining < 1)
                    player.RoomGatheringStatus[player.CurrentRoom.Id] = DateTime.Now.Date;

                Console.WriteLine("🪓 You've already gathered everything useful here for today.");
                return true;
            }

            string spotName = input.Substring(7).Trim().ToLower();
            var spot = player.CurrentRoom.GatheringSpots.FirstOrDefault(g =>
                g.Id.Equals(spotName, StringComparison.OrdinalIgnoreCase) ||
                g.Name.Equals(spotName, StringComparison.OrdinalIgnoreCase));

            if (spot == null) { Console.WriteLine($"❌ No gathering spot called '{spotName}' found here."); return true; }

            if (!player.HasToolFor(spot.Type)) { Console.WriteLine("🛠 You don't have the required tool to gather here."); return true; }

            if (!ItemFactory.TryCreateItem(spot.GatheredItemId, out var item))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Failed to gather: item '{spot.GatheredItemId}' not found.");
                Console.ResetColor();
                return true;
            }

            if (player.Inventory.AddItem(item, player))
            {
                Console.WriteLine($"🧺 You gathered: {item.Name}");
                player.CurrentRoom.GathersRemaining--;
            }
            else
            {
                Console.WriteLine("❌ Inventory full. You couldn't carry the item.");
            }

            return true;
        }

    }

}
