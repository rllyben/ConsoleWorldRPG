using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Enums;

namespace ConsoleWorldRPG.Entities
{
    public class GatheringSpot
    {
        public string Id { get; set; }           // e.g., "iron_vein"
        public string Name { get; set; }         // e.g., "Iron Vein"
        public string Description { get; set; }
        public GatheringType Type { get; set; }
        public string GatheredItemId { get; set; } // e.g., "iron_ore"
    }

}
