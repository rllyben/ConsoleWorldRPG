using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Systems;
using ConsoleWorldRPG.Interfaces;
using System;

namespace ConsoleWorldRPG.Services
{
    public class GameService
    {
        private static Dictionary<string, Room> rooms = new();
        public static void InitializeGame(Player player)
        {
            rooms = RoomService.LoadRooms();
            if (rooms.Count == 0)
            {
                Console.WriteLine("No rooms found! Exiting...");
                return;
            }
            player = new("Hero", new Stats());
            player.CurrentRoom = rooms["room1"];
        }

    }

}