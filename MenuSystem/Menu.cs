using System;
using System.Collections.Generic;

namespace MenuSystem
{
    public enum MenuLevel
    {
        Level0,
        Level1,
        Level2Plus,
        LevelGame
    }

    public class Menu
    {
        private string? _menuCommandExitProgram;
        private readonly string? _menuExitProgramTitle;
        private string _menuCommandReturnToPreviousMenu;
        private readonly string _menuReturnToPreviousMenuTitle;
        private string? _menuCommandReturnToMainMenu;
        private readonly string? _menuReturnToMainMenuTitle;

        private bool closeMenu;
        
        private readonly string _defaultExitCommand = "ee.itcollege.ee.marko.gordejev.exit";
        private readonly string _defaultReturnMainMenuCommand = "ee.itcollege.ee.marko.gordejev.returnMenu";
        private readonly string _defaultReturnPreviousCommand = "ee.itcollege.ee.marko.gordejev.returnPrevious";
        
        private Dictionary<string, MenuItem> MenuItems { get; set; } = new Dictionary<string, MenuItem>();

        private readonly MenuLevel _menuLevel;

        public Menu(MenuLevel level, string menuCommandExitProgram = "X", string menuExitProgramTitle = "EXIT PROGRAM",
            string menuCommandReturnToPreviousMenu = "R", string menuReturnToPreviousMenuTitle = "RETURN TO PREVIOUS MENU", 
            string menuCommandReturnToMainMenu = "M", string menuReturnToMainTitle = "RETURN TO MAIN MENU")
        {
            _menuLevel = level;

            if (_menuLevel == MenuLevel.LevelGame)
            {
                _menuCommandExitProgram = null;
                _menuExitProgramTitle = null;
                _menuCommandReturnToPreviousMenu = menuCommandReturnToPreviousMenu.ToUpper();
                _menuReturnToPreviousMenuTitle = menuReturnToPreviousMenuTitle.ToUpper();
                _menuCommandReturnToMainMenu = null;
                _menuReturnToMainMenuTitle = null;
            }
            else
            {
                _menuCommandExitProgram = menuCommandExitProgram.ToUpper();
                _menuExitProgramTitle = menuExitProgramTitle.ToUpper();
                _menuCommandReturnToPreviousMenu = menuCommandReturnToPreviousMenu.ToUpper();
                _menuReturnToPreviousMenuTitle = menuReturnToPreviousMenuTitle.ToUpper();
                _menuCommandReturnToMainMenu = menuCommandReturnToMainMenu.ToUpper();
                _menuReturnToMainMenuTitle = menuReturnToMainTitle.ToUpper();
            }
        }
        
        public void CloseMenu()
        {
            closeMenu = true;
        }

        public string RunMenu()
        {
            var userChoice = "";

            AddDefaultMainMenuItems();
            
            do
            {
                if (closeMenu)
                {
                    break;
                }
                
                Console.WriteLine("===========================");

                foreach (var menuItem in MenuItems)
                {
                    Console.WriteLine(menuItem.Value);
                }

                Console.WriteLine("===========================");

                Console.Write(">");

                userChoice = Console.ReadLine()?.ToUpper().Trim() ?? "";

                if (userChoice == _menuCommandExitProgram)
                {
                    userChoice = _defaultExitCommand;
                }
                else if (userChoice == _menuCommandReturnToMainMenu)
                {
                    userChoice = _defaultReturnMainMenuCommand;
                }
                else if (userChoice == _menuCommandReturnToPreviousMenu)
                {
                    userChoice = _defaultReturnPreviousCommand;
                }
                
                if (userChoice == _defaultReturnMainMenuCommand &&
                    _menuLevel == MenuLevel.Level0 ||
                    userChoice == _defaultReturnPreviousCommand &&
                    (_menuLevel == MenuLevel.Level0 || _menuLevel == MenuLevel.Level1))
                {
                    userChoice = "";
                }

                if (userChoice != _defaultExitCommand && userChoice != _defaultReturnMainMenuCommand &&
                    userChoice != _defaultReturnPreviousCommand)
                {
                    if (MenuItems.TryGetValue(userChoice, out var userMenuItem))
                    {
                        userChoice = userMenuItem.MethodToExecute!();
                    }
                    else
                    {
                        Console.WriteLine("Unknown menu option selected!");
                    }
                }

                if (userChoice == _defaultReturnMainMenuCommand)
                {
                    userChoice = _menuLevel != MenuLevel.Level0 ? _defaultReturnMainMenuCommand : "";
                }

                if (userChoice == _defaultReturnPreviousCommand && _menuLevel == MenuLevel.Level1)
                {
                    userChoice = "";
                }

                if (userChoice == _defaultExitCommand && _menuLevel == MenuLevel.Level0)
                {
                    Console.WriteLine("Closing down...");
                }
                
            } while (userChoice != _defaultExitCommand &&
                     userChoice != _defaultReturnMainMenuCommand &&
                     userChoice != _defaultReturnPreviousCommand);

            return userChoice;
        }

        private void AddDefaultMainMenuItems()
        {
            if (_menuLevel == MenuLevel.LevelGame)
            {
                _menuCommandReturnToPreviousMenu = _menuCommandReturnToPreviousMenu.ToUpper();
            }
            else
            {
                _menuCommandReturnToPreviousMenu = _menuCommandReturnToPreviousMenu.ToUpper();
                _menuCommandExitProgram = _menuCommandExitProgram!.ToUpper();
                _menuCommandReturnToMainMenu = _menuCommandReturnToMainMenu!.ToUpper();
            }

            if (_menuLevel != MenuLevel.LevelGame)
            {
                if (_menuLevel == MenuLevel.Level1)
                {
                    if (!MenuItems.ContainsKey(_menuCommandReturnToMainMenu!))
                    {
                        MenuItems.Add(_menuCommandReturnToMainMenu!, new MenuItem(_menuReturnToMainMenuTitle!, _menuCommandReturnToMainMenu!, null));
                    }
                }
                if (_menuLevel == MenuLevel.Level2Plus)
                {
                    if (!MenuItems.ContainsKey(_menuCommandReturnToPreviousMenu))
                    {
                        MenuItems.Add(_menuCommandReturnToPreviousMenu, new MenuItem(_menuReturnToPreviousMenuTitle, _menuCommandReturnToPreviousMenu, null));
                    }
                    if (!MenuItems.ContainsKey(_menuCommandReturnToMainMenu!)) {
                        MenuItems.Add(_menuCommandReturnToMainMenu!, new MenuItem(_menuReturnToMainMenuTitle!, _menuCommandReturnToMainMenu!, null));
                    }
                }
                if (!MenuItems.ContainsKey(_menuCommandExitProgram!))
                {
                    MenuItems.Add(_menuCommandExitProgram!, new MenuItem(_menuExitProgramTitle!, _menuCommandExitProgram!, null));
                }
            }
            else
            {
                if (!MenuItems.ContainsKey(_menuCommandReturnToPreviousMenu))
                {
                    MenuItems.Add(_menuCommandReturnToPreviousMenu, new MenuItem(_menuReturnToPreviousMenuTitle, _menuCommandReturnToPreviousMenu, null));
                }
            }
        }

        public void AddMenuItem(MenuItem item)
        {
            if (item.UserChoice == _menuCommandExitProgram)
            {
                throw new ArgumentException(
                    $"A MenuItem with the UserChoice {item.UserChoice} is already present in that menu level!");
            }

            if (item.UserChoice == _menuCommandReturnToMainMenu &&
                (_menuLevel == MenuLevel.Level1 || _menuLevel == MenuLevel.Level2Plus))
            {
                throw new ArgumentException(
                    $"A MenuItem with the UserChoice {item.UserChoice} is already present in that menu level!");
            }

            if ((item.UserChoice == _menuCommandReturnToPreviousMenu || item.UserChoice == _menuCommandReturnToPreviousMenu) &&
                _menuLevel == MenuLevel.Level2Plus)
            {
                throw new ArgumentException(
                    $"A MenuItem with the UserChoice {item.UserChoice} is already present in that menu level!");
            }

            MenuItems.Add(item.UserChoice, item);
        }
    }
}