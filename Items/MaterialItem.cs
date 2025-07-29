using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;

namespace ConsoleWorldRPG.Items
{
    public class MaterialItem : Item
    {
        public override int BuyPrice => 0; // or define some vendor value
        public override void Use(Player player)
        {
            Console.WriteLine("You can't use a material item directly.");
        }

        public override int MaxStackSize => 99; // or whatever default
    }
}
