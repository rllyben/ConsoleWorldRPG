using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Utils;

namespace ConsoleWorldRPG.Commands
{
    public static class DebugCommands
    {
        private static bool _debug = false;
        public static bool Handle(string input, Player player)
        {
            if (input == "/debug")
            {
                _debug = !_debug;
                return true;
            }
            if (!_debug)
                return false;
            if (input == "/help")
            {
                Printer.ShowDebugHelp();
                return true;
            }
            else if (input.StartsWith("/set "))
            {
                if (input.Substring(5, 5) == "level")
                {
                    int newLevel = 0;
                    if (int.TryParse(input.Substring(10), out newLevel))
                    {
                        while (player.Level < newLevel)
                        {
                            player.LevelUp();
                        }
                        if (player.Level > newLevel)
                            Console.WriteLine("player level can only be increased!");
                        return true;
                    }

                }

            }
                return false;
        }

    }

}
