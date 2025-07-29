using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Enums;

namespace ConsoleWorldRPG.Entities
{
    public class Room
    {
        public int Id { get; set; } = 0;
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public bool HasMonsters { get; set; }
        public RoomRequirementType RequirementType { get; set; } = RoomRequirementType.None;
        public int AccessLevel { get; set; } = 0;
        public string RequiredQuestId { get; set; } = null!;
        public bool RequiresParty { get; set; } = false;
        public bool IsCity { get; set; } = false;
        public List<string> Npcs { get; set; } = new(); // e.g. ["Healer", "Smith"]
        public Dictionary<string, int> ExitIds { get; set; } = new(); // "north": "2"
        public Dictionary<string, Room> Exits { get; set; } = new();     // populated after loading
        public Dictionary<int, float> EncounterableMonsters { get; set; } = new();
        public List<Monster> Monsters { get; set; } = new List<Monster>(); 
        public List<Corpse> Corpses { get; set; } = new();

        public Room(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public void ConnectRoom(string direction, Room room)
        {
            Exits[direction] = room;
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


            if (IsCity && Npcs.Count > 0)
            {
                Console.WriteLine("You see the following people here:");
                foreach (var npc in Npcs)
                {
                    Console.WriteLine($"  - {npc}");
                }
            }
            if (Corpses.Any())
            {
                Console.WriteLine("You see the following corpses:");
                foreach (var corpse in Corpses)
                    corpse.Describe();
            }

        }

    }

}
