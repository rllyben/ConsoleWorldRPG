using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleWorldRPG.Items
{
    public class UniqueLootEntry
    {
        public string ItemId { get; set; }
        public float DropChance { get; set; }  // 0.0 to 1.0
    }

}
