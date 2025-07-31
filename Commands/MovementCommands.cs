using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Enums;
using ConsoleWorldRPG.Services;
using ConsoleWorldRPG.Utils;

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
                Move(player, dir);
                return true;
            }

            return false;
        }
        /// <summary>
        /// Moves the player in the specified direction or room
        /// </summary>
        /// <param name="direction">the direction choosen by the player</param>
        private static void Move(Player player, string direction)
        {
            if (player.CurrentRoom.Exits.TryGetValue(direction, out Room nextRoom))
            {
                if (player.CurrentRoom.IsDungeonRoom && !player.CurrentRoom.IsCleared)
                {
                    Console.WriteLine("🚪 You cannot continue until all enemies in this room are defeated.");
                    return;
                }
                else if (player.CurrentRoom.IsDungeonRoom && !nextRoom.IsDungeonRoom)
                {
                    Console.WriteLine("🌀 You feel the dungeon magic reset behind you...");
                    foreach (Room dungonRoom in player.CurrentRoom.DungonList)
                    {
                        dungonRoom.IsCleared = false;
                        dungonRoom.Corpses = new();
                    }

                }
                if (nextRoom.RequirementType != RoomRequirementType.None)
                {
                    if (nextRoom.RequirementType == RoomRequirementType.Level && player.Level < nextRoom.AccessLevel)
                    {
                        Console.WriteLine($"You must be level {nextRoom.AccessLevel} to enter this area.");
                        return;
                    }

                }
                if (nextRoom.IsCity && nextRoom.Npcs.Contains("Healer"))
                {
                    player.LastHealerRoomId = nextRoom.Id;
                }
                player.CurrentRoom = nextRoom;
                Console.WriteLine($"\nYou move {direction}.");
                player.CurrentRoom.Describe();
            }
            else
            {
                Console.WriteLine("You can't go that way.");
            }

        }

    }

}
