﻿using System;
 using System.Collections.Generic;
 using System.IO;
 using System.Linq;
 using System.Text.Json;
 using DAL;
 using Domain;
 using Domain.Enums;
 using MenuSystem;
 using Microsoft.EntityFrameworkCore;

 namespace GameEngine
{
    public class GameLogic
    {
        public CanShipsTouchEnum CanShipsTouch { get; set; }
        public NextMoveAfterHitEnum NextMoveAfterHit { get; set; }
        public Player Player1 { get; set; } = null!;
        public Player Player2 { get; set; } = null!;
        public Game Game { get; set; } = null!;
        public int BoardWidth { get; private set; }
        public int BoardHeight { get; private set; }
        private BoardSquareState[,] P1Board { get; set; } = null!;
        private BoardSquareState[,] P2Board { get; set; } = null!;
        public bool ShipsPlacedOnBoard { get; set; }
        
        private static readonly DbContextOptions<AppDbContext> _dbOptions = 
            new DbContextOptionsBuilder<AppDbContext>().UseSqlite(
                    $"DataSource={Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../Battleship.db"))}")
                .Options;
        
        public GameLogic() {}
        
        public GameLogic(GameOption gameOptions)
        {
            if (gameOptions.BoardHeight < 10 || gameOptions.BoardWidth < 10)
            {
                throw new ArgumentException("Board size has to be at least 10x10!");
            }

            BoardHeight = gameOptions.BoardHeight;
            BoardWidth = gameOptions.BoardWidth;
            
            CanShipsTouch = gameOptions.CanShipsTouchEnum;
            NextMoveAfterHit = gameOptions.NextMoveAfterHitEnum;
            
            P1Board = new BoardSquareState[BoardHeight,BoardWidth];
            P2Board = new BoardSquareState[BoardHeight, BoardWidth];
            
            for (var x = 0; x < BoardWidth; x++)
            {
                for (var y = 0; y < BoardHeight; y++)
                {
                    P1Board[y, x] = new BoardSquareState();
                    P2Board[y, x] = new BoardSquareState();
                }
            }
        }
        
        public (Tuple<int, int>[] coordinates, bool conversionSuccessful) UserInputToCoordinates(string userInput)
        {
            var userValue = userInput!.Split('-');
            var startingPosition = userValue[0].Split(',');
            var endPosition = userValue[1].Split(',');
            (int x, int y) startingCoord = (LetterToNumber(startingPosition[0][0]), int.Parse(startingPosition[1]));
            (int x, int y) endCoord = (LetterToNumber(endPosition[0][0]), int.Parse(endPosition[1]));

            if (startingCoord.x <= BoardWidth && startingCoord.y <= BoardHeight
                                                   && endCoord.x <= BoardWidth && endCoord.y <= BoardHeight)
            {
                return (new[] {startingCoord.ToTuple(), endCoord.ToTuple() }, true);
            }
            return (new Tuple<int, int>[] { }, false);
        }
        
        public BoardSquareState GetBoardSquareState(BoardSquareState[,] board, int x, int y)
        {
            return board[y, x];
        }

        private void SetPlayer1Board(BoardSquareState[,]? cellState)
        {
            if (cellState != null)
            {
                P1Board = cellState;
            }
        }

        public bool IsPlayer1Turn()
        {
            return Player1.PlayerTurn;
        }

        private void SetPlayer2Board(BoardSquareState[,]? cellState)
        {
            if (cellState != null)
            {
                P2Board = cellState;
            }
        }
        
        public BoardSquareState[,] GetPlayer1Board()
        {
            var result = new BoardSquareState[BoardHeight, BoardWidth];
            Array.Copy(P1Board, result, P1Board.Length);
            return result;
        }
        
        public BoardSquareState[,] GetPlayer2Board()
        {
            var result = new BoardSquareState[BoardHeight, BoardWidth];
            Array.Copy(P2Board, result, P2Board.Length);
            return result;
        }

        public BoardSquareState[,] GetActivePlayerBoard()
        {
            return IsPlayer1Turn() ? P1Board : P2Board;
        }
        
        public BoardSquareState[,] GetNonActivePlayerBoard()
        {
            return IsPlayer1Turn() ? P2Board : P1Board;
        }
        
        public static int LetterToNumber(char letter)  
        {  
            var index = char.ToUpper(letter) - 64;  
            return index;  
        }

        public string AutomatePlacingShipsOnBoard(ICollection<GameOptionShip> gameOptionShips, Menu? placingShipsMenu)
        {
            Random random = new Random();
            var randomBoardHeight = random.Next(1, BoardHeight - 1);
            var randomBoardWidth = random.Next(1, BoardWidth - 1);
            var randomShipDirection = random.Next(2) == 1;
            var activePlayerBoard = GetActivePlayerBoard();
            
            using var db = new AppDbContext(_dbOptions);
            
            IList<Ship> shipList = db.Ships.ToList();

            foreach (var ship in shipList)
            {
                foreach (var gameOptionShip in gameOptionShips)
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
                                PlayerId = IsPlayer1Turn() ? Player1.PlayerId : Player2.PlayerId
                            };
                            db.GameShips.Add(gameShipOnBoard);
                            db.SaveChanges();
                            
                            var shipPlaced = false;
                            
                            do
                            {
                                if (CanPlaceShipAtCoordinates((randomBoardWidth, randomBoardHeight), gameShipOnBoard, activePlayerBoard, randomShipDirection))
                                {
                                    // Horizontal
                                    if (randomShipDirection)
                                    {
                                        foreach (var xValue in Enumerable.Range(randomBoardWidth, gameShipOnBoard.Size))
                                        {
                                            activePlayerBoard[randomBoardHeight - 1, xValue - 1].ShipId = gameShipOnBoard.GameShipId;
                                        }
                                    }
                                    // Vertical
                                    else
                                    {
                                        foreach (var yValue in Enumerable.Range(randomBoardHeight, gameShipOnBoard.Size))
                                        {
                                            activePlayerBoard[yValue - 1, randomBoardWidth - 1].ShipId = gameShipOnBoard.GameShipId;
                                        }
                                    }
                                    shipPlaced = true;
                                }
                                randomBoardHeight = random.Next(1, BoardHeight - 1);
                                randomBoardWidth = random.Next(1, BoardWidth - 1);
                                randomShipDirection = random.Next(2) == 1;
                            } while (!shipPlaced);
                        }
                    }
                }
            }

            if (placingShipsMenu != null)
            {
                placingShipsMenu.CloseMenu();
                ShipsPlacedOnBoard = true;
            }
            return "";
        }

        public bool PlaceShipOnBoard(GameShip ship, (int, int) shipStartCoordinates, (int, int) shipEndCoordinates, bool checkOnly = false)
        {
            // SIZE ONE SHIP PLACEMENT
            if (shipStartCoordinates.Item1 == shipEndCoordinates.Item1 &&
                shipStartCoordinates.Item2 == shipEndCoordinates.Item2 && ship.Size == 1)
            {
                if (IsPlayer1Turn())
                {
                    if (CanPlaceShipAtCoordinates(shipStartCoordinates, ship, P1Board))
                    {
                        if (checkOnly)
                        {
                            return true;
                        }
                        P1Board[shipStartCoordinates.Item2 - 1, shipStartCoordinates.Item1 - 1].ShipId = ship.GameShipId;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (CanPlaceShipAtCoordinates(shipStartCoordinates, ship, P2Board))
                    {
                        if (checkOnly)
                        {
                            return true;
                        }
                        P2Board[shipStartCoordinates.Item2 - 1, shipStartCoordinates.Item1 - 1].ShipId = ship.GameShipId;
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }

            // VERTICAL SHIP PLACEMENT
            if (shipStartCoordinates.Item1 == shipEndCoordinates.Item1)
            {
                var verticalLength = Math.Abs(shipStartCoordinates.Item2 - shipEndCoordinates.Item2) + 1;
                if (ship.Size != verticalLength)
                {
                    return false;
                }
                
                if (shipStartCoordinates.Item2 > shipEndCoordinates.Item2)
                {
                    shipStartCoordinates.Item2 = shipEndCoordinates.Item2;
                    shipEndCoordinates.Item2 = shipStartCoordinates.Item2;
                }
                
                if (IsPlayer1Turn())
                {
                    if (CanPlaceShipAtCoordinates(shipStartCoordinates, ship, P1Board, false))
                    {
                        if (checkOnly)
                        {
                            return true;
                        }
                        foreach (var yValue in Enumerable.Range(shipStartCoordinates.Item2, verticalLength))
                        {
                            P1Board[yValue - 1, shipEndCoordinates.Item1 - 1].ShipId = ship.GameShipId;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (CanPlaceShipAtCoordinates(shipStartCoordinates, ship, P2Board, false))
                    {
                        if (checkOnly)
                        {
                            return true;
                        }
                        foreach (var yValue in Enumerable.Range(shipStartCoordinates.Item2, verticalLength))
                        {
                            P2Board[yValue - 1, shipEndCoordinates.Item1 - 1].ShipId = ship.GameShipId;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
            
            // HORIZONTAL SHIP PLACEMENT
            if (shipStartCoordinates.Item2 == shipEndCoordinates.Item2)
            {
                var horizontalShipLength = Math.Abs(shipStartCoordinates.Item1 - shipEndCoordinates.Item1) + 1;
                if (ship.Size != horizontalShipLength)
                {
                    return false;
                }

                if (shipStartCoordinates.Item1 > shipEndCoordinates.Item1)
                {
                    shipStartCoordinates.Item1 = shipEndCoordinates.Item1;
                    shipEndCoordinates.Item1 = shipStartCoordinates.Item1;
                }
                
                if (IsPlayer1Turn())
                {
                    if (CanPlaceShipAtCoordinates(shipStartCoordinates, ship, P1Board))
                    {
                        if (checkOnly)
                        {
                            return true;
                        }
                        foreach (var xValue in Enumerable.Range(shipStartCoordinates.Item1, horizontalShipLength))
                        {
                            P1Board[shipStartCoordinates.Item2 - 1, xValue - 1].ShipId = ship.GameShipId;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (CanPlaceShipAtCoordinates(shipStartCoordinates, ship, P2Board))
                    {
                        if (checkOnly)
                        {
                            return true;
                        }
                        foreach (var xValue in Enumerable.Range(shipStartCoordinates.Item1, horizontalShipLength))
                        {
                            P2Board[shipStartCoordinates.Item2 - 1, xValue - 1].ShipId = ship.GameShipId;
                        }
                    }
                    else
                    {
                        return false;
                    }
                    
                }
                return true;
            }
            return false;
        }

        private bool CheckIfSurroundingCoordinatesFree(int xValue, int yValue, BoardSquareState[,] board)
        {
            if (CanShipsTouch == CanShipsTouchEnum.Corner || CanShipsTouch == CanShipsTouchEnum.No)
            {
                // IN RIGHT SIDE OF BOARD
                if (xValue - 1 == BoardWidth - 1)
                {
                    // RIGHT TOP OF BOARD
                    if (yValue - 1 == 0)
                    {
                        if (board[yValue, xValue - 1].ShipId != null
                            || board[yValue - 1, xValue - 2].ShipId != null)
                        {
                            return false;
                        }

                        if (CanShipsTouch == CanShipsTouchEnum.No)
                        {
                            if (board[yValue, xValue - 2].ShipId != null)
                            {
                                return false;
                            }
                        }
                    }
                    
                    // RIGHT BOTTOM OF BOARD
                    if (yValue - 1 == BoardHeight - 1)
                    {
                        if (board[yValue - 1, xValue - 2].ShipId != null
                            || board[yValue - 2, xValue - 1].ShipId != null)
                        {
                            return false;
                        }
                        
                        if (CanShipsTouch == CanShipsTouchEnum.No)
                        {
                            if (board[yValue - 2, xValue - 2].ShipId != null)
                            {
                                return false;
                            }
                        }
                    }
                    
                    if (xValue - 1 == BoardWidth - 1 && yValue - 1 > 0 && yValue - 1 < BoardHeight - 1)
                    {
                        if (board[yValue - 1, xValue - 2].ShipId != null
                            || board[yValue - 2, xValue - 1].ShipId != null
                            || board[yValue, xValue - 1].ShipId != null)
                        {
                            return false;
                        }

                        if (CanShipsTouch == CanShipsTouchEnum.No)
                        {
                            if (board[yValue, xValue - 2].ShipId != null
                                || board[yValue - 2, xValue - 2].ShipId != null)
                            {
                                return false;
                            }
                        }
                    }
                }
                
                // IN LEFT SIDE OF BOARD
                if (xValue - 1 == 0)
                {
                    // LEFT TOP OF BOARD
                    if (yValue - 1 == 0)
                    {
                        if (board[yValue, xValue - 1].ShipId != null
                            || board[yValue - 1, xValue].ShipId != null)
                        {
                            return false;
                        }
                        if (CanShipsTouch == CanShipsTouchEnum.No)
                        {
                            if (board[yValue, xValue].ShipId != null)
                            {
                                return false;
                            }
                        }
                    }
                    
                    // LEFT BOTTOM OF BOARD
                    if (yValue - 1 == BoardHeight - 1)
                    {
                        if (board[yValue - 1, xValue].ShipId != null
                            || board[yValue - 2, xValue - 1].ShipId != null)
                        {
                            return false;
                        }
                        if (CanShipsTouch == CanShipsTouchEnum.No)
                        {
                            if (board[yValue - 2, xValue].ShipId != null)
                            {
                                return false;
                            }
                        }
                    }

                    if (xValue - 1 == 0 && yValue - 1 > 0 && yValue - 1 < BoardHeight - 1)
                    {
                        if (board[yValue - 1, xValue].ShipId != null
                            || board[yValue - 2, xValue - 1].ShipId != null
                            || board[yValue, xValue - 1].ShipId != null)
                        {
                            return false;
                        }
                        
                        if (CanShipsTouch == CanShipsTouchEnum.No)
                        {
                            if (board[yValue, xValue].ShipId != null
                                || board[yValue - 2, xValue].ShipId != null)
                            {
                                return false;
                            }
                        }
                    }
                }
                
                // IN TOP OF BOARD
                if (yValue - 1 == 0 && xValue - 1 > 0 && xValue - 1 < BoardWidth - 1)
                {
                    if (board[yValue - 1, xValue].ShipId != null
                        || board[yValue - 1, xValue - 2].ShipId != null
                        || board[yValue, xValue - 1].ShipId != null)
                    {
                        return false;
                    }
                    
                    if (CanShipsTouch == CanShipsTouchEnum.No)
                    {
                        if (board[yValue, xValue - 2].ShipId != null
                            || board[yValue, xValue].ShipId != null)
                        {
                            return false;
                        }
                    }
                }
                
                // IN BOTTOM OF BOARD
                if (yValue - 1 == BoardHeight - 1 && xValue - 1 > 0 && xValue - 1 < BoardWidth - 1)
                {
                    if (board[yValue - 1, xValue].ShipId != null
                        || board[yValue - 1, xValue - 2].ShipId != null
                        || board[yValue - 2, xValue - 1].ShipId != null)
                    {
                        return false;
                    }
                    
                    if (CanShipsTouch == CanShipsTouchEnum.No)
                    {
                        if (board[yValue - 2, xValue - 2].ShipId != null
                            || board[yValue - 2, xValue].ShipId != null)
                        {
                            return false;
                        }
                    }
                }
                
                // ALL CORNERS
                if (xValue - 1 != BoardWidth - 1 
                    && xValue - 1 != 0 
                    && yValue - 1 != 0 
                    && yValue - 1 != BoardHeight - 1)
                {
                    if (board[yValue - 2, xValue - 1].ShipId != null 
                        || board[yValue, xValue - 1].ShipId != null
                        || board[yValue - 1, xValue - 2].ShipId != null 
                        || board[yValue - 1, xValue].ShipId != null)
                    {
                        return false;
                    }
                    
                    if (CanShipsTouch == CanShipsTouchEnum.No)
                    {
                        // ALL DIRECTIONS
                        if (board[yValue, xValue].ShipId != null
                            || board[yValue - 2, xValue].ShipId != null
                            || board[yValue - 2, xValue - 2].ShipId != null
                            || board[yValue, xValue - 2].ShipId != null)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private bool CanPlaceShipAtCoordinates((int, int) shipStartCoordinates, GameShip ship, BoardSquareState[,] board, Boolean horizontalDirection = true)
        {
            if (horizontalDirection)
            {
                foreach (var xValue in Enumerable.Range(shipStartCoordinates.Item1, ship.Size))
                {
                    if (xValue - 1 > BoardWidth - 1)
                    {
                        return false;
                    }
                    
                    if (board[shipStartCoordinates.Item2 - 1, xValue - 1].ShipId != null)
                    {
                        return false;
                    }

                    if (CheckIfSurroundingCoordinatesFree(xValue, shipStartCoordinates.Item2, board) == false)
                    {
                        return false;
                    }
                }
                
                return true;
            }
            foreach (var yValue in Enumerable.Range(shipStartCoordinates.Item2, ship.Size))
            {
                if (yValue - 1 > BoardHeight - 1)
                {
                    return false;
                }
                
                if (board[yValue - 1, shipStartCoordinates.Item1 - 1].ShipId != null)
                {
                    return false;
                }
                
                if (CheckIfSurroundingCoordinatesFree(shipStartCoordinates.Item1, yValue, board) == false)
                {
                    return false;
                }
            }
            return true;
        }

        public (bool, Player?) MakeAMove(int x, int y, out string hitOrMissMessage)
        {
            using var db = new AppDbContext(_dbOptions);
            
            hitOrMissMessage = "";
            var changeTurn = true;
            if (x > BoardWidth || y > BoardHeight)
            {
                return (false, null);
            }

            if (Player1.PlayerTurn)
            {
                if (P2Board[y, x].Bomb == null)
                {
                    if (P2Board[y, x].ShipId != null)
                    {
                        hitOrMissMessage = "HIT!";
                        P2Board[y, x].Bomb = true;

                        if (CheckShipSunk(P2Board[y, x].ShipId, P2Board))
                        {
                            hitOrMissMessage += " AND SUNKEN!";
                        }

                        if (NextMoveAfterHit == NextMoveAfterHitEnum.SamePlayerTurn)
                        {
                            changeTurn = false;
                        }
                    }
                    else
                    {
                        hitOrMissMessage = "MISS!";
                        P2Board[y, x].Bomb = true;
                    }
                    
                    if (changeTurn)
                    {
                        Player1.PlayerTurn = !Player1.PlayerTurn;
                        Player2.PlayerTurn = !Player2.PlayerTurn;
                        
                        db.Entry(Player1).State = EntityState.Modified;
                        db.Entry(Player2).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    
                    return (true, Player1);
                }
                return (false, null);
            }

            if (P1Board[y, x].Bomb == null)
            {
                if (P1Board[y, x].ShipId != null)
                {
                    hitOrMissMessage = "HIT!";
                    P1Board[y, x].Bomb = true;

                    if (CheckShipSunk(P1Board[y, x].ShipId, P1Board))
                    {
                        hitOrMissMessage += " AND SUNKEN!";
                    }

                    if (NextMoveAfterHit == NextMoveAfterHitEnum.SamePlayerTurn)
                    {
                        changeTurn = false;
                    }
                }
                else
                {
                    hitOrMissMessage = "MISS!";
                    P1Board[y, x].Bomb = true;
                }
                        
                if (changeTurn)
                {
                    Player1.PlayerTurn = !Player1.PlayerTurn;
                    Player2.PlayerTurn = !Player2.PlayerTurn;
                    
                    db.Entry(Player1).State = EntityState.Modified;
                    db.Entry(Player2).State = EntityState.Modified;
                    db.SaveChanges();
                }
                        
                return (true, Player2);
            }
            return (false, null);
        }

        private bool CheckShipSunk(int? gameShipId, BoardSquareState[,] board)
        {
            using var db = new AppDbContext(_dbOptions);
            var ship = db.GameShips.Find(gameShipId);

            var hitCount = 0;

            for (var x = 0; x < BoardWidth; x++)
            {
                for (var y = 0; y < BoardHeight; y++)
                {
                    if (board[y, x].ShipId == gameShipId && board[y, x].Bomb == true)
                    {
                        hitCount++;
                    }
                }
            }
            if (hitCount == ship.Size)
            {
                ship.IsSunken = true;
                db.SaveChanges();
                return true;
            }
            return false;
        }
        
        public (bool, Player) CheckGameWin()
        {
            using var db = new AppDbContext(_dbOptions);
            IList<GameShip> shipList = db.GameShips.ToList();

            var player1ShipCount = 0;
            var player1ShipsSunken = 0;
            
            var player2ShipCount = 0;
            var player2ShipsSunken = 0;

            foreach (var gameShip in shipList)
            {
                if (gameShip.PlayerId == Player1.PlayerId)
                {
                    player1ShipCount++;
                    if (gameShip.IsSunken)
                    {
                        player1ShipsSunken++;
                    }
                }
                else if (gameShip.PlayerId == Player2.PlayerId)
                {
                    player2ShipCount++;
                    if (gameShip.IsSunken)
                    {
                        player2ShipsSunken++;
                    }
                }
            }
            
            if (player1ShipsSunken == player1ShipCount)
            {
                return (true, Player2);
            }
            if (player2ShipsSunken == player2ShipCount)
            {
                return (true, Player1);
            }
            
            return (false, null)!;
        }
        
        public (bool, bool) UndoLastMove()
        {
            var changeTurn = true;
            
            using var db = new AppDbContext(_dbOptions);
            
            var playerBoardState = db.PlayerBoardStates.AsEnumerable().LastOrDefault(p => p.PlayerId == Player1.PlayerId || p.PlayerId == Player2.PlayerId);
            var gameBoardState = db.GameSaves.AsEnumerable().LastOrDefault(gs => gs.GameId == Game.GameId && gs.CreatedByUser == false);

            if (playerBoardState == null)
            {
                return (false, false);
            }

            var boardState = JsonSerializer.Deserialize<BoardStateDTO>(playerBoardState.BoardState);
            
            IList<GameShip> gameShipList = db.GameShips.ToList();

            foreach (var (gameShipFromStateId, isSunken) in boardState.GameShipsStates!)
            {
                var gameShipId = int.Parse(gameShipFromStateId);
                foreach (var gameShipFromDb in gameShipList)
                {
                    if (gameShipFromDb.GameShipId == gameShipId)
                    {
                        gameShipFromDb.IsSunken = isSunken;
                        db.SaveChanges();
                    }
                }
            }
            
            if (playerBoardState.PlayerId == Player1.PlayerId)
            {
                SetPlayer1Board(new BoardSquareState[boardState.BoardHeight, boardState.BoardWidth]);
                
                for (var x = 0; x < boardState.BoardWidth; x++)
                {
                    for (var y = 0; y < boardState.BoardHeight; y++)
                    {
                        P1Board[y, x] = boardState.PlayerBoard[y][x];
                    }
                }
                
                if (!IsPlayer1Turn() && NextMoveAfterHit == NextMoveAfterHitEnum.SamePlayerTurn)
                {
                    changeTurn = false;
                }
            }
            else if (playerBoardState.PlayerId == Player2.PlayerId)
            {
                SetPlayer2Board(new BoardSquareState[boardState.BoardHeight, boardState.BoardWidth]);
                
                for (var x = 0; x < boardState.BoardWidth; x++)
                {
                    for (var y = 0; y < boardState.BoardHeight; y++)
                    {
                        P2Board[y, x] = boardState.PlayerBoard[y][x];
                    }
                }
                
                if (IsPlayer1Turn() && NextMoveAfterHit == NextMoveAfterHitEnum.SamePlayerTurn)
                {
                    changeTurn = false;
                }
            }
            db.GameSaves.Remove(gameBoardState);
            db.PlayerBoardStates.Remove(playerBoardState);
            db.SaveChanges();

            if (changeTurn)
            {
                Player1.PlayerTurn = !Player1.PlayerTurn;
                Player2.PlayerTurn = !Player2.PlayerTurn;
                
                db.Entry(Player1).State = EntityState.Modified;
                db.Entry(Player2).State = EntityState.Modified;
                db.SaveChanges();
                return (true, true);
            }

            return (true, false);
        }
        
        public string GetJsonBoardState(Player playerWhoMadeMove)
        {
            using var db = new AppDbContext(_dbOptions);
            
            Dictionary<string, bool> gameShipsStates = new Dictionary<string, bool>();

            var gameShips =
                db.GameShips.Where(gs => gs.PlayerId == Player1.PlayerId || gs.PlayerId == Player2.PlayerId);

            foreach (var gameShip in gameShips)
            {
                gameShipsStates[gameShip.GameShipId.ToString()] = gameShip.IsSunken;
            }

            var dto = new BoardStateDTO()
            {
                PlayerBoard = new BoardSquareState[BoardHeight][],
                BoardHeight = BoardHeight,
                BoardWidth = BoardWidth,
                GameShipsStates = gameShipsStates
            };
            
            for (var i = 0; i < dto.PlayerBoard.Length; i++)
            {
                dto.PlayerBoard[i] = new BoardSquareState[dto.BoardWidth];
            }

            for (var x = 0; x < dto.BoardWidth; x++)
            {
                for (var y = 0; y < dto.BoardHeight; y++)
                {
                    if (playerWhoMadeMove.PlayerId == Player1.PlayerId)
                    {
                        dto.PlayerBoard[y][x] = P1Board[y, x];
                    }
                    else
                    {
                        dto.PlayerBoard[y][x] = P2Board[y, x];
                    }
                }
            }
            return JsonSerializer.Serialize(dto);
        }

        public string GetSerializedGameState()
        {
            var jsonOptions = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            
            using var db = new AppDbContext(_dbOptions);
            
            Dictionary<string, bool> gameShipsStates = new Dictionary<string, bool>();

            foreach (var gameShip in db.GameShips)
            {
                gameShipsStates[gameShip.GameShipId.ToString()] = gameShip.IsSunken;
            }
            
            var state = new GameState
            {
                PlayerOneMove = IsPlayer1Turn(),
                GamePhase = Game.GamePhase,
                BoardHeight = P1Board.GetLength(0),
                BoardWidth = P1Board.GetLength(1),
                Player1Board = new BoardSquareState[BoardHeight][],
                Player2Board = new BoardSquareState[BoardHeight][],
                GameShipsStates = gameShipsStates
            };
            
            for (var i = 0; i < state.Player1Board.Length; i++)
            {
                state.Player1Board[i] = new BoardSquareState[state.BoardWidth];
                state.Player2Board[i] = new BoardSquareState[state.BoardWidth];
            }

            for (var x = 0; x < state.BoardWidth; x++)
            {
                for (var y = 0; y < state.BoardHeight; y++)
                {
                    state.Player1Board[y][x] = P1Board[y, x];
                    state.Player2Board[y][x] = P2Board[y, x];
                }
            }
            
            return JsonSerializer.Serialize(state, jsonOptions);
        }

        public void SetGameStateFromJsonString(string jsonString, bool setFromUserSave = true)
        {
            var state = JsonSerializer.Deserialize<GameState>(jsonString);
            
            using var db = new AppDbContext(_dbOptions);
            
            IList<GameShip> gameShipList = db.GameShips.ToList();

            if (setFromUserSave)
            {
                foreach (var (gameShipFromStateId, isSunken) in state.GameShipsStates!)
                {
                    var gameShipId = int.Parse(gameShipFromStateId);
                    foreach (var gameShipFromDb in gameShipList)
                    {
                        if (gameShipFromDb.GameShipId == gameShipId)
                        {
                            gameShipFromDb.IsSunken = isSunken;
                            db.SaveChanges();
                        }
                    }
                }
                db.GameSaves.RemoveRange(db.GameSaves.Where(x => x.CreatedByUser == false));
                db.SaveChanges();
                
                var playerBoardStates = db.PlayerBoardStates.Where(pbs => pbs.PlayerId == Player1.PlayerId || pbs.PlayerId == Player2.PlayerId).ToList();
            
                foreach (var playerBoardState in playerBoardStates)
                {
                    db.Remove(playerBoardState);
                    db.SaveChanges();
                }
            }

            if (state.PlayerOneMove)
            {
                Player1.PlayerTurn = true;
                Player2.PlayerTurn = false;
            }
            else
            {
                Player1.PlayerTurn = false;
                Player2.PlayerTurn = true;
            }

            SetPlayer1Board(new BoardSquareState[state.BoardHeight, state.BoardWidth]);
            SetPlayer2Board(new BoardSquareState[state.BoardHeight, state.BoardWidth]);
            
            BoardHeight = state.BoardHeight;
            BoardWidth = state.BoardWidth;
            Game.GamePhase = state.GamePhase;

            for (var x = 0; x < state.BoardWidth; x++)
            {
                for (var y = 0; y < state.BoardHeight; y++)
                {
                    P1Board[y, x] = state.Player1Board[y][x];
                    P2Board[y, x] = state.Player2Board[y][x];
                }
            }
        }
    }
}
