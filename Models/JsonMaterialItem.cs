using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Items;

namespace ConsoleWorldRPG.Models
{
    public class JsonMaterialItem : ConsumableItem
    {
        public JsonMaterialItem(GameItem def)
        {
            Id = def.Id;
            Name = def.Name;
            Description = def.Description;
            StackSize = 1;
            _buyPrice = def.BuyPrice;
            _maxStack = def.MaxStackSize;
        }

        private int _buyPrice, _maxStack;
        public override int BuyPrice => _buyPrice;
        public override int MaxStackSize => _maxStack;
    }
}
