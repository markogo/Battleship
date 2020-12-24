using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Domain.Enums;
using GameEngine;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace WebApp.Pages.GamePlay
{
    public class Index : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly DAL.AppDbContext _context;

        public Index(ILogger<IndexModel> logger, DAL.AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [RegularExpression(@"^[A-Za-z]{1}(,[0-9]+)-[A-Za-z]{1}(,[0-9]+$)",
            ErrorMessage = "Please enter the coordinates correctly!")]
        
        [BindProperty(SupportsGet = true)] public string? ShipCoordinates { get; set; }

        public Ship? ShipBeingPlaced { get; set; }

        public GameLogic GameLogic { get; set; } = new GameLogic();
        
        public string? ActionMessage { get; set; }
        
        public bool TurnChange;
        
        public Player? Winner { get; set; }

        public bool? LoadGame { get; set; }
        
        public bool? UndoMove { get; set; }
        
        public bool? SaveGame { get; set; } 
        
        [BindProperty(SupportsGet = true)] public GameSave? GameSave { get; set; }

        public IList<GameShip>? GameShips { get; set; }
        
        public IList<GameOptionShip>? GameOptionShips { get; set; }

        [BindProperty(SupportsGet = true)] public Game? Game { get; set; }

        private void SetShipToBePlaceOnBoard(IEnumerable<GameOptionShip> gameOptionShips)
        {
            foreach (var optionShip in gameOptionShips.OrderByDescending(s => s.Ship!.Size))
            {
                ShipBeingPlaced = optionShip.Ship;
                        
                var shipCount = 0;
                foreach (var gameShip in GameShips!)
                {
                    if (gameShip.Name == ShipBeingPlaced!.Name)
                    {
                        shipCount++;
                    }
                }

                if (shipCount != optionShip.Amount)
                {
                    break;
                }
            }
        }

        public int GetTotalAmountOfShipsForOnePlayer()
        {
            var result = 0;
            foreach (var gameOptionShip in GameOptionShips!)
            {
                result += gameOptionShip.Amount;
            }

            return result;
        }

        public async Task<IActionResult> OnGetAsync(int gameId, int? x, int? y, bool? placeShipsRandomly, bool? saveGame, int? gameSaveId, bool? undoMove)
        {
            Game = await _context.Games
                .Where(g => g.GameId == gameId)
                .Include(g => g.GameOption)
                .ThenInclude(g => g!.GameOptionShips)
                .ThenInclude(g => g.Ship)
                .Include(g => g.Player1)
                .Include(g => g.Player2)
                .FirstOrDefaultAsync();

            if (Game != null)
            {
                GameShips = Game.Player1!.PlayerTurn ? _context.GameShips.Where(gs => gs.PlayerId == Game.Player1Id).ToList() : _context.GameShips.Where(gs => gs.PlayerId == Game.Player2Id).ToList();
                
                GameOptionShips = _context.GameOptionShips.Where(gos => gos.GameOptionId == Game.GameOptionId).ToList();

                Game.Player1.PlayerBoardStates =
                    _context.PlayerBoardStates.Where(pb => pb.PlayerId == Game.Player1Id).OrderByDescending(o => o.PlayerBoardStateId).Take(1).ToList();
                
                Game.Player2!.PlayerBoardStates =
                    _context.PlayerBoardStates.Where(pb => pb.PlayerId == Game.Player2Id).OrderByDescending(o => o.PlayerBoardStateId).Take(1).ToList();

                Game.GameSaves = _context.GameSaves.Where(gs => gs.CreatedByUser == false && gs.GameId == Game.GameId)
                    .OrderByDescending(o => o.GameSaveId).Take(1).ToList();
                
                GameLogic = new GameLogic(Game.GameOption!)
                {
                    Player1 = Game.Player1, Player2 = Game.Player2, Game = Game
                };
                
                if (Game.GameSaves!.Any() && gameSaveId == null)
                {
                    GameLogic.SetGameStateFromJsonString(Game.GameSaves!.LastOrDefault()?.SerializedGame!, false);
                }

                if (undoMove == true)
                {
                    UndoMove = true;
                    var (undoSuccessful, changeTurn) = GameLogic.UndoLastMove();

                    ActionMessage = !undoSuccessful ? "NO MOVES MADE YET" : "UNDO SUCCESSFUL";

                    if (changeTurn)
                    {
                        TurnChange = true;
                    }
                }

                if (gameSaveId != null)
                {
                    GameSave saveFile = await _context.GameSaves.FindAsync(gameSaveId);
                    GameLogic.SetGameStateFromJsonString(saveFile.SerializedGame!);
                    LoadGame = true;
                    ActionMessage = "GAME LOADED!";
                    
                    var gameState = new GameSave()
                    {
                        CreatedByUser = false,
                        SerializedGame = GameLogic.GetSerializedGameState(),
                        GameId = gameId
                    };

                    _context.GameSaves.Add(gameState);
                    await _context.SaveChangesAsync();
                }

                if (GameLogic.Game.GamePhase == GamePhaseEnum.PlacingShips)
                {
                    SetShipToBePlaceOnBoard(GameOptionShips);

                    if (placeShipsRandomly == true && !GameShips!.Any())
                    {
                        GameLogic.AutomatePlacingShipsOnBoard(Game.GameOption!.GameOptionShips!, null);
                        
                        if (GameLogic.IsPlayer1Turn())
                        {
                            Game.Player1.PlayerTurn = false;
                            Game.Player2.PlayerTurn = true;
                            await _context.SaveChangesAsync();
                            SetShipToBePlaceOnBoard(GameOptionShips);
                        }
                        else
                        {
                            Game.Player1.PlayerTurn = true;
                            Game.Player2.PlayerTurn = false;
                            Game.GamePhase = GamePhaseEnum.Bombing;
                            await _context.SaveChangesAsync();
                        }

                        GameShips = Game.Player1.PlayerTurn ? _context.GameShips.Where(gs => gs.PlayerId == Game.Player1Id).ToList() : _context.GameShips.Where(gs => gs.PlayerId == Game.Player2Id).ToList();
                        var gameState = new GameSave()
                        {
                            CreatedByUser = false,
                            SerializedGame = GameLogic.GetSerializedGameState(),
                            GameId = gameId
                        };

                        _context.GameSaves.Add(gameState);
                        await _context.SaveChangesAsync();
                    }

                    if (ModelState.IsValid && ShipCoordinates != null)
                    {
                        // SHIP PLACEMENT
                        var coordinates = GameLogic.UserInputToCoordinates(ShipCoordinates);
                        if (coordinates.conversionSuccessful)
                        {
                            GameShip gameShipOnBoard = new GameShip
                            {
                                Size = ShipBeingPlaced!.Size,
                                Name = ShipBeingPlaced.Name,
                                IsSunken = false,
                                PlayerId = GameLogic.IsPlayer1Turn() ? GameLogic.Player1.PlayerId : GameLogic.Player2.PlayerId
                            };
                            
                            if (GameLogic.PlaceShipOnBoard(gameShipOnBoard, coordinates.coordinates[0].ToValueTuple(),
                                coordinates.coordinates[1].ToValueTuple(), true))
                            {
                                _context.GameShips.Add(gameShipOnBoard);
                                await _context.SaveChangesAsync();
                                GameShips.Add(gameShipOnBoard);
                                
                                if (GameLogic.PlaceShipOnBoard(gameShipOnBoard,
                                    coordinates.coordinates[0].ToValueTuple(),
                                    coordinates.coordinates[1].ToValueTuple()))
                                {
                                    SetShipToBePlaceOnBoard(GameOptionShips);
                                    ShipCoordinates = null;
                                    await _context.SaveChangesAsync();

                                    if (GameLogic.IsPlayer1Turn() &&
                                        GameShips.Count == GetTotalAmountOfShipsForOnePlayer() ||
                                        !GameLogic.IsPlayer1Turn() &&
                                        GameShips.Count == GetTotalAmountOfShipsForOnePlayer())
                                    {
                                        Game.Player1.PlayerTurn = false;
                                        Game.Player2.PlayerTurn = true;
                                        await _context.SaveChangesAsync();
                                        GameShips = Game.Player1.PlayerTurn ? _context.GameShips.Where(gs => gs.PlayerId == Game.Player1Id).ToList() : _context.GameShips.Where(gs => gs.PlayerId == Game.Player2Id).ToList();
                                        SetShipToBePlaceOnBoard(GameOptionShips);
                                        if (!GameLogic.IsPlayer1Turn() && GameShips.Count == GetTotalAmountOfShipsForOnePlayer())
                                        {
                                            Game.Player1.PlayerTurn = true;
                                            Game.Player2.PlayerTurn = false;
                                            Game.GamePhase = GamePhaseEnum.Bombing;
                                            await _context.SaveChangesAsync();
                                        }
                                    }
                                    
                                    var gameState = new GameSave()
                                    {
                                        CreatedByUser = false,
                                        SerializedGame = GameLogic.GetSerializedGameState(),
                                        GameId = gameId
                                    };

                                    _context.GameSaves.Add(gameState);
                                    await _context.SaveChangesAsync();
                                }
                            }
                        }
                    }
                }

                if (GameLogic.Game.GamePhase == GamePhaseEnum.Bombing)
                {
                    if (saveGame == true)
                    {
                        var defaultSaveName = "SaveFile_" + DateTime.Now.ToString("dd-MM-yyyy_HH:mm:ss");
                        
                        if (string.IsNullOrWhiteSpace(GameSave!.Name))
                        {
                            GameSave.Name = defaultSaveName;
                        }

                        GameSave.CreatedByUser = true;
                        GameSave.GameId = gameId;
                        GameSave.SerializedGame = GameLogic.GetSerializedGameState();

                        _context.GameSaves.Add(GameSave);
                        _context.SaveChanges();
                        
                        GameSave.Name = null;
                        SaveGame = true;
                        ActionMessage = "GAME SAVED!";
                    }
                    if (x != null && y != null)
                    {
                        PlayerBoardState playerBoardState = new PlayerBoardState
                        {
                            PlayerId = (Game.Player1.PlayerTurn ? Game.Player2 : Game.Player1!).PlayerId,
                            CreatedAt = DateTime.Now,
                            BoardState = GameLogic.GetJsonBoardState(Game.Player1.PlayerTurn ? Game.Player2 : Game.Player1)
                        };
                        
                        var (moveSuccessful, _) = GameLogic.MakeAMove(x.Value, y.Value, out string hitOrMissMessage);

                        if (!moveSuccessful)
                        {
                            ActionMessage = "BOMBING UNSUCCESSFUL!";
                        }
                        else
                        {
                            _context.PlayerBoardStates.Add(playerBoardState);
                            _context.SaveChanges();

                            if (Game.GameOption!.NextMoveAfterHitEnum == NextMoveAfterHitEnum.SamePlayerTurn && (hitOrMissMessage == "HIT!" || hitOrMissMessage == "HIT! AND SUNKEN!"))
                            {
                                ActionMessage = hitOrMissMessage;
                            }
                            else
                            {
                                ActionMessage = hitOrMissMessage;
                                TurnChange = true;
                            }
                            
                            var gameState = new GameSave()
                            {
                                CreatedByUser = false,
                                SerializedGame = GameLogic.GetSerializedGameState(),
                                GameId = gameId
                            };

                            _context.GameSaves.Add(gameState);
                            await _context.SaveChangesAsync();
                            
                            var (gameIsOver, winner) = GameLogic.CheckGameWin();
                            if (gameIsOver)
                            {
                                Winner = winner;
                                Game.GamePhase = GamePhaseEnum.GameOver;
                                await _context.SaveChangesAsync();
                            }
                        }
                    }
                }
            }
            return Page();
        }
    }
}