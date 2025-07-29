using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;

namespace ConsoleWorldRPG.Items
{
    public abstract class Item
    {
        public string Id { get; protected set; }
        public string Name { get; protected set; }
        public string Description { get; protected set; }
        public int StackSize { get; set; } = 1;
        public virtual int MaxStackSize => 50;

        public virtual int BuyPrice { get; set; } = 100; // default value
        public virtual int SellValue => (int)(BuyPrice * 0.75);

        public abstract void Use(Player player); // base method for using an item

        public virtual bool CanStackWith(Item other)
        {
            return other != null && Id == other.Id;
        }

        public Item CloneOne()
        {
            var copy = (Item)MemberwiseClone();
            copy.StackSize = 1;
            return copy;
        }

    }

}
