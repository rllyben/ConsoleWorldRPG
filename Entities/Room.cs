using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleWorldRPG.Entities
{
    public class Room
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public Dictionary<string, string> ExitIds { get; set; } = new(); // "north": "room2"
        public Dictionary<string, Room> Exits { get; set; } = new();     // populated after loading


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
                foreach (var dir in Exits.Keys)
                    Console.Write($"{dir} ");
                Console.WriteLine();
            }
            
        }

    }

}
