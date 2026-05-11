namespace ConsoleWorldRPG.Commands
{
    public static class NpcCommands
    {
        public static bool Handle(string input, Player player)
        {
            if (input.StartsWith("go to "))
            {
                string npc = input.Substring(6).Trim();
                NpcInteractionHandler.InteractWithNpc(npc, player);
                return true;
            }

            return false;
        }

    }

}
