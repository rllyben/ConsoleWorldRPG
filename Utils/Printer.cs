namespace ConsoleWorldRPG.Utils
{
    public class Printer
    {
        public static void ShowInvalidCommand()
        {
            Console.WriteLine("❌ Unknown command. Type 'help' to see available commands.");
        }

        public static void PrintColoredItemName(Item item)
        {
            string displayName = item is EquipmentItem eq && eq.UpgradeLevel > 0
                                 ? $"{item.Name} +{eq.UpgradeLevel}"
                                 : item.Name;
            if (item.Rarity == ItemRarity.Godly)
            {
                PrintRainbow(displayName);
                return;
            }
            var color = item.Rarity switch
            {
                ItemRarity.Common    => ConsoleColor.Gray,
                ItemRarity.Uncommon  => ConsoleColor.Green,
                ItemRarity.Rare      => ConsoleColor.Blue,
                ItemRarity.Epic      => ConsoleColor.Magenta,
                ItemRarity.Unique    => ConsoleColor.Yellow,
                ItemRarity.Legendary => ConsoleColor.DarkYellow,
                _                    => ConsoleColor.Gray
            };
            Console.ForegroundColor = color;
            Console.Write($" - {displayName}");
            Console.ResetColor();
        }

        private static void PrintRainbow(string text)
        {
            var colors = new[]
            {
                ConsoleColor.Red, ConsoleColor.Yellow, ConsoleColor.Green,
                ConsoleColor.Cyan, ConsoleColor.Blue, ConsoleColor.Magenta
            };
            int i = 0;
            foreach (char c in text)
            {
                Console.ForegroundColor = colors[i++ % colors.Length];
                Console.Write(c);
            }
            Console.ResetColor();
        }

        public static void ShowLook(Room room)
        {
            Console.WriteLine($"\n{room.Name}: {room.Description}");

            if (room.Exits.Any())
                Console.WriteLine("Exits: " + string.Join(", ", room.Exits.Keys));

            if (room.IsCity && room.Npcs.Count > 0)
            {
                Console.WriteLine("You see the following people:");
                foreach (var npc in room.Npcs)
                    Console.WriteLine($"  - {npc}");
            }

            if (room.Corpses.Any())
            {
                Console.WriteLine("There are corpses here:");
                foreach (var corpse in room.Corpses)
                    corpse.Describe();
            }

            if (room.GatheringSpots.Any())
            {
                Console.WriteLine("\nYou notice the following gathering spots:");
                foreach (var spot in room.GatheringSpots)
                    Console.WriteLine($"  - {spot.Name}: {spot.Description}");
            }

            if (room.HasMonsters)
            {
                Console.WriteLine("\nYou see the following Monsters:");
                foreach (Monster monster in room.Monsters)
                    Console.WriteLine($"  - {monster.Name}");
            }
        }

        public static void ShowHelp()
        {
            Console.WriteLine("\nAvailable commands:");
            Console.WriteLine("  move <direction>           - Move to another room (e.g., move north)");
            Console.WriteLine("  look                       - Re-describe the current room");
            Console.WriteLine("  fight                      - Starts an encounter if monsters are present");
            Console.WriteLine("  status                     - Show your current health, mana, stats and equipped items");
            Console.WriteLine("  character                  - Full character sheet: race, stat breakdown, derived stats");
            Console.WriteLine("  stats                      - Alias for character");
            Console.WriteLine("  alloc <stat> [n]           - Spend unspent stat points (str/dex/end/int/spr)");
            Console.WriteLine("  jobs                       - List your job levels (Skill / Knowledge / Fame per job)");
            Console.WriteLine("  skills                     - List learned skills, combined skills, and active slots");
            Console.WriteLine("  combine                    - Fuse 2–5 learned skills into a combined skill");
            Console.WriteLine("  slots                      - Manage which skills are active in your combat bar");
            Console.WriteLine("  inventory                  - Show your inventory and moneybag");
            Console.WriteLine("  go to <npc name>           - Go to the specified NPC (e.g., go to healer)");
            Console.WriteLine("  equip <item name>          - Equip the specified item");
            Console.WriteLine("  use <item name>            - Use the specified item (e.g., use simple Healing Potion)");
            Console.WriteLine("  loot                       - Loot the first dropped corpse in the current room");
            Console.WriteLine("  gather <source name>       - Gather material from the given source");
            Console.WriteLine("  map                        - Print the world/dungeon/cave/forest/city map");
            Console.WriteLine("  help                       - Show this help message");
            Console.WriteLine("  say <message>              - Send a chat message to the room (online)");
            Console.WriteLine("  g <message>                - Send a global chat message (online)");
            Console.WriteLine("  w <player> <message>       - Send a private whisper (online)");
            Console.WriteLine("  whisper <player> <message> - Alias for w");
            Console.WriteLine("  party                      - Show current party members");
            Console.WriteLine("  party <message>            - Send a message to your party (online)");
            Console.WriteLine("  party invite <player>      - Invite a player to your party (online)");
            Console.WriteLine("  party accept <id>          - Accept a party invite (online)");
            Console.WriteLine("  party leave                - Leave the current party (online)");
            Console.WriteLine("  party kick <player>        - Kick a player from your party (leader only, online)");
            Console.WriteLine("  party promote <player>     - Transfer party leadership (leader only, online)");
            Console.WriteLine("  friend list                - Show your friends list (online)");
            Console.WriteLine("  friend requests            - Show pending friend requests (online)");
            Console.WriteLine("  friend add <name>          - Send a friend request (online)");
            Console.WriteLine("  friend accept <id>         - Accept a friend request (online)");
            Console.WriteLine("  friend remove <id>         - Remove a friend (online)");
            Console.WriteLine("  logout                     - Save and log out");
            Console.WriteLine("  exit                       - Save and quit the game");
        }

        public static void ShowDebugHelp()
        {
            Console.WriteLine("\nDebug commands:");
            Console.WriteLine("  /set level <level>  - Set the current player level (can only increase)");
            Console.WriteLine("  /skillmaster        - Open the Skill Master menu");
            Console.WriteLine("  /help               - Show this help message");
        }

        public static void ShowStatus(Player player)
        {
            Console.WriteLine($"\n{player.Name}'s Status:");
            Console.ForegroundColor = ConsoleColor.Cyan;
            int    classLvl = ClassManager.GetClassLevel(player, player.Class);
            string classXp  = JobXpService.FormatProgress(ClassManager.GetClassXp(player, player.Class));
            Console.WriteLine($"  Class: {player.Class}  (Lv {classLvl}  {classXp})");
            Console.ResetColor();
            Console.WriteLine($"  Level: {player.Level}    Exp: {player.Experience}/{player.ExpForNextLvl}\n");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  HP: {player.CurrentHealth}/{player.MaxHealth}");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"  Mana: {player.CurrentMana}/{player.MaxMana}");
            Console.ResetColor();
            Console.WriteLine($"  STR: {player.Stats.Strength} + {player.GetBonusFromGear(g => g.BonusSTR)} from gear");
            Console.WriteLine($"  DEX: {player.Stats.Dexterity} + {player.GetBonusFromGear(g => g.BonusDEX)} from gear");
            Console.WriteLine($"  END: {player.Stats.Endurance} + {player.GetBonusFromGear(g => g.BonusEND)} from gear");
            Console.WriteLine($"  INT: {player.Stats.Intelligence} + {player.GetBonusFromGear(g => g.BonusINT)} from gear");
            Console.WriteLine($"  SPR: {player.Stats.Spirit} + {player.GetBonusFromGear(g => g.BonusSPR)} from gear");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"\n  ATK:  {player.TotalPhysicalAttack  - player.GetBonusFromGear(g => g.BonusATK)}  + {player.GetBonusFromGear(g => g.BonusATK)} from gear");
            Console.WriteLine($"  DEF:  {player.TotalPhysicalDefense - player.GetBonusFromGear(g => g.BonusDEF)}  + {player.GetBonusFromGear(g => g.BonusDEF)} from gear");
            Console.WriteLine($"  MATK: {player.TotalMagicAttack     - player.GetBonusFromGear(g => g.BonusMATK)} + {player.GetBonusFromGear(g => g.BonusMATK)} from gear");
            Console.WriteLine($"  MDEF: {player.TotalMagicDefense    - player.GetBonusFromGear(g => g.BonusMDEF)} + {player.GetBonusFromGear(g => g.BonusMDEF)} from gear");
            Console.WriteLine($"  Aim:  {player.TotalAim     - player.GetBonusFromGear(g => g.BonusAim)}      + {player.GetBonusFromGear(g => g.BonusAim)} from gear");
            Console.WriteLine($"  Eva:  {player.TotalEvasion - player.GetBonusFromGear(g => g.BonusEvasion)} + {player.GetBonusFromGear(g => g.BonusEvasion)} from gear");
            Console.WriteLine($"  Crit: {player.CritChance:P0} | Block: {player.BlockChance:P0}");
            Console.ResetColor();

            Console.WriteLine("\nEquipped:");
            Console.Write("  Weapon:    ");
            if (player.WeaponSlot == null) Console.Write("(none)"); else PrintColoredItemName(player.WeaponSlot);
            Console.WriteLine();
            Console.Write("  Armor:     ");
            if (player.ArmorSlot == null) Console.Write("(none)"); else PrintColoredItemName(player.ArmorSlot);
            Console.WriteLine();
            Console.Write("  Accessory: ");
            if (player.AccessorySlot == null) Console.Write("(none)"); else PrintColoredItemName(player.AccessorySlot);
            Console.WriteLine();
        }

        public static void ShowCharacter(Player player)
        {
            Console.WriteLine($"\n== {player.Name} ==");
            Console.WriteLine($"  Race:  {player.Race}");

            int classLvl = ClassManager.GetClassLevel(player, player.Class);
            string classXp = JobXpService.FormatProgress(ClassManager.GetClassXp(player, player.Class));
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  Class: {player.Class}  (Lv {classLvl}  {classXp})");
            Console.ResetColor();
            Console.WriteLine($"  Level: {player.Level}    Exp: {player.Experience}/{player.ExpForNextLvl}");

            Console.WriteLine("\n── Base Stats ───────────────────────");
            PrintStatLine("STR", player.Stats.Strength, player.Stats.StrengthBonus,    ClassManager.GetClassBonusForStat(player, "STR"), player.GetBonusFromGear(g => g.BonusSTR));
            PrintStatLine("DEX", player.Stats.Dexterity, player.Stats.DexterityBonus,   ClassManager.GetClassBonusForStat(player, "DEX"), player.GetBonusFromGear(g => g.BonusDEX));
            PrintStatLine("END", player.Stats.Endurance, player.Stats.EnduranceBonus,   ClassManager.GetClassBonusForStat(player, "END"), player.GetBonusFromGear(g => g.BonusEND));
            PrintStatLine("INT", player.Stats.Intelligence, player.Stats.IntelligenceBonus, ClassManager.GetClassBonusForStat(player, "INT"), player.GetBonusFromGear(g => g.BonusINT));
            PrintStatLine("SPR", player.Stats.Spirit, player.Stats.SpiritBonus,        ClassManager.GetClassBonusForStat(player, "SPR"), player.GetBonusFromGear(g => g.BonusSPR));

            if (player.Stats.UnusedPoints > 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n  ★ {player.Stats.UnusedPoints} unspent stat point(s) — use 'alloc <stat> [n]' to spend them");
                Console.ResetColor();
            }

            Console.WriteLine("\n── Derived Stats ────────────────────");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  HP:   {player.CurrentHealth}/{player.MaxHealth}");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"  MP:   {player.CurrentMana}/{player.MaxMana}");
            Console.ResetColor();
            Console.WriteLine($"  ATK:  {player.TotalPhysicalAttack}");
            Console.WriteLine($"  DEF:  {player.TotalPhysicalDefense}");
            Console.WriteLine($"  MATK: {player.TotalMagicAttack}");
            Console.WriteLine($"  MDEF: {player.TotalMagicDefense}");
            Console.WriteLine($"  Aim:  {player.TotalAim}");
            Console.WriteLine($"  Eva:  {player.TotalEvasion}");
            Console.WriteLine($"  Crit: {player.CritChance:P0}  Block: {player.BlockChance:P0}");
        }

        private static void PrintStatLine(string name, int @base, int allocated, int classBon, int gear)
        {
            int total = @base + allocated + classBon + gear;
            string parts = "";
            if (allocated > 0) parts += $" +{allocated} alloc";
            if (classBon  > 0) parts += $" +{classBon} class";
            if (gear      > 0) parts += $" +{gear} gear";
            Console.WriteLine($"  {name}: {@base}{parts}  = {total}");
        }

        public static void ShowEquipSuccess(Item item) => Console.WriteLine($"✔ Equipped {item.Name}.");
        public static void ShowEquipFail(string reason) => Console.WriteLine($"❌ Cannot equip: {reason}");

        public static void ShowJobs(Player player)
        {
            Console.WriteLine("\n== Your Jobs ==");

            if (player.Jobs.Count == 0)
            {
                Console.WriteLine("You haven't started any jobs yet. Visit a job master to get started.");
                return;
            }

            if (!string.IsNullOrEmpty(player.ActiveJobId))
            {
                string activeName = JobManager.GetById(player.ActiveJobId)?.Name ?? player.ActiveJobId;
                var    cd         = JobManager.GetCooldownRemaining(player);
                string cdStr      = cd > TimeSpan.Zero ? $"  (switch cooldown: {FormatCooldown(cd)})" : "  (cooldown: ready)";
                Console.WriteLine($"Active: {activeName}{cdStr}");
            }
            else
            {
                Console.WriteLine("No active job.");
            }
            Console.WriteLine();

            foreach (var pj in player.Jobs)
            {
                var    job      = JobManager.GetById(pj.JobId);
                string name     = job?.Name ?? pj.JobId;
                bool   isActive = pj.JobId == player.ActiveJobId;
                string tag      = isActive ? "  [ACTIVE]" : "";

                Console.WriteLine($"  {name}{tag}");
                Console.WriteLine($"    Skill:     Lv {JobXpService.GetLevel(pj.SkillXp),-3}  {JobXpService.FormatProgress(pj.SkillXp)}");
                Console.WriteLine($"    Knowledge: Lv {JobXpService.GetLevel(pj.KnowledgeXp),-3}  {JobXpService.FormatProgress(pj.KnowledgeXp)}");
                Console.WriteLine($"    Fame:      Lv {JobXpService.GetLevel(pj.FameXp),-3}  {JobXpService.FormatProgress(pj.FameXp)}");
            }
        }

        public static string FormatCooldown(TimeSpan cd)
        {
            if (cd.TotalDays >= 1)  return $"{(int)cd.TotalDays}d {cd.Hours}h";
            if (cd.TotalHours >= 1) return $"{(int)cd.TotalHours}h {cd.Minutes}m";
            return $"{cd.Minutes}m";
        }

    }

}
