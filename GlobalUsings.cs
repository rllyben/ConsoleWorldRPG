// ── MyriaLib entity namespaces ─────────────────────────────────────────────
global using MyriaLib.Entities;
global using MyriaLib.Entities.Players;
global using MyriaLib.Entities.Monsters;
global using MyriaLib.Entities.Maps;
global using MyriaLib.Entities.Items;
global using MyriaLib.Entities.Skills;
global using MyriaLib.Entities.NPCs;

// ── MyriaLib model / enum / interface / util namespaces ───────────────────
global using MyriaLib.Systems.Enums;
global using MyriaLib.Systems.Interfaces;
global using MyriaLib.Models;
global using MyriaLib.Models.Settings;
global using MyriaLib.Utils;

// ── MyriaLib service namespaces that have no name conflicts ───────────────
global using MyriaLib.Services.Builder;     // ItemFactory, SkillFactory, MapBuilder
global using MyriaLib.Services.Regestries;  // DungeonRegistry, CaveRegistry, CityRegistry, ForestRegistry

// ── MyriaLib types that would conflict if their namespace was imported whole ─
// MyriaLib.Services: GameService conflicts with ConsoleWorldRPG.Services.GameService
// MyriaLib.Services.Manager: LoginManager, DayCycleManager  — LoginManager conflicts
// MyriaLib.Systems: Localization conflicts with ConsoleWorldRPG.Systems.Localization
global using CharacterService = MyriaLib.Services.CharacterService;
global using RoomService = MyriaLib.Services.RoomService;
global using SettingsService = MyriaLib.Services.SettingsService;
global using QuestManager = MyriaLib.Services.Manager.QuestManager;
global using JobManager = MyriaLib.Services.Manager.JobManager;
global using DayCycleManager = MyriaLib.Services.Manager.DayCycleManager;
global using LootGenerator = MyriaLib.Systems.LootGenerator;
global using SkillFusionSystem = MyriaLib.Systems.SkillFusionSystem;
global using GameLog = MyriaLib.Systems.GameLog;
global using GameStatus = MyriaLib.Systems.GameStatus;
global using ItemConverter = MyriaLib.Systems.ItemConverter;

// ── ConsoleWorldRPG own types ─────────────────────────────────────────────
global using ConsoleWorldRPG.Services;       // GameService, LoginManager (console wrappers)
global using ConsoleWorldRPG.Systems;        // EncounterRunner, CommandRouter, Localization
global using ConsoleWorldRPG.Utils;          // Printer
global using ConsoleWorldRPG.Entities.NPCs;  // NpcInteractionHandler
