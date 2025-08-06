using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Items;

namespace ConsoleWorldRPG.Utils
{
    public static class InventoryUtils
    {
        public static Item ResolveInventoryItem(string input, Player player)
        {
            var matches = player.Inventory.Items
                .Where(i => i.Name.StartsWith(input, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matches.All(i => i.Name.ToLower() == input) || matches.Count < 2)
                return matches[0];
            else
                return null;
        }

    }

}