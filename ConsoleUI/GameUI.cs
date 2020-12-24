using System;
using Domain;
using GameEngine;

namespace ConsoleUI
{
    public static class GameUI
    {
        private static readonly string _verticalSeparator = "|";
        private static readonly string _horizontalSeparator = "-";
        private static readonly string _centerSeparator = "+";

        public static void PrintBoard(GameLogic game, BoardSquareState[,] gameBoard, bool withShips, int cursorLeft, int cursorTop, bool switchNames, bool printSingleBoard = false)
        { 
            var crTop = cursorTop;
            var rowNumber1 = 1;
            var rowNumber2 = 1;
             
            string tabs = new string('\t', 1);
            
            var board = gameBoard;
            
            PrintColumnMarkings(game, cursorLeft, cursorTop);

            for (var yIndex = 0; yIndex < game.BoardHeight; yIndex++)
            {
                if (yIndex == 0)
                {
                    crTop++;
                    PrintBoardClosings(game, cursorLeft, crTop);
                }

                var line = " ";
                
                for (var xIndex = 0; xIndex < game.BoardWidth; xIndex++)
                {
                    if (xIndex == 0)
                    {
                        if (rowNumber1 >= 10)
                        {
                            line += $"{rowNumber1}  " + _verticalSeparator;
                        }
                        else
                        {
                            line += $" {rowNumber1}  " +  _verticalSeparator;    
                        }
                        rowNumber1++;
                    }

                    line = line + GetSingleBoardSquareState(board[yIndex, xIndex], withShips);
                    
                    if (xIndex < game.BoardWidth + 1)
                    {
                        line += _verticalSeparator;
                    }

                    if (xIndex == game.BoardWidth - 1)
                    {
                        if (rowNumber2 >= 10)
                        {
                            line += $"  {rowNumber2}";
                        }
                        else
                        {
                            line += $"  {rowNumber2}";   
                        }
                        rowNumber2++;
                    }
                }
                
                crTop += 1;
                
                Console.SetCursorPosition(cursorLeft, crTop);
                var rowNumber = line.Substring(1,2);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(rowNumber + " ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(line.Substring(3, line.Length - 5));
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(rowNumber);
                Console.ForegroundColor = ConsoleColor.Red;

                crTop += 1;
                
                if (yIndex < game.BoardHeight)
                {
                    PrintBoardClosings(game, cursorLeft, crTop);
                }
            }
            
            PrintColumnMarkings(game, cursorLeft, crTop + 1);
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.SetCursorPosition(0, crTop + 3);
            if (switchNames)
            {
                Console.WriteLine(!game.IsPlayer1Turn()
                    ? $"{tabs} {game.Player1.Name.ToUpper()}'S BOARD"
                    : $"{tabs} {game.Player2.Name.ToUpper()}'S BOARD");
            }
            else
            {
                Console.WriteLine(game.IsPlayer1Turn()
                    ? $"{tabs} {game.Player1.Name.ToUpper()}'S BOARD"
                    : $"{tabs} {game.Player2.Name.ToUpper()}'S BOARD");
            }

            if (!printSingleBoard)
            {
                Console.SetCursorPosition(Console.LargestWindowWidth / 2, crTop + 3);

                if (switchNames)
                {
                    Console.WriteLine(!game.IsPlayer1Turn()
                        ? $"{tabs} {game.Player2.Name.ToUpper()}'S BOARD"
                        : $"{tabs} {game.Player1.Name.ToUpper()}'S BOARD");
                }
                else
                {
                    Console.WriteLine(game.IsPlayer1Turn()
                        ? $"{tabs} {game.Player2.Name.ToUpper()}'S BOARD"
                        : $"{tabs} {game.Player1.Name.ToUpper()}'S BOARD");
                    
                }
                
            }
            Console.SetCursorPosition(0, crTop + 4);
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Red;
        }

        private static void PrintBoardClosings(GameLogic game, int cursorLeft, int cursorTop)
        {
            var line = "     ";
            for (var xIndex = 0; xIndex < game.BoardWidth; xIndex++)
            {
                if (xIndex == 0)
                {
                    line += _centerSeparator;
                }
                    
                line = line + _horizontalSeparator+ _horizontalSeparator+ _horizontalSeparator;
                    
                if (xIndex < game.BoardWidth + 1)
                {
                    line += _centerSeparator;
                }
            }
            
            Console.SetCursorPosition(cursorLeft, cursorTop);
            Console.WriteLine(line);
            
        }

        private static string GetLetter(int elementNr)
        {
            char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

            for (int i = 0; i < alpha.Length; i++)
            {
                if (elementNr == i)
                {
                    return alpha[i].ToString();
                }
            }
            return "";
        }

        private static void PrintColumnMarkings(GameLogic game, int cursorLeft, int cursorTop)
        {
            var letters = "     ";
            for (var xIndex = 0; xIndex < game.BoardWidth; xIndex++)
            {
                if (xIndex == 0)
                {
                    letters += " ";
                }
                        
                letters = letters + " " + GetLetter(xIndex) + " ";
                
                if (xIndex < game.BoardWidth + 1)
                {
                    letters += " ";
                }
                
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.SetCursorPosition(cursorLeft, cursorTop);
            Console.WriteLine(letters);
            Console.ForegroundColor = ConsoleColor.Red;
        }

        public static string GetSingleBoardSquareState(BoardSquareState state, bool withShips)
        {
            if (withShips)
            {
                if (state.ShipId != null && state.Bomb == null)
                {
                    return " S " ; // SHIP
                }
                if (state.ShipId == null && state.Bomb == true)
                {
                    return " O "; // MISS️
                }
                if (state.ShipId != null && state.Bomb == true)
                {
                    return " X "; // HIT
                }
                return "   ";
            }

            if (state.ShipId == null && state.Bomb == true)
            {
                return " O "; // MISS
            }
            if (state.ShipId != null && state.Bomb == true)
            {
                return " X "; // HIT
            }
            return "   ";
        }
    }
}
