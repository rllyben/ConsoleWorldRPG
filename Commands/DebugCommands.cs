namespace ConsoleWorldRPG.Commands
{
    public static class DebugCommands
    {
        private static bool _debug = false;

        public static bool Handle(string input, Player player)
        {
            if (input == "/debug") { _debug = !_debug; return true; }
            if (!_debug) return false;

            if (input == "/help")
            {
                Printer.ShowDebugHelp();
                return true;
            }
            else if (input.StartsWith("/set ") && input.Length > 10 && input.Substring(5, 5) == "level")
            {
                if (int.TryParse(input.Substring(10), out int newLevel))
                {
                    while (player.Level < newLevel)
                        player.LevelUp();
                    if (player.Level > newLevel)
                        Console.WriteLine("Player level can only be increased!");
                    return true;
                }
            }
            else if (input == "/skillmaster")
            {
                NpcInteractionHandler.SkillMasterMenu(player);
                return true;
            }

            return false;
        }

    }

}
