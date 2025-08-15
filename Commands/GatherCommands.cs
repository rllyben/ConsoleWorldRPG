using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Services;

namespace ConsoleWorldRPG.Commands
{
    public static class GatherCommands
    {
        /// <summary>
        /// Handels gather commands
        /// </summary>
        /// <param name="input">player input</param>
        /// <param name="player">player character</param>
        /// <returns>if the command was found</returns>
        public static bool Handle(string input, Player player)
        {
            if (!input.StartsWith("gather ")) return false;

            if (player.CurrentRoom.GathersRemaining < 1 || player.RoomGatheringStatus.TryGetValue(player.CurrentRoom.Id, out var lastGatherTime) && lastGatherTime == DateTime.Now.Date)
            {
                if (player.CurrentRoom.GathersRemaining < 1) 
                    player.RoomGatheringStatus[player.CurrentRoom.Id] = DateTime.Now.Date;

                Console.WriteLine("🪓 You've already gathered everything useful here for today.");
                return true;
            }

            string spotName = input.Substring(7).Trim().ToLower();
            var room = player.CurrentRoom;

            var spot = room.GatheringSpots.FirstOrDefault(g =>
                g.Id.Equals(spotName, StringComparison.OrdinalIgnoreCase) ||
                g.Name.Equals(spotName, StringComparison.OrdinalIgnoreCase));

            if (spot == null)
            {
                Console.WriteLine($"❌ No gathering spot called '{spotName}' found here.");
                return true;
            }

            if (!player.HasToolFor(spot.Type))
            {
                Console.WriteLine("🛠 You don't have the required tool to gather here.");
                return true;
            }

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
                Console.WriteLine("❌ Inventory full. You couldn't carry the item.");

            return true;
        }

    }

}
