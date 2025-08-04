using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Items;

namespace ConsoleWorldRPG.Entities
{
    public class Corpse
    {
        public string Name { get; set; }
        public List<Item> Loot { get; set; } = new();
        public bool IsLooted { get; set; } = false;

        public Corpse(string name, List<Item> loot)
        {
            Name = name;
            Loot = loot;
        }
        /// <summary>
        /// discribes the corpse
        /// </summary>
        public void Describe()
        {
            if (IsLooted || Loot.Count == 0)
                Console.WriteLine($"The corpse of {Name} lies here, empty.");
            else
            {
                Console.WriteLine($"The corpse of {Name} lies here.");
                Console.WriteLine("It may contain something...");
            }

        }

    }

}
