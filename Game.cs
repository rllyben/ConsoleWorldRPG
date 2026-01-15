using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ConsoleWorldRPG.Entities;
using ConsoleWorldRPG.Entities.NPCs;
using ConsoleWorldRPG.Enums;
using ConsoleWorldRPG.Items;
using ConsoleWorldRPG.Models;
using ConsoleWorldRPG.Services;
using ConsoleWorldRPG.Systems;
using ConsoleWorldRPG.Utils;
using Microsoft.VisualBasic;

namespace ConsoleWorldRPG
{
    public class Game
    {
        private bool _isRunning = true;
        private Player _player = new Player("Hero", new Stats());
        CommandRouter commandRouter;
        public static bool LogOut = false;
        /// <summary>
        /// Handels the Main User interaction and calles the initialatiation of the Game
        /// </summary>
        public void Start()
        {
            // Initialization logic here
            Console.WriteLine(Localization.T("app.startmassage"));
            if (GameService.InitializeGame(_player))
            {
                while (_isRunning)
                {
                    LogOut = false;
                    _player = ShowLoginMenu();
                    if (_player == null)
                        _isRunning = false;

                    if (_isRunning)
                    {
                        commandRouter = new CommandRouter(_player);

                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(Localization.T("app.helpinfo"));
                        Console.ResetColor();

                        _player.CurrentRoom.Describe();
                    }
                    // Main loop
                    while (true)
                    {
                        Console.Write("> ");
                        string? input = Console.ReadLine()?.Trim();

                        if (string.IsNullOrEmpty(input)) continue;
                        if (input == "exit")
                        {
                            JsonSaveService.SaveCharacter(LoginManager.UserAccount, _player);
                            _isRunning = false;
                            Console.WriteLine(Localization.T("app.goodbye"));
                            break;
                        }
                        if (LogOut)
                            break;
                        else if (!commandRouter.HandleCommand(input))
                            Console.WriteLine(Localization.T("ui_unknown_command"));
                    }

                }

            }

        }
        public static Player ShowLoginMenu()
        {
            while (true)
            {
                Console.WriteLine("\n==== " + Localization.T("ui_login_menue") + " ====");
                Console.WriteLine("0. "+ Localization.T("ui_exit"));
                Console.WriteLine("1. "+ Localization.T("ui_login"));
                Console.WriteLine("2. "+ Localization.T("ui_register"));
                Console.WriteLine("3. "+ Localization.T("ui_settings"));
                Console.Write(Localization.T("ui_select_option"));

                string? input = Console.ReadLine()?.Trim();

                switch (input)
                {
                    case "0":
                        return null;

                    case "1":
                        var user = LoginManager.Login();
                        if (user != null)
                            return user;
                        break;

                    case "2":
                        LoginManager.Register();
                        break;

                    case "3":
                        ShowSettingsMenu();
                        break;

                    default:
                        Console.WriteLine(Localization.T("ui_invalid_option"));
                        break;
                }

            }

        }
        private static void ShowSettingsMenu()
        {
            Console.WriteLine("\n== " + Localization.T("ui_settings") + " ==");
            Console.WriteLine("1. " + Localization.T("ui_language") + " (" + SettingsService.Current.Language + ")");
            Console.WriteLine("0. " + Localization.T("ui_back"));
            Console.Write("> ");
            var choice = Console.ReadLine();

            if (choice == "1")
            {
                Console.WriteLine(Localization.T("ui_select_language") + "1) En  2) De");
                Console.Write("> ");
                var l = Console.ReadLine();
                SettingsService.Current.Language = l switch
                {
                    "2" => GameLanguage.DE,
                    _ => GameLanguage.En
                };
                SettingsService.Save();
                Localization.Load(SettingsService.Current.Language);
                Console.WriteLine(Localization.T("msg.language_applied", SettingsService.Current.Language));
            }

        }

    }

}
