namespace ConsoleWorldRPG.Commands
{
    public static class SkillCommands
    {
        public static bool Handle(string input, Player player)
        {
            if (input == "skills")  { ShowSkillsMenu(player); return true; }
            if (input == "combine") { CombineMenu(player);    return true; }
            if (input == "slots")   { SlotsMenu(player);      return true; }
            return false;
        }

        // ── skills ───────────────────────────────────────────────────────────────

        public static void ShowSkillsMenu(Player player)
        {
            Console.WriteLine("\n== Your Skills ==");

            // Learned base skills
            var learned = player.Skills;
            Console.WriteLine($"\nLearned ({learned.Count}):");
            if (learned.Count == 0)
                Console.WriteLine("  (none — defeat enemies and level up to unlock skills)");
            else
                for (int i = 0; i < learned.Count; i++)
                    PrintSkillLine($"  {i + 1,2}.", learned[i]);

            // Combined skills
            var combined = player.CombinedSkills.Where(c => c.ResolvedSkill != null).ToList();
            Console.WriteLine($"\nCombined ({combined.Count}):");
            if (combined.Count == 0)
                Console.WriteLine("  (none — use 'combine' to fuse 2–5 learned skills)");
            else
                foreach (var c in combined)
                {
                    var sk = c.ResolvedSkill!;
                    var src = string.Join(" + ", c.SkillIds.Select(id =>
                        learned.FirstOrDefault(s => s.Id == id)?.Name ?? id));
                    Console.WriteLine($"  • {c.DisplayName,-22}  {SkillSummary(sk),-30}  ({src})");
                }

            // Active slots
            var slots = player.SkillSlots;
            Console.WriteLine($"\nActive slots ({slots.Count} / {player.SkillSlotCount}):");
            if (slots.Count == 0)
                Console.WriteLine("  (none — use 'slots' to add skills to your combat bar)");
            else
                for (int i = 0; i < slots.Count; i++)
                {
                    var slot = slots[i];
                    string tag = slot.Source switch
                    {
                        SlottedSkillSource.Combined       => " [Combined]",
                        SlottedSkillSource.CompositeFusion => " [Fusion]",
                        _ => ""
                    };
                    string name = slot.ResolvedSkill?.Name ?? slot.SkillId;
                    Console.WriteLine($"  [{i + 1}] {name}{tag}");
                }

            Console.WriteLine("\nType 'combine' to create combined skills, 'slots' to manage the combat bar.");
        }

        // ── combine ───────────────────────────────────────────────────────────────

        public static void CombineMenu(Player player)
        {
            var learned = player.Skills;
            if (learned.Count < 2)
            {
                Console.WriteLine("You need at least 2 learned skills to combine. Keep levelling up!");
                return;
            }

            Console.WriteLine("\n== Skill Combination ==");
            Console.WriteLine("Pick 2–5 learned skills to combine (numbers space-separated, repeats allowed).");
            Console.WriteLine("Type 'cancel' to abort.\n");

            for (int i = 0; i < learned.Count; i++)
                PrintSkillLine($"  {i + 1,2}.", learned[i]);

            while (true)
            {
                Console.Write("\n> ");
                string raw = Console.ReadLine()?.Trim() ?? "";
                if (raw.Equals("cancel", StringComparison.OrdinalIgnoreCase)) return;

                var parts = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var ids   = new List<string>();
                bool valid = true;

                foreach (var part in parts)
                {
                    if (!int.TryParse(part, out int idx) || idx < 1 || idx > learned.Count)
                    {
                        Console.WriteLine($"❌ '{part}' is not a valid skill number.");
                        valid = false;
                        break;
                    }
                    ids.Add(learned[idx - 1].Id);
                }

                if (!valid) continue;

                if (ids.Count < 2 || ids.Count > 5)
                {
                    Console.WriteLine("❌ Choose between 2 and 5 skills.");
                    continue;
                }

                // Preview the result before committing
                var preview = SkillCombinationService.Combine(ids);
                if (preview == null)
                {
                    Console.WriteLine("❌ Could not combine those skills.");
                    continue;
                }

                string namesStr = string.Join(" + ", ids.Select(id =>
                    learned.FirstOrDefault(s => s.Id == id)?.Name ?? id));
                Console.WriteLine($"  Combining: {namesStr}");
                Console.WriteLine($"  Result:    {preview.Name}  {SkillSummary(preview)}");
                Console.Write("Confirm? (y/n): ");
                string confirm = Console.ReadLine()?.Trim().ToLower() ?? "";
                if (confirm != "y") { Console.WriteLine("Cancelled."); continue; }

                var result = SkillCombinationService.TryCreateForPlayer(player, ids);
                if (result == null)
                    Console.WriteLine("❌ You already have that combination, or it could not be created.");
                else
                {
                    CharacterService.SaveCharacter(LoginManager.UserAccount, player);
                    Console.WriteLine($"✅ Combined skill created: {result.DisplayName}!");
                    Console.WriteLine("Type 'slots' to add it to your combat bar.");
                }
                return;
            }
        }

        // ── slots ─────────────────────────────────────────────────────────────────

        public static void SlotsMenu(Player player)
        {
            while (true)
            {
                Console.WriteLine($"\n== Skill Slots ({player.SkillSlots.Count} / {player.SkillSlotCount}) ==");

                // Active slots
                Console.WriteLine("Active slots:");
                if (player.SkillSlots.Count == 0)
                    Console.WriteLine("  (empty)");
                else
                    for (int i = 0; i < player.SkillSlots.Count; i++)
                    {
                        var s   = player.SkillSlots[i];
                        string tag  = s.Source switch
                        {
                            SlottedSkillSource.Combined        => " [Combined]",
                            SlottedSkillSource.CompositeFusion => " [Fusion]",
                            _ => ""
                        };
                        Console.WriteLine($"  [{i + 1}] {s.ResolvedSkill?.Name ?? s.SkillId}{tag}");
                    }

                // Available skills not yet slotted — build a lettered list
                bool atCap = player.SkillSlots.Count >= player.SkillSlotCount;
                var available = BuildAvailableList(player);

                Console.WriteLine("\nAvailable to slot:");
                if (available.Count == 0 || atCap)
                    Console.WriteLine(atCap ? "  (slots full)" : "  (none)");
                else
                    for (int i = 0; i < available.Count; i++)
                    {
                        char letter  = (char)('a' + i);
                        var  (id, name, src) = available[i];
                        string tag   = src switch
                        {
                            SlottedSkillSource.Combined        => " [Combined]",
                            SlottedSkillSource.CompositeFusion => " [Fusion]",
                            _ => " [Regular]"
                        };
                        Console.WriteLine($"  {letter}. {name}{tag}");
                    }

                Console.WriteLine("\nCommands: slot <letter>, unslot <number>, move <from> <to>, done");
                Console.Write("> ");
                string line = Console.ReadLine()?.Trim().ToLower() ?? "";

                if (line == "done") return;

                if (line.StartsWith("slot "))
                {
                    string arg = line[5..].Trim();
                    if (arg.Length == 1 && arg[0] >= 'a')
                    {
                        int idx = arg[0] - 'a';
                        if (idx >= 0 && idx < available.Count)
                        {
                            var (id, name, src) = available[idx];
                            if (SkillSlotService.TryAddSlot(player, src, id))
                            {
                                CharacterService.SaveCharacter(LoginManager.UserAccount, player);
                                Console.WriteLine($"✅ {name} added to slot {player.SkillSlots.Count}.");
                            }
                            else
                                Console.WriteLine("❌ Could not add that skill (already slotted or slots full).");
                        }
                        else Console.WriteLine("❌ No such letter.");
                    }
                    else Console.WriteLine("❌ Enter a letter (e.g. 'slot a').");
                }
                else if (line.StartsWith("unslot "))
                {
                    string arg = line[7..].Trim();
                    if (int.TryParse(arg, out int num) && num >= 1 && num <= player.SkillSlots.Count)
                    {
                        var slot = player.SkillSlots[num - 1];
                        string name = slot.ResolvedSkill?.Name ?? slot.SkillId;
                        SkillSlotService.RemoveSlot(player, slot.Source, slot.SkillId);
                        CharacterService.SaveCharacter(LoginManager.UserAccount, player);
                        Console.WriteLine($"✅ {name} removed from slot {num}.");
                    }
                    else Console.WriteLine("❌ Enter a valid slot number.");
                }
                else if (line.StartsWith("move "))
                {
                    var parts = line[5..].Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2
                        && int.TryParse(parts[0], out int from) && from >= 1 && from <= player.SkillSlots.Count
                        && int.TryParse(parts[1], out int to)   && to   >= 1 && to   <= player.SkillSlots.Count)
                    {
                        SkillSlotService.ReorderSlots(player, from - 1, to - 1);
                        CharacterService.SaveCharacter(LoginManager.UserAccount, player);
                        Console.WriteLine($"✅ Moved slot {from} → slot {to}.");
                    }
                    else Console.WriteLine("❌ Usage: move <from> <to>  (e.g. 'move 1 3').");
                }
                else
                {
                    Console.WriteLine("❌ Unknown command. Use: slot <letter>, unslot <number>, move <from> <to>, done.");
                }
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private static void PrintSkillLine(string prefix, Skill skill)
        {
            Console.WriteLine($"{prefix} {skill.Name,-22}  {SkillSummary(skill)}");
        }

        private static string SkillSummary(Skill skill)
        {
            string target = skill.Target switch
            {
                SkillTarget.AllEnemies => "All enemies",
                SkillTarget.Self       => "Self",
                _                      => "Single enemy"
            };
            return $"{skill.Type,-8} · {target,-12}  MP {skill.ManaCost}";
        }

        private static List<(string Id, string Name, SlottedSkillSource Source)> BuildAvailableList(Player player)
        {
            var list = new List<(string, string, SlottedSkillSource)>();

            foreach (var s in player.Skills)
            {
                if (!player.SkillSlots.Any(sl => sl.Source == SlottedSkillSource.Regular && sl.SkillId == s.Id))
                    list.Add((s.Id, s.Name, SlottedSkillSource.Regular));
            }
            foreach (var c in player.CombinedSkills.Where(c => c.ResolvedSkill != null))
            {
                if (!player.SkillSlots.Any(sl => sl.Source == SlottedSkillSource.Combined && sl.SkillId == c.Id))
                    list.Add((c.Id, c.DisplayName, SlottedSkillSource.Combined));
            }
            foreach (var f in player.CompositeSkills.Where(f => f.ResolvedSkill != null))
            {
                if (!player.SkillSlots.Any(sl => sl.Source == SlottedSkillSource.CompositeFusion && sl.SkillId == f.Id))
                    list.Add((f.Id, f.DisplayName, SlottedSkillSource.CompositeFusion));
            }
            return list;
        }
    }
}
