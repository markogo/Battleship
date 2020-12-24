using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ConsoleUI;
using DAL;
using Domain;
using Domain.Enums;
using GameEngine;
using MenuSystem;
using Microsoft.EntityFrameworkCore;

namespace ConsoleApp
{
    class Program
    {
        private static GameOption _currentGameOptions = new GameOption();
        private static ICollection<GameOptionShip> _currentGameOptionShips = new List<GameOptionShip>();
        
        private static readonly DbContextOptions<AppDbContext> _dbOptions = 
            new DbContextOptionsBuilder<AppDbContext>().UseSqlite(
                    $"DataSource={Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../Battleship.db"))}")
                .Options;
        
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            using (var db = new AppDbContext(_dbOptions))
            {
                //Console.WriteLine("Deleting database...");
                //DAL.Helpers.DataInitializers.DeleteDatabase(db);
                Console.WriteLine("Migrating database...");
                DAL.Helpers.DataInitializers.MigrateDatabase(db);

                if (db.GameOptions.ToList().Count == 0)
                {
                    Console.WriteLine("Adding data database...");
                    
                    // Adding default GameOptions
                    db.GameOptions.Add(_currentGameOptions);
                    db.SaveChanges();
                    
                    // Adding default Ships
                    db.Ships.Add(new Ship() {Size = 5, Name = "Carrier"});
                    db.Ships.Add(new Ship() {Size = 4, Name = "Battleship"});
                    db.Ships.Add(new Ship() {Size = 3, Name = "Cruiser"});
                    db.Ships.Add(new Ship() {Size = 2, Name = "Submarine"});
                    db.Ships.Add(new Ship() {Size = 1, Name = "Destroyer"});
                    db.SaveChanges();
                    
                    // Adding default GameOptionShips
                    foreach (var ship in db.Ships)
                    {
                        var gameOptionShip = new GameOptionShip()
                        {
                            Amount = 1,
                            ShipId = ship.ShipId,
                            GameOptionId = _currentGameOptions.GameOptionId
                        };
                        db.GameOptionShips.Add(gameOptionShip);
                        _currentGameOptionShips.Add(gameOptionShip);
                    }
                    db.SaveChanges();
                }
                else
                {
                    _currentGameOptions = db.GameOptions.ToList().First();
                    
                    var latestGameOptionShips =
                        db.GameOptionShips.Where(gos => gos.GameOptionId == _currentGameOptions!.GameOptionId);
                    
                    foreach (var gameOptionShip in latestGameOptionShips)
                    {
                        _currentGameOptionShips.Add(gameOptionShip);
                    }
                }
            }

            Console.ForegroundColor = ConsoleColor.Red;
            
            Console.WriteLine("=========> BATTLESHIPS MAGORD <============");

            var menuA = new Menu(MenuLevel.Level1);
            menuA.AddMenuItem(new MenuItem("New game", "N", StartNewGame));
            menuA.AddMenuItem(new MenuItem("Load game", "L", LoadGameAction));
            
            var menu = new Menu(MenuLevel.Level0);
            menu.AddMenuItem(new MenuItem("Play game", "P", menuA.RunMenu));
            menu.AddMenuItem(new MenuItem("Create new game options", "S", SaveGameSettings));
            menu.AddMenuItem(new MenuItem("Load game options", "L", LoadGameSettings));

            menu.RunMenu();
        }

        private static string LoadGameSettings()
        {
            using (var db = new AppDbContext(_dbOptions))
            {
                if (db.GameOptions.Any())
                {
                    Dictionary<int, GameOption> choices = new Dictionary<int, GameOption>();
                    bool settingsLoaded = false;
                    do
                    {
                        var counter = 1;
                        Console.WriteLine("Please choose a game setting to load:");
                        foreach (var gameOption in db.GameOptions)
                        {
                            Console.WriteLine($"{counter}) {gameOption.Name}");
                            choices[counter] = gameOption;
                            counter++;
                        }

                        Console.Write(">");
                        var userChoice = Console.ReadLine();

                        if (int.TryParse(userChoice, out var userSelection))
                        {
                            if (choices.ContainsKey(userSelection))
                            {
                                Console.WriteLine("Game option loaded!");
                                _currentGameOptions = choices[userSelection];
                                _currentGameOptionShips.Clear();
                                foreach (var gameOptionShip in db.GameOptionShips)
                                {
                                    if (gameOptionShip.GameOptionId == _currentGameOptions.GameOptionId)
                                    {
                                        _currentGameOptionShips.Add(gameOptionShip);
                                    }
                                }
                                settingsLoaded = true;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Please enter a valid option!");
                        }
                    } while (!settingsLoaded);
                }
                else
                {
                    Console.WriteLine("Unknown error!");
                    return "";
                }
            }

            return "";
        }

        private static string SaveGameSettings()
        {
            const int maxBoardWidth = 26;
            const int minBoardWidth = 10;
            
            const int maxBoardHeight = 26;
            const int minBoardHeight = 10;
            
            Console.Clear();
             
            int boardWidth;
            int boardHeight;
            var optionName = "";
            bool gotValue;
            var canShipsTouch = CanShipsTouchEnum.No;
            var nextMoveAfterHit = NextMoveAfterHitEnum.OtherPlayerTurn;
             
            do
            {
                Console.WriteLine("Choose game board height:");
                Console.Write(">");
                var userHeight = Console.ReadLine();

                if (!int.TryParse(userHeight, out boardHeight))
                {
                    Console.WriteLine($"{userHeight} is not a number! Please try again!");
                    boardHeight = -1;
                }
                else if (boardHeight < minBoardHeight)
                {
                    Console.WriteLine($"Minimum board height is {minBoardHeight}! Please try again!");
                    boardHeight = -1;
                }
                else if (boardHeight > maxBoardHeight)
                {
                    Console.WriteLine($"Maximum board height is {maxBoardHeight}! Please try again!");
                    boardHeight = -1;
                }
            } while (boardHeight < 0);
             
            do
            {
                Console.WriteLine("Choose game board width:");
                Console.Write(">");
                var userWidth = Console.ReadLine();
                if (!int.TryParse(userWidth, out boardWidth))
                {
                    Console.WriteLine($"{userWidth} is not a number! Please try again!");
                    boardWidth = -1;
                }
                else if (boardWidth > maxBoardWidth)
                {
                    Console.WriteLine($"Maximum board width is {maxBoardWidth}! Please try again!");
                    boardWidth = -1;
                }
                else if (boardWidth < minBoardWidth)
                {
                    Console.WriteLine($"Minimum board width is {minBoardWidth}! Please try again!");
                    boardWidth = -1;
                }
            } while (boardWidth < 0);

            do
            {
                Console.WriteLine("Choose if ships can touch on the board:");
                Console.WriteLine($"1) {CanShipsTouchEnum.No}");
                Console.WriteLine($"2) {CanShipsTouchEnum.Corner}");
                Console.WriteLine($"3) {CanShipsTouchEnum.Yes}");
                Console.Write(">");
                var userOption = Console.ReadLine();
                if (!int.TryParse(userOption, out var userChoice))
                {
                    Console.WriteLine($"{userOption} is not a number! Please try again!");
                    gotValue = false;
                }
                else if (userChoice > 3 || userChoice < 1)
                {
                    Console.WriteLine("Please enter a valid option!");
                    gotValue = false;
                }
                else
                {
                    canShipsTouch = userChoice switch
                    {
                        1 => CanShipsTouchEnum.No,
                        2 => CanShipsTouchEnum.Corner,
                        _ => CanShipsTouchEnum.Yes
                    };
                    gotValue = true;
                }
            } while (!gotValue);

            gotValue = false;
            
            do
            {
                Console.WriteLine("Choose who's turn it is after a successful hit:");
                Console.WriteLine("1) Other player's turn");
                Console.WriteLine("2) Same player's turn");
                Console.Write(">");
                var userOption = Console.ReadLine();
                var userChoice = -1;
                if (!int.TryParse(userOption, out userChoice))
                {
                    Console.WriteLine($"{userOption} is not a number! Please try again!");
                    gotValue = false;
                }
                else if (userChoice > 2 || userChoice < 1)
                {
                    Console.WriteLine($"Please enter a valid option!");
                    gotValue = false;
                }
                else
                {
                    if (userChoice == 1)
                    {
                        nextMoveAfterHit = NextMoveAfterHitEnum.OtherPlayerTurn;
                    }
                    else
                    {
                        nextMoveAfterHit = NextMoveAfterHitEnum.SamePlayerTurn;
                    }
                    gotValue = true;
                }
            } while (!gotValue);
            
            var newGameOption = new GameOption
            {
                BoardHeight = boardHeight,
                BoardWidth = boardWidth,
                Name = optionName,
                CanShipsTouchEnum = canShipsTouch,
                NextMoveAfterHitEnum = nextMoveAfterHit
            };

            _currentGameOptions = newGameOption;

            int insertedGameOptionId;
            
            using (var db = new AppDbContext(_dbOptions))
            {
                db.Add(newGameOption);
                db.SaveChanges();
                insertedGameOptionId = newGameOption.GameOptionId;
            }
            
            using var database = new AppDbContext(_dbOptions);
            
            _currentGameOptionShips.Clear();

            foreach (var ship in database.Ships)
            {
                const int defaultShipAmount = 1;

                var gameOptionBoatAdded = false;
                
                do
                {
                    Console.WriteLine($"Please enter the amount of {ship.Name.ToUpper()}S({ship.Size} x 1) you want in the game [DEFAULT: {defaultShipAmount}]");
                    Console.Write(">");
                    var userInput = Console.ReadLine();

                    if (userInput!.Trim().Length < 1)
                    {
                        var customGameOptionShip = new GameOptionShip()
                        {
                            Amount = defaultShipAmount,
                            ShipId = ship.ShipId,
                            GameOptionId = insertedGameOptionId
                        };
                        database.Add(customGameOptionShip);
                        database.SaveChangesAsync();
                        _currentGameOptionShips.Add(customGameOptionShip);
                        gameOptionBoatAdded = !gameOptionBoatAdded;
                    }
                    else
                    {
                        if (int.TryParse(userInput, out var userChoice))
                        {
                            var customGameOptionShip = new GameOptionShip()
                            {
                                Amount = userChoice,
                                ShipId = ship.ShipId,
                                GameOptionId = insertedGameOptionId
                            };
                            database.Add(customGameOptionShip);
                            database.SaveChangesAsync();
                            _currentGameOptionShips.Add(customGameOptionShip);
                            gameOptionBoatAdded = !gameOptionBoatAdded;
                        }
                    }
                } while (gameOptionBoatAdded == false);
            }

            do
            {
                Console.WriteLine("Please enter a name for the new settings");
                Console.Write(">");
                var userInput = Console.ReadLine();
                if (userInput!.Length > 0)
                {
                    optionName = userInput;
                    var gameOption = database.GameOptions.Find(insertedGameOptionId);
                    gameOption.Name = userInput;
                    database.SaveChanges();
                }
            } while (optionName == "");

            Console.Clear();
            return "";
        }

        private static bool CheckConsoleWidthSizing(int boardWidth)
        {
            return !((double) Console.LargestWindowWidth / boardWidth < 9);
        }
        
        private static bool CheckConsoleHeightSizing(int boardHeight)
        {
            return !((double) Console.LargestWindowHeight / boardHeight < 2.35);
        }

        private static string StartNewGame()
        {
            Game game = new Game();
            GameLogic gameLogic = new GameLogic(_currentGameOptions);
            if (CheckConsoleHeightSizing(_currentGameOptions.BoardHeight) && CheckConsoleWidthSizing(_currentGameOptions.BoardWidth))
            {
                Player player1 = new Player {PlayerTurn = true, PlayerTypeEnum = PlayerTypeEnum.Human};
                Player player2 = new Player {PlayerTurn = false, PlayerTypeEnum = PlayerTypeEnum.Human};
                SetPlayerNames(player1, player2);
                game.Player1 = player1;
                game.Player2 = player2;
                gameLogic.Player1 = player1;
                gameLogic.Player2 = player2;
                game.GameOptionId = _currentGameOptions.GameOptionId;
                using var db = new AppDbContext(_dbOptions);
                db.Games.Add(game);
                db.SaveChanges();
                player1.GameId = game.GameId;
                player2.GameId = game.GameId;
                db.SaveChanges();
                gameLogic.Game = game;
                game.GamePhase = GamePhaseEnum.PlacingShips;
                db.SaveChanges();
                HumanVHumanGame(gameLogic);
            }
            else
            {
                Console.WriteLine("Please resize your console or change board dimensions" + 
                                  $" to start the game with a board width of {gameLogic.BoardWidth} and board height of {gameLogic.BoardHeight}!");
            }
            return "Starting new game!";
        }

        private static void SetPlayerNames(Player player1, Player player2)
        {
            Console.WriteLine("Please enter Player 1 name [DEFAULT: Player1]:");
            Console.Write(">");
            var player1Name = Console.ReadLine();
            player1.Name = player1Name!.Trim().Length < 1 ? "Player1" : player1Name.Trim();
            
            Console.WriteLine("Please enter Player 2 name [DEFAULT: Player2]:");
            Console.Write(">");
            var player2Name = Console.ReadLine();
            player2.Name = player2Name!.Trim().Length < 1 ? "Player2" : player2Name.Trim();
        }

        private static string GetWhosTurn(GameLogic game)
        {
            return game.IsPlayer1Turn() ? $"{game.Player1.Name.ToUpper()}'S TURN!" : $"{game.Player2.Name.ToUpper()}'S TURN!";
        }

        private static Tuple<int, int>[] GetShipCoordinates(GameLogic game, GameShip ship)
        {
            var regex = new Regex(@"^[A-Za-z]{1}(,[0-9]+)-[A-Za-z]{1}(,[0-9]+$)");
            var gotCoordinates = false;
            var invalidPlacement = false;
            
            do
            {
                PrintSinglePlayerBoardState(game);
                Console.WriteLine("");
                Console.WriteLine(GetWhosTurn(game));
                Console.WriteLine($"CHOOSE COORDINATES FOR {ship.Name}({ship.Size}) (EXAMPLE -> A,1-B,1)");
                if (invalidPlacement)
                {
                    Console.WriteLine("INVALID COORDINATES ENTERED! PLEASE TRY AGAIN");
                }
                Console.WriteLine("======================================================");

                var userInput = Console.ReadLine();

                if (regex.IsMatch(userInput!))
                {
                    var result = game.UserInputToCoordinates(userInput!);
                    if (result.conversionSuccessful)
                    {
                        gotCoordinates = true;
                        return result.coordinates;
                    }
                }

                invalidPlacement = true;
            } while (!gotCoordinates);
            
            return null!;
        }
        
        private static void PlaceShip(GameLogic game, GameShip ship)
        {
            bool shipPlacementSuccessful;
            do
            {
                var shipCoordinates = GetShipCoordinates(game, ship);
                var shipStartCoordinates = shipCoordinates[0];
                var shipEndCoordinates = shipCoordinates[1];

                shipPlacementSuccessful = game.PlaceShipOnBoard(ship, shipStartCoordinates.ToValueTuple(),
                    shipEndCoordinates.ToValueTuple());

            } while (!shipPlacementSuccessful);
        }

        private static void PlaceShipsOnBoard(GameLogic game)
        {
            Console.Clear();
            PrintSinglePlayerBoardState(game);
            Console.WriteLine("");
            Console.WriteLine(GetWhosTurn(game));
            
            var player1PlacingShipsMenu = new Menu(MenuLevel.LevelGame, menuCommandReturnToPreviousMenu:"M", menuReturnToPreviousMenuTitle:"Manual ship placement");
            
            player1PlacingShipsMenu.AddMenuItem(new MenuItem(
                "Random ship placement",
                "R",
                () => game.AutomatePlacingShipsOnBoard(_currentGameOptionShips, player1PlacingShipsMenu)));
           
            using var database = new AppDbContext(_dbOptions);
            IList<Ship> shipList = database.Ships.ToList();

            player1PlacingShipsMenu.RunMenu();
            
            if (!game.ShipsPlacedOnBoard)
            {
                foreach (var ship in shipList)
                {
                    foreach (var gameOptionShip in _currentGameOptionShips)
                    {
                        if (ship.ShipId == gameOptionShip.ShipId)
                        {
                            var amountOfShip = gameOptionShip.Amount;

                            foreach (var value in Enumerable.Range(1, amountOfShip))
                            {
                                GameShip gameShipOnBoard = new GameShip
                                {
                                    Size = ship.Size,
                                    Name = ship.Name,
                                    IsSunken = false,
                                    PlayerId = game.IsPlayer1Turn() ? game.Player1.PlayerId : game.Player2.PlayerId
                                };
                                database.GameShips.Add(gameShipOnBoard);
                                database.SaveChanges();
                                PlaceShip(game, gameShipOnBoard);
                            }
                        }
                    }
                }
            }
            else
            {
                PrintSinglePlayerBoardState(game);
                Console.WriteLine("");
                Console.WriteLine("SHIPS PLACED!");
                System.Threading.Thread.Sleep(5000);
            }
            
            if (game.Player2.PlayerTypeEnum == PlayerTypeEnum.Human)
            {
                game.Player1.PlayerTurn = !game.Player1.PlayerTurn;
                game.Player2.PlayerTurn = !game.Player2.PlayerTurn;
                
                database.Entry(game.Player1).State = EntityState.Modified;
                database.Entry(game.Player2).State = EntityState.Modified;
                database.SaveChanges();
                
                Console.Clear();
                Console.WriteLine("GIVE GAME CONTROLS TO OTHER PLAYER");
                System.Threading.Thread.Sleep(5000);

                game.ShipsPlacedOnBoard = false;
                PrintSinglePlayerBoardState(game);
                Console.WriteLine("");
                Console.WriteLine(GetWhosTurn(game));
                
                var player2PlacingShipsMenu = new Menu(MenuLevel.LevelGame, menuCommandReturnToPreviousMenu:"M", menuReturnToPreviousMenuTitle:"Manual ship placement");
                
                player2PlacingShipsMenu.AddMenuItem(new MenuItem(
                    "Random ship placement",
                    "R",
                    () => game.AutomatePlacingShipsOnBoard(_currentGameOptionShips, player2PlacingShipsMenu)));
               
                player2PlacingShipsMenu.RunMenu();

                if (!game.ShipsPlacedOnBoard)
                {
                    foreach (var ship in shipList)
                    {
                        foreach (var gameOptionShip in _currentGameOptionShips)
                        {
                            if (ship.ShipId == gameOptionShip.ShipId)
                            {
                                var amountOfShip = gameOptionShip.Amount;

                                foreach (var value in Enumerable.Range(1, amountOfShip))
                                {
                                    GameShip gameShipOnBoard = new GameShip
                                    {
                                        Size = ship.Size,
                                        Name = ship.Name,
                                        IsSunken = false,
                                        PlayerId = game.IsPlayer1Turn() ? game.Player1.PlayerId : game.Player2.PlayerId
                                    };
                                    database.GameShips.Add(gameShipOnBoard);
                                    database.SaveChanges();
                                    PlaceShip(game, gameShipOnBoard);
                                }
                            }
                        }
                    }
                }
                else
                {
                    PrintSinglePlayerBoardState(game);
                    Console.WriteLine("");
                    Console.WriteLine("SHIPS PLACED!");
                    System.Threading.Thread.Sleep(5000);
                }
                
                game.Player1.PlayerTurn = !game.Player1.PlayerTurn;
                game.Player2.PlayerTurn = !game.Player2.PlayerTurn;
                database.Entry(game.Player1).State = EntityState.Modified;
                database.Entry(game.Player2).State = EntityState.Modified;
                database.SaveChanges();
                Console.Clear();
                Console.WriteLine("GIVE GAME CONTROLS TO OTHER PLAYER");
                System.Threading.Thread.Sleep(5000);
            }
            else
            {
                game.AutomatePlacingShipsOnBoard(_currentGameOptionShips, null);
            }
        }

        private static string HumanVHumanGame(GameLogic game)
        {
            if (game.Game.GamePhase == GamePhaseEnum.PlacingShips)
            {
                Console.Clear();
                Console.WriteLine("PHASE 1 - PLACE YOUR SHIPS!");
                System.Threading.Thread.Sleep(3000);
                PlaceShipsOnBoard(game);
                Console.Clear();
                Console.WriteLine("PHASE 2 - START BOMBING!");
                System.Threading.Thread.Sleep(3000);
                using var db = new AppDbContext(_dbOptions);
                game.Game.GamePhase = GamePhaseEnum.Bombing;
                db.Entry(game.Game).State = EntityState.Modified;
                db.SaveChanges();
            }
            
            if (game.Game.GamePhase == GamePhaseEnum.GameOver)
            {
                return "";
            }

            Console.Clear();
            PrintCurrentStateBoards(game);
            Console.WriteLine("");
            Console.WriteLine(GetWhosTurn(game));
            
            var gameMenu = new Menu(MenuLevel.LevelGame, menuCommandReturnToPreviousMenu:"Q", menuReturnToPreviousMenuTitle:"Quit game");
            
            gameMenu.AddMenuItem(new MenuItem(
                "Make a move",
                "w",
                () => MakeMove(game, gameMenu)));
            gameMenu.AddMenuItem(new MenuItem(
                "Save game",
                "s",
                () => SaveGameAction(game)));
            gameMenu.AddMenuItem(new MenuItem(
                "Undo last move",
                "u",
                () => UndoMove(game)));
            var userChoice = gameMenu.RunMenu();

            return userChoice;
        }

        private static string UndoMove(GameLogic game)
        {
            var undoSuccessful = game.UndoLastMove();

            if (!undoSuccessful.Item1)
            {
                PrintCurrentStateBoards(game);
                Console.WriteLine("");
                Console.WriteLine(GetWhosTurn(game));
                Console.WriteLine("NO MOVES MADE YET");
                return "";
            }

            if (undoSuccessful.Item2)
            {
                Console.Clear();
                Console.WriteLine("UNDO SUCCESSFUL! GIVE CONTROL BACK TO OTHER PLAYER!");
                System.Threading.Thread.Sleep(5000);
            }
            else
            {
                Console.Clear();
                Console.WriteLine("UNDO SUCCESSFUL!");
                System.Threading.Thread.Sleep(2000);
            }

            PrintCurrentStateBoards(game);
            Console.WriteLine("");
            Console.WriteLine(GetWhosTurn(game));
            
            return "";
        }

        private static string MakeMove(GameLogic game, Menu gameMenu)
        {
            PlayerBoardState playerBoardState = new PlayerBoardState
            {
                PlayerId = (game.Player1.PlayerTurn ? game.Player2 : game.Player1!).PlayerId,
                CreatedAt = DateTime.Now,
                BoardState = game.GetJsonBoardState(game.Player1.PlayerTurn ? game.Player2 : game.Player1)
            };
            
            var (x, y) = GetMoveCoordinates(game);
            var (moveSuccessful, _) = game.MakeAMove(x, y, out string hitOrMissMessage);
            
            if (!moveSuccessful)
            {
                PrintCurrentStateBoards(game);
                Console.WriteLine("");
                Console.WriteLine(GetWhosTurn(game));
                Console.WriteLine("You have already moved there!");
                return "";
            }

            using var db = new AppDbContext(_dbOptions);

            db.PlayerBoardStates.Add(playerBoardState);
            db.SaveChanges();

            GameSave gameState = new GameSave
            {
                CreatedByUser = false,
                SerializedGame = game.GetSerializedGameState(),
                GameId = game.Game.GameId
            };

            db.GameSaves.Add(gameState);
            db.SaveChanges();
            
            PrintOldStateBoards(game, hitOrMissMessage);

            var (gameIsOver, winner) = game.CheckGameWin();

            if (gameIsOver)
            {
                if (game.NextMoveAfterHit == NextMoveAfterHitEnum.OtherPlayerTurn)
                {
                    game.Player1.PlayerTurn = !game.Player1.PlayerTurn;
                    game.Player2.PlayerTurn = !game.Player2.PlayerTurn;
                }
                PrintCurrentStateBoards(game);
                game.Game.GamePhase = GamePhaseEnum.GameOver;
                db.Entry(game.Game).State = EntityState.Modified;
                db.SaveChanges();
                Console.WriteLine("");
                Console.WriteLine("HIT! AND SUNKEN!");
                Console.WriteLine($"GAME OVER! THE WINNER IS {winner.Name.ToUpper()}");
                System.Threading.Thread.Sleep(7000);
                Console.Clear();
                gameMenu.CloseMenu();
                return "";
            }

            if (_currentGameOptions.NextMoveAfterHitEnum == NextMoveAfterHitEnum.SamePlayerTurn &&
                (hitOrMissMessage == "HIT!" || hitOrMissMessage == "HIT! AND SUNKEN!"))
            {
                Console.Clear();
                PrintCurrentStateBoards(game);
                Console.WriteLine("");
                Console.WriteLine(hitOrMissMessage);
            }
            else
            {
                NotifyScreenChange();
                PrintCurrentStateBoards(game);
            }

            Console.WriteLine("");
            Console.WriteLine(GetWhosTurn(game));
            
            return "";
        }

        private static void NotifyScreenChange()
        {
            System.Threading.Thread.Sleep(3000);
            Console.Clear();
            Console.WriteLine("PLEASE GIVE GAME CONTROL TO OTHER PLAYER!");;
            System.Threading.Thread.Sleep(3000);
        }
        
        private static (int x, int y) GetMoveCoordinates(GameLogic game)
        {
            var regex = new Regex(@"^[A-Za-z]{1}(,[0-9]+$)");
            var moveMade = false;

            do
            {
                PrintCurrentStateBoards(game);
                Console.WriteLine("");
                Console.WriteLine(GetWhosTurn(game));
                Console.WriteLine("==============================================================");
                Console.WriteLine("Please enter a correct position on the board (EXAMPLE -> A,1) ");

                var userInput = Console.ReadLine();

                if (regex.IsMatch(userInput!))
                {
                    var userValue = userInput!.Split(',');
                    var x = GameLogic.LetterToNumber(userInput![0]);
                    var y = int.Parse(userValue[1]);

                    if (x <= game.BoardWidth && y <= game.BoardHeight)
                    {
                        moveMade = true;
                        return (x - 1, y - 1);
                    }
                }
            } while (!moveMade);
            
            return (0,0);
        }

        private static string LoadGameAction()
        {
            Dictionary<int, GameSave> choices = new Dictionary<int, GameSave>();
            const string exitCommand = "X";
            var gameLoaded = false;

            using var db = new AppDbContext(_dbOptions);
            
            do
            {
                if (!db.GameSaves.Any(x => x.CreatedByUser))
                {
                    Console.WriteLine("No save files found!");
                }
                else
                {
                    var counter = 1;
                    foreach (var save in db.GameSaves)
                    {
                        if (save.CreatedByUser)
                        {
                            Console.WriteLine($"{counter} - {save.Name}");
                            choices[counter] = save;
                            counter++;
                        }
                    }
                }

                Console.WriteLine("=======================================");
                Console.WriteLine($"Please choose a correct number of a load game or type {exitCommand} to go back!");

                var fileNo = Console.ReadLine();

                if (fileNo == exitCommand || fileNo == exitCommand.ToLower())
                {
                    return "";
                }

                if (int.TryParse(fileNo, out var loadFile))
                {
                    if (choices.ContainsKey(loadFile))
                    {
                        GameSave saveFile = db.GameSaves.Find(choices[loadFile].GameSaveId);
                        
                        Game loadedGame = db.Games.Find(saveFile.GameId);
                        GameOption loadedGameOptions = db.GameOptions.Find(loadedGame.GameOptionId);
                        _currentGameOptions = loadedGameOptions;
                        _currentGameOptionShips.Clear();
                        foreach (var gameOptionShip in db.GameOptionShips)
                        {
                            if (gameOptionShip.GameOptionId == _currentGameOptions.GameOptionId)
                            {
                                _currentGameOptionShips.Add(gameOptionShip);
                            }
                        }

                        GameLogic loadedGameLogic = new GameLogic(loadedGameOptions)
                        {
                            Player1 = db.Players.Find(loadedGame.Player1Id), Player2 = db.Players.Find(loadedGame.Player2Id), Game = loadedGame
                        };
                        
                        loadedGameLogic.SetGameStateFromJsonString(saveFile.SerializedGame!);

                        if (!CheckConsoleHeightSizing(loadedGameLogic.BoardHeight) || !CheckConsoleWidthSizing(loadedGameLogic.BoardWidth))
                        {
                            Console.WriteLine("Please resize your console and try again!");
                            return "";
                        }
                        
                        HumanVHumanGame(loadedGameLogic);
                        
                        gameLoaded = true;
                        return "";
                    }
                }
            } while (!gameLoaded);

            return "";
        }

        private static string SaveGameAction(GameLogic game)
        {
            GameSave gameSave = new GameSave();
            var defaultName = "SaveFile_" + DateTime.Now.ToString("dd-MM-yyyy_HH:mm:ss");
            Console.Write($"SaveFile name (Default -> {defaultName}):");
            var fileName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = defaultName;
            }

            gameSave.Name = fileName;
            gameSave.CreatedByUser = true;
            gameSave.GameId = game.Game.GameId;
            gameSave.SerializedGame = game.GetSerializedGameState();

            using (var db = new AppDbContext(_dbOptions))
            {
                db.GameSaves.Add(gameSave);
                db.SaveChanges();
            }

            PrintCurrentStateBoards(game);
            Console.WriteLine("");
            Console.WriteLine(GetWhosTurn(game));
            
            Console.WriteLine("Game saved!");
            return "";
        }

        private static void PrintCurrentStateBoards(GameLogic game)
        {
            Console.Clear();
            var currentCursorTop = Console.CursorTop;
            var currentCursorLeft = Console.CursorLeft;
            GameUI.PrintBoard(game, game.IsPlayer1Turn() ? game.GetPlayer1Board() : game.GetPlayer2Board(), true, currentCursorLeft, currentCursorTop, false);
            GameUI.PrintBoard(game, game.IsPlayer1Turn() ? game.GetPlayer2Board() : game.GetPlayer1Board(), false, currentCursorLeft + Console.LargestWindowWidth / 2, currentCursorTop, false);
        }

        private static void PrintOldStateBoards(GameLogic game, string hitOrMissMessage)
        {
            Console.Clear();
            var currentCursorTop = Console.CursorTop;
            var currentCursorLeft = Console.CursorLeft;

            if (_currentGameOptions.NextMoveAfterHitEnum == NextMoveAfterHitEnum.SamePlayerTurn &&
                hitOrMissMessage == "HIT!")
            {
                GameUI.PrintBoard(game, game.IsPlayer1Turn() ? game.GetPlayer1Board() : game.GetPlayer2Board(), true, currentCursorLeft, currentCursorTop, true);
                GameUI.PrintBoard(game, game.IsPlayer1Turn() ? game.GetPlayer2Board() : game.GetPlayer1Board(), false, currentCursorLeft + Console.LargestWindowWidth / 2, currentCursorTop, true);
                Console.WriteLine("");
                Console.WriteLine(hitOrMissMessage);
            }
            else
            {
                GameUI.PrintBoard(game, game.IsPlayer1Turn() ? game.GetPlayer2Board() : game.GetPlayer1Board(), true, currentCursorLeft, currentCursorTop, true);
                GameUI.PrintBoard(game, game.IsPlayer1Turn() ? game.GetPlayer1Board() : game.GetPlayer2Board(), false, currentCursorLeft + Console.LargestWindowWidth / 2, currentCursorTop, true);
                Console.WriteLine("");
                Console.WriteLine(hitOrMissMessage);
            }
        }

        private static void PrintSinglePlayerBoardState(GameLogic game)
        {
            Console.Clear();
            GameUI.PrintBoard(game, game.IsPlayer1Turn() ? game.GetPlayer1Board() : game.GetPlayer2Board(), true, 0, 0, false, true);
        }
    }
}