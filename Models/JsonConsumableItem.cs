using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ConsoleWorldRPG.Items;

namespace ConsoleWorldRPG.Models
{
    public class JsonConsumableItem : ConsumableItem
    {
        public JsonConsumableItem(GameItem def)
        {
            Id = def.Id;
            Name = def.Name;
            Description = def.Description;
            HealAmount = def.HealAmount;
            ManaRestore = def.ManaRestore;
            StackSize = 1;
            _buyPrice = def.BuyPrice;
            _maxStack = def.MaxStackSize;
        }

        private int _buyPrice, _maxStack;
        public override int BuyPrice => _buyPrice;
        public override int MaxStackSize => _maxStack;
    }

}
