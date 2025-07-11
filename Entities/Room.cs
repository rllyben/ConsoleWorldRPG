using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleWorldRPG.Entities
{
    public class Room
    {
        public string Name { get; }
        public string Description { get; }

        // Directions: north, south, east, west
        public Dictionary<string, Room> Exits { get; } = new();

        public Room(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public void ConnectRoom(string direction, Room room)
        {
            Exits[direction.ToLower()] = room;
        }

        public void Describe()
        {
            Console.WriteLine($"You are in {Name}.");
            Console.WriteLine(Description);

            if (Exits.Count > 0)
            {
                Console.Write("Exits: ");
                foreach (var exit in Exits.Keys)
                {
                    Console.Write($"{exit} ");
                }
                Console.WriteLine();
            }

        }

    }

}
