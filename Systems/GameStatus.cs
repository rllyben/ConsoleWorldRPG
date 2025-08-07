using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleWorldRPG.Systems
{
    [Serializable]
    public class GameStatus
    {
        public DateTime LastUpdateTime { get; set; }
        public Dictionary<string, DateTime> RoomGatheringStatus { get; set; } = new();
    }

}
