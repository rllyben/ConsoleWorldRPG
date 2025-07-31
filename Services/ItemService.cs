using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Items;

namespace ConsoleWorldRPG.Services
{
    public static class ItemService
    {
        public static Item? GetItemByNameFromPlayer(string name, Player player)
        {
            Item? item = null;
            item = player.Inventory.Items.First(i => i.Name.ToLower() == name);
            if (item == null && player.WeaponSlot != null && player.WeaponSlot.Name == name) 
                item = player.WeaponSlot;
            else if (item == null && player.ArmorSlot != null && player.ArmorSlot.Name == name)
                item = player.ArmorSlot;
            else if (item == null && player.AccessorySlot != null && player.AccessorySlot.Name == name)
                item = player.AccessorySlot;
            return item;
        }
        public static Item? GetItemByNameFromInventory(string name, Inventory inventory)
        {
            Item? item = null;
            item = inventory.Items.FirstOrDefault(i => i.Name.ToLower() == name);
            return item;
        }

    }

}
