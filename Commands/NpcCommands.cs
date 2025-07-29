using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Entities.NPCs;

namespace ConsoleWorldRPG.Commands
{
    public static class NpcCommands
    {
        public static bool Handle(string input, Player player)
        {
            if (input.StartsWith("go to "))
            {
                string npc = input.Substring(6).Trim();
                NpcInteractionHandler.InteractWithNpc(npc, ref player);
                return true;
            }

            return false;
        }

    }

}
